using UnityEngine;
using WiimoteApi;
using System.Collections;

public class Lanzamiento : MonoBehaviour
{
    public GameObject bola;
    public float fuerzaLanzamiento = 500f;
    public Wiimote wiimote;

    private Rigidbody rbBola;
    private GameObject mano;
    private bool bolaEnganchada = false;
    private bool bolaLiberada = false; // Flag para rastrear si la bola ha sido liberada
    private Quaternion rotacionInicialBrazo;
    private float suavizadoZ = 0f;
    private float velocidadMovimiento = 10f;
    private float rangoRotacion = 120f; // Rango de rotación del brazo
    private float velocidadRotacionMaxima = 100f; // Limitar la velocidad de rotación para evitar movimientos demasiado rápidos
    private float rotacionXActual = 0f; // Para mantener la rotación anterior de manera suave
    private Vector3 posicionManoAnterior;
    private Vector3 velocidadMano;
    private Vector3 velocidadObjetivo; // Velocidad objetivo para la bola al liberarla
    private float factorSuavizado = 0.2f; // Factor de suavizado para el acelerómetrovoid Update()

void Start()
{
    // Buscar el objeto de la bola
    if (bola == null)
    {
        bola = GameObject.Find("Bola");
        if (bola != null)
        {
            rbBola = bola.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("No se encontró el objeto 'Bola'. Asegúrate de que exista en la escena.");
        }
    }

    // Buscar el objeto de la mano
    if (mano == null)
    {
        mano = GameObject.Find("Mano");
        if (mano == null)
        {
            Debug.LogError("No se encontró el objeto 'Mano'. Asegúrate de que exista en la escena.");
        }
    }

    // Inicializar el Wiimote
    WiimoteManager.FindWiimotes();
    if (WiimoteManager.HasWiimote())
    {
        wiimote = WiimoteManager.Wiimotes[0];      
        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
        wiimote.Accel.CalibrateAccel(AccelCalibrationStep.LEFT_SIDE_UP);
        Debug.Log("Wiimote conectado y configurado.");
    }
    else
    {
        Debug.LogWarning("No se detectó ningún mando Wii. Conéctalo e intenta de nuevo.");
    }
}

void Update()
{
    if (wiimote != null)
    {
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);
        wiimote.ReadWiimoteData();
    }

    // Si el botón B está presionado, la bola está enganchada
    if (wiimote.Button.b)
    {
        EngancharBola();
    }
    else if (!wiimote.Button.b && bolaEnganchada)
    {
        SoltarBola();
    }
}



void EngancharBola()
{
    if (rbBola != null)
    {
        bolaEnganchada = true;
        rbBola.isKinematic = true;

        Vector3 posicionMano = mano.transform.position; 
        bola.transform.position = posicionMano + new Vector3(0, 0.2f, 0); // Ajusta el valor en Y según sea necesario

    }
}

void SoltarBola()
{
    if (rbBola != null)
    {
        bolaEnganchada = false;

        rbBola.isKinematic = false; // Reactivar la física

        // Obtener los datos del acelerómetro para calcular la dirección de lanzamiento
        float[] accel = wiimote.Accel.GetCalibratedAccelData();

        // Reducir el movimiento lateral (eje X)
        float movimientoLateralReducido = accel[0] * 0.3f;

        // Calcular la dirección del lanzamiento
        Vector3 direccionLanzamiento = new Vector3(-movimientoLateralReducido, 0, accel[1]); // Eje Z hacia adelante

        // Normalizar la dirección para evitar valores extremos
        direccionLanzamiento = direccionLanzamiento.normalized;

        // Calcular la rapidez del movimiento de la mano (magnitud de la aceleración)
        float rapidezMovimiento = Mathf.Sqrt(accel[0] * accel[0] + accel[1] * accel[1] + accel[2] * accel[2]);

        // Escalar la velocidad inicial de la bola en función de la rapidez del movimiento
        float fuerzaLanzamiento = rapidezMovimiento * 10f; // Ajusta el factor multiplicador según sea necesario

        // Aplicar la velocidad inicial a la bola
        rbBola.linearVelocity = direccionLanzamiento * fuerzaLanzamiento;

        Debug.Log($"Dirección de lanzamiento: {direccionLanzamiento}, Rapidez: {rapidezMovimiento}, Velocidad: {rbBola.linearVelocity}");
    }
}
}