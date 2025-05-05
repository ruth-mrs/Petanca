using UnityEngine;
using WiimoteApi;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CameraController : MonoBehaviour
{
    // Removidos los parámetros de sensibilidad de mouse ya que no lo usaremos
    public float wiimoteMovementSpeed = 8.0f;  // Aumentado para un movimiento más rápido
    public float wiimoteRotationSpeed = 90.0f; // Para el giro con botones 1 y 2
    
    private float yRotation = 0f;
    private float xRotation = 0f;

    Wiimote mote;
    private bool isRecordingAccel = false;
    private List<Vector3> accelReadings = new List<Vector3>();
    private float recordingDuration = 2.0f;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        // Mantenemos el cursor bloqueado para mejor experiencia
        Cursor.lockState = CursorLockMode.Locked;
        
        WiimoteManager.FindWiimotes();
        if (WiimoteManager.Wiimotes.Count > 0)
        {
            mote = WiimoteManager.Wiimotes[0];
            mote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            mote.Accel.CalibrateAccel(AccelCalibrationStep.A_BUTTON_UP);
            
            // Verificar si hay datos de calibración disponibles 
            float[] zeroPoints = mote.Accel.GetAccelZeroPoints();
            bool calibrationAvailable = (zeroPoints != null && zeroPoints.Length == 3);
            
            Debug.Log("Estado de calibración del Wiimote: " + (calibrationAvailable ? "Disponible" : "No disponible"));
            
            // Encender LED para indicar que está listo
            mote.SendPlayerLED(true, false, false, false);
            
            Debug.Log("Wiimote conectado! Usa cruceta para movimiento, botones 1 y 2 para girar.");
        }
        else
        {
            Debug.LogError("No se encontró ningún Wiimote. Por favor conecta un Wiimote e intenta nuevamente.");
        }
    }

    void Update()
    {
        // Verificar que el mando esté conectado
        if (mote == null)
        {
            Debug.LogWarning("Wiimote no conectado o perdido. Intentando reconectar...");
            WiimoteManager.FindWiimotes();
            if (WiimoteManager.Wiimotes.Count > 0)
            {
                mote = WiimoteManager.Wiimotes[0];
                mote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            }
            return;
        }
        
        // Leer datos del Wiimote cada frame
        mote.ReadWiimoteData();
        
        if (!isRecordingAccel)
        {
            // --------------- MANEJO DE MOVIMIENTO ---------------
            moveDirection = Vector3.zero;
            
            // Movimiento con cruceta
            if (mote.Button.d_up)
            {
                moveDirection += transform.forward;
                Debug.Log("▲ Moviendo ADELANTE");
            }
            
            if (mote.Button.d_down)
            {
                moveDirection -= transform.forward;
                Debug.Log("▼ Moviendo ATRÁS");
            }
            
            if (mote.Button.d_left)
            {
                moveDirection -= transform.right;
                Debug.Log("◄ Moviendo IZQUIERDA");
            }
            
            if (mote.Button.d_right)
            {
                moveDirection += transform.right;
                Debug.Log("► Moviendo DERECHA");
            }
            
            // Movimiento vertical con + y -
            if (mote.Button.plus)
            {
                moveDirection += Vector3.up;
                Debug.Log("+ Moviendo ARRIBA");
            }
            
            if (mote.Button.minus)
            {
                moveDirection += Vector3.down;
                Debug.Log("- Moviendo ABAJO");
            }
            
            // --------------- MANEJO DE ROTACIÓN ---------------
            // Usar botones 1 y 2 para rotar
            if (mote.Button.one)
            {
                yRotation -= wiimoteRotationSpeed * Time.deltaTime; // Girar izquierda
                Debug.Log("1 Girando IZQUIERDA");
            }
            
            if (mote.Button.two)
            {
                yRotation += wiimoteRotationSpeed * Time.deltaTime; // Girar derecha
                Debug.Log("2 Girando DERECHA");
            }
            
            // Mantén los límites de rotación
            if (yRotation > 360) yRotation -= 360;
            if (yRotation < 0) yRotation += 360;
            
            // Aplicar el movimiento si hay dirección
            // Aplicar el movimiento si hay dirección
            if (moveDirection.magnitude > 0.1f)
            {
                // Normalizar solo si la magnitud es mayor que 1
                if (moveDirection.magnitude > 1f)
                {
                    moveDirection.Normalize();
                }
                
                // Aumentar SIGNIFICATIVAMENTE la velocidad para ver el efecto
                float actualSpeed = wiimoteMovementSpeed * 3f; // Triplicamos la velocidad
                
                // Usar transform.Translate en lugar de modificar directamente la posición
                transform.Translate(moveDirection * actualSpeed * Time.deltaTime);
                
                // Imprimir posición actual para verificar
                Debug.Log("POSICIÓN ACTUAL: " + transform.position);
            }
            
            // Aplicar rotación
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            
            // Botón Home para reiniciar posición
            if (mote.Button.home)
            {
                transform.position = new Vector3(0, 1, 0);
                yRotation = 0;
                xRotation = 0;
                Debug.Log("⌂ Posición reiniciada");
            }
            
            // Iniciar grabación de aceleración con botón A
            if (mote.Button.a)
            {
                Debug.Log("Botón A presionado - Iniciando grabación de aceleración...");
                StartCoroutine(RecordAccelerationData());
            }
        }
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

}