using UnityEngine;
using WiimoteApi;

public class Lanzamiento : MonoBehaviour
{
    public GameObject bola;
    public float fuerzaLanzamiento = 500f;
    public Wiimote wiimote;

    private Rigidbody rbBola;
    private GameObject mano;
    private bool bolaEnganchada = false;
    private Quaternion rotacionInicialBrazo;
    private float suavizadoZ = 0f;
    private float velocidadMovimiento = 10f;
    private float rangoRotacion = 120f;  // Rango de rotación del brazo
    private float velocidadRotacionMaxima = 100f;  // Limitar la velocidad de rotación para evitar movimientos demasiado rápidos

    // Para mantener la rotación anterior de manera suave
    private float rotacionXActual = 0f;

    // Variables para calcular la velocidad de la mano
    private Vector3 posicionManoAnterior;
    private Vector3 velocidadMano;

    // Factor de suavizado para el acelerómetro
    private float factorSuavizado = 0.2f;

    void Start()
    {
        // Inicializar Wiimote
        WiimoteManager.FindWiimotes();
        wiimote = WiimoteManager.Wiimotes[0];
        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
        wiimote.Accel.CalibrateAccel(AccelCalibrationStep.A_BUTTON_UP);
        wiimote.SendPlayerLED(true, false, false, false);

        // Inicializar bola y brazo
        if (bola == null)
        {
            bola = GameObject.Find("Bola");
        }

        if (bola != null)
        {
            rbBola = bola.GetComponent<Rigidbody>();
        }

        mano = GameObject.Find("Brazo");
        rotacionInicialBrazo = Quaternion.Euler(0, 0, 0);
        mano.transform.rotation = rotacionInicialBrazo;

        // Inicializar la posición anterior de la mano
        posicionManoAnterior = mano.transform.position;
    }

    void Update()
    {
        // Leer datos del Wiimote
        if (wiimote != null)
        {
            wiimote.ReadWiimoteData();
        }

        // Calcular la velocidad de la mano
        velocidadMano = (mano.transform.position - posicionManoAnterior) / Time.deltaTime;
        posicionManoAnterior = mano.transform.position;

        // Leer datos del acelerómetro y suavizarlos
        if (wiimote != null)
        {
            float[] accel = wiimote.Accel.GetCalibratedAccelData();
            suavizadoZ = Mathf.Lerp(suavizadoZ, accel[2], factorSuavizado); // Suaviza el eje Z
        }

        // Enganchar o soltar la bola
        if (wiimote.Button.b && !bolaEnganchada)
        {
            EngancharBola();
        }
        else if (!wiimote.Button.b && bolaEnganchada)
        {
            SoltarBola();
        }

        // Actualizar la posición de la bola si está enganchada
        if (bolaEnganchada)
        {
            CalcularMovimientoBrazo();
            if (rbBola != null)
            {
                Vector3 puntoDebajoBrazo = mano.transform.TransformPoint(new Vector3(0, -1.3f, 0));
                bola.transform.position = Vector3.Lerp(bola.transform.position, puntoDebajoBrazo, Time.deltaTime * velocidadMovimiento);
            }
        }
        else
        {
            mano.transform.rotation = rotacionInicialBrazo;
        }
    }

    void CalcularMovimientoBrazo()
    {
        if (wiimote != null)
        {
            // Usa el valor suavizado del acelerómetro
            float rotacionX = Mathf.Clamp(suavizadoZ * rangoRotacion, -rangoRotacion, rangoRotacion);

            float rotacionXSuave = Mathf.MoveTowards(rotacionXActual, rotacionX, velocidadRotacionMaxima * Time.deltaTime);
            rotacionXActual = rotacionXSuave;

            Quaternion rotacionObjetivo = Quaternion.Euler(rotacionXActual, 0f, 0f);
            mano.transform.localRotation = rotacionObjetivo;

            Debug.Log($"Aceleración Z (suavizada): {suavizadoZ} | Rotación X: {rotacionXSuave}");
        }
    }

    void EngancharBola()
    {
        bolaEnganchada = true;

        if (rbBola != null)
        {
            rbBola.isKinematic = true; // Evitamos que la bola sea afectada por la física
        }
    }

    void SoltarBola()
    {
        bolaEnganchada = false;

        if (rbBola != null)
        {
            rbBola.isKinematic = false; // Volvemos a habilitar la física de la bola

            Vector3 velocidadMano = (mano.transform.position - posicionManoAnterior) / Time.deltaTime;

            // Usa la velocidad de la mano para lanzar la bola
            rbBola.linearVelocity = velocidadMano;

            Debug.Log($"Velocidad de la mano al soltar: {velocidadMano}");
        }
    }
}
