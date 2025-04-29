using UnityEngine;
using WiimoteApi;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float verticalClamp = 80f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    Wiimote mote;
    private bool isRecordingAccel = false;
    private List<Vector3> accelReadings = new List<Vector3>();
    private float recordingDuration = 2.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  
        WiimoteManager.FindWiimotes(); 
        mote = WiimoteManager.Wiimotes[0];
        mote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16); 
        
        // Corregido: CalibrateAccel (una 'l') y AccelCalibrationStep.A_BUTTON_UP (BUTTON, no BUTON)
        mote.Accel.CalibrateAccel(AccelCalibrationStep.A_BUTTON_UP);
        
        // Verificar si hay datos de calibración disponibles 
        float[] zeroPoints = mote.Accel.GetAccelZeroPoints();
        bool calibrationAvailable = (zeroPoints != null && zeroPoints.Length == 3);
        
        // Enviar datos a la consola
        Debug.Log("Estado de calibración del Wiimote: " + (calibrationAvailable ? "Disponible" : "No disponible"));
        
        // Guardar datos en archivo
        string calibrationPath = Application.persistentDataPath + "/wiimote_calibration.txt";
        string calibrationData = "Estado de calibración del Wiimote: " + (calibrationAvailable ? "Disponible" : "No disponible") + "\n";
        
        if (calibrationAvailable)
        {
            calibrationData += "Datos de accel_calib:\n";
            for (int i = 0; i < 3; i++) 
            {
                calibrationData += "Paso " + i + ": ";
                for (int j = 0; j < 3; j++)
                {
                    calibrationData += mote.Accel.accel_calib[i, j] + " ";
                }
                calibrationData += "\n";
            }
            
            calibrationData += "\nPuntos cero:\n";
            calibrationData += "X0: " + zeroPoints[0] + "\n";
            calibrationData += "Y0: " + zeroPoints[1] + "\n";
            calibrationData += "Z0: " + zeroPoints[2] + "\n";
            
            // También obtener datos calibrados actuales
            float[] calibratedData = mote.Accel.GetCalibratedAccelData();
            calibrationData += "\nDatos calibrados actuales:\n";
            calibrationData += "X: " + calibratedData[0] + "\n";
            calibrationData += "Y: " + calibratedData[1] + "\n";
            calibrationData += "Z: " + calibratedData[2] + "\n";
            
            Debug.Log("Datos de calibración guardados en: " + calibrationPath);
        }
        
        System.IO.File.WriteAllText(calibrationPath, calibrationData);
        mote.SendPlayerLED(true, false, false, false);

        // Después de la calibración, inicia la lectura de aceleración durante 2 segundos
        StartCoroutine(RecordAccelerationData());
    }

   IEnumerator RecordAccelerationData()
{
    // Esperar un momento para que se estabilice la calibración
    yield return new WaitForSeconds(0.5f);
    
    Debug.Log("Comenzando a registrar datos de aceleración durante " + recordingDuration + " segundos...");
    accelReadings.Clear();
    isRecordingAccel = true;
    
    // Asegurar que estamos en modo de reporte correcto antes de registrar
    mote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
    
    // Encender todos los LEDs para indicar inicio de grabación
    mote.SendPlayerLED(true, true, true, true);
    yield return new WaitForSeconds(0.1f);
    
    float startTime = Time.time;
    int sampleCount = 0;
    
    // Bucle de grabación con tiempo fijo
    while (Time.time - startTime < recordingDuration)
    {
        // Forzar la lectura de datos
        mote.ReadWiimoteData();
        
        // Intentar obtener datos varias veces si es necesario
        float[] calibratedData = mote.Accel.GetCalibratedAccelData();
        if (calibratedData != null && calibratedData.Length == 3)
        {
            Vector3 accelVector = new Vector3(calibratedData[0], calibratedData[1], calibratedData[2]);
            accelReadings.Add(accelVector);
            sampleCount++;
            
            // Mostrar cada 10 muestras para no saturar la consola
            if (sampleCount % 10 == 0)
            {
                Debug.Log("Muestra " + sampleCount + " - Aceleración: X=" + calibratedData[0].ToString("F3") + 
                         " Y=" + calibratedData[1].ToString("F3") + 
                         " Z=" + calibratedData[2].ToString("F3"));
            }
        }
        
        // Esperar un tiempo fijo - 20ms = 50 muestras por segundo
        yield return new WaitForSeconds(0.02f);
    }
    
    // Indicar fin de grabación
    mote.SendPlayerLED(true, false, false, false);
    isRecordingAccel = false;
    
    Debug.Log("Registro finalizado. Se capturaron " + accelReadings.Count + " muestras.");
    
    // Guardar todos los datos recolectados
    if (accelReadings.Count > 0)
    {
        SaveAccelerationData();
    }
    else
    {
        Debug.LogError("No se capturó ninguna muestra de aceleración. Verifica que el Wiimote esté funcionando correctamente.");
    }
}

void SaveAccelerationData()
{
    string dataPath = Application.persistentDataPath + "/wiimote_acceleration_data.txt";
    StringBuilder sb = new StringBuilder();
    
    sb.AppendLine("Datos de aceleración recolectados durante " + recordingDuration + " segundos:");
    sb.AppendLine("Total de muestras: " + accelReadings.Count);
    sb.AppendLine("Ruta del archivo: " + dataPath);  // Añadir la ruta para encontrarlo fácilmente
    sb.AppendLine();
    sb.AppendLine("Tiempo (s)\tX\tY\tZ\tMagnitud");
    
    for (int i = 0; i < accelReadings.Count; i++)
    {
        float timeStamp = (float)i / accelReadings.Count * recordingDuration;
        Vector3 accel = accelReadings[i];
        float magnitude = accel.magnitude;
        
        sb.AppendLine(timeStamp.ToString("F3") + "\t" + 
                     accel.x.ToString("F3") + "\t" + 
                     accel.y.ToString("F3") + "\t" + 
                     accel.z.ToString("F3") + "\t" +
                     magnitude.ToString("F3"));
    }
    
    // El resto de tu código para estadísticas...
    
    System.IO.File.WriteAllText(dataPath, sb.ToString());
    Debug.Log("Datos de aceleración guardados en: " + dataPath);
    
    // También mostrar la ruta en la consola para facilitar encontrar el archivo
    Debug.Log("RUTA COMPLETA DEL ARCHIVO: " + System.IO.Path.GetFullPath(dataPath));
}

void Update()
{
    // Lectura de datos del mouse para controlar la cámara
    float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
    float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

    yRotation += mouseX;
    xRotation -= mouseY;
    xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

    transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    
    // Si presionas la tecla 'R', comienza a grabar datos
    if (Input.GetKeyDown(KeyCode.R) && !isRecordingAccel && mote != null)
    {
        Debug.Log("Iniciando grabación de aceleración con tecla R...");
        StartCoroutine(RecordAccelerationData());
    }
}
}