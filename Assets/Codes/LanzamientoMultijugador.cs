using UnityEngine;
using WiimoteApi;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


public class LanzamientoMultijugador : MonoBehaviour
{
 public GameObject Bola;

    public Wiimote wiimote;

    private Rigidbody rbBola;
    public GameObject mano;
    private bool bolaEnganchada = false;
    private Quaternion rotacionInicialBrazo;
    private Vector3 posicionManoAnterior;
    private Vector3 velocidadMano;
    private Vector3 velocidadObjetivo;
    private bool isStart = true;
    public GameObject Boliche;
    public ModoMultijugador modoMultijugador;

    public double distanciaAlBoliche = -1.0;
    private bool puedeLanzar = true;    

    private Vector3 posicionInicialCamara;
    private Quaternion rotacionInicialCamara;
    private Vector3 posicionInicialBrazo;
    private Vector3 posicionFinalBrazo;

    private PerfilUsuario perfilUsuario;

    private PerfilUsuario perfilUsuario2;

    private int turno = 1;
    private bool kinectListo = false;

void Start()
{
    if (mano == null)
    {
        mano = GameObject.Find("Mano");
        if (mano == null)
        {
            Debug.LogError("No se encontró el objeto 'Mano'. Asegúrate de que exista en la escena.");
        }

    }

       if(isStart){
        rbBola = Boliche.GetComponent<Rigidbody>();
            if (rbBola != null)
        {
        rbBola.linearVelocity = Vector3.zero;
        rbBola.angularVelocity = Vector3.zero;

  
            perfilUsuario2 = GameObject.Find("PerfilUsuario2").GetComponent<PerfilUsuario>();
            perfilUsuario2.setFuerzaBase(10f);
            perfilUsuario = GameObject.Find("PerfilUsuario").GetComponent<PerfilUsuario>();
        }
        
    }

    if (KinectManager.Instance != null && KinectManager.Instance.InicializadoCorrectamente)
        {
            kinectListo = true;
            Debug.Log("Kinect asignada correctamente desde el script dependiente.");
        }
        if (!kinectListo) {
            KinectManager.Instance.InicializarKinect();
    };
}
    

void Update()
{

    if(turno % 2 == 0){
        wiimote = GestorWiimotes.Instance?.wiimote2;
    }else{
        wiimote = GestorWiimotes.Instance?.wiimote;
    }
    if (wiimote != null)
    {
        
        if(modoMultijugador.mostrarFinJuego)
        {
            if(wiimote.Button.d_up){
                Debug.Log("Botón d_up presionado");
                modoMultijugador.moverMenu(-1);
            }
            else if(wiimote.Button.d_down){
                Debug.Log("Botón d_down presionado");
                modoMultijugador.moverMenu(1);
            
            }else if(wiimote.Button.a){
                modoMultijugador.SeleccionarBoton();
            }
            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                modoMultijugador.LiberarBoton();
            }
        }

        if (modoMultijugador.mostrarPausa){
            if (wiimote.Button.d_up)
            {
                Debug.Log("Botón d_up presionado");
                modoMultijugador.moverMenu(-1);
            }
            else if (wiimote.Button.d_down)
            {
                Debug.Log("Botón d_down presionado");
                modoMultijugador.moverMenu(1);
            }
            else if (wiimote.Button.a)
            {
                modoMultijugador.SeleccionarBoton();
            }else if (wiimote.Button.plus)
            {
                modoMultijugador.SalirMenu();
            }
            if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.plus)
            {
                modoMultijugador.LiberarBoton();
            }
        }else{
      

        
        if(puedeLanzar){
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);
    
 
        if (wiimote.Button.b)
        {
            EngancharBola();
        }
        else if (!wiimote.Button.b && bolaEnganchada)
        {
            if(modoMultijugador != null && !isStart)
            {
                if(turno % 2 == 0){
                    modoMultijugador.ReducirBolas(2);
                }else{
                    modoMultijugador.ReducirBolas(1);
                }
            }
            SoltarBola();
       
        
        }else if(wiimote.Button.plus && !modoMultijugador.mostrarPausa && !modoMultijugador.mostrarFinJuego){
                modoMultijugador.PausarJuego();
        }
        }
    }
    }

}


void EngancharBola()
{
    if (rbBola != null)
    {
        if(!bolaEnganchada){
                posicionInicialBrazo = ObtenerPosicionBrazo();
        }
        bolaEnganchada = true;
        rbBola.isKinematic = true;

       rbBola.transform.position = mano.transform.position - new Vector3(0,0.1f,0); 

        

    }
}

void SoltarBola()
{
    if (rbBola != null)
    {
        bolaEnganchada = false;

        rbBola.isKinematic = false; 

        float[] accel = wiimote.Accel.GetCalibratedAccelData();
        float rapidezMovimiento = Mathf.Sqrt(accel[0] * accel[0] + accel[1] * accel[1] + accel[2] * accel[2]);


        float fuerzaLanzamiento = 0f;
        if(turno % 2 == 0){
            fuerzaLanzamiento = rapidezMovimiento * perfilUsuario2.getFuerzaBase();
        }else if (turno % 2 == 1){
            fuerzaLanzamiento = rapidezMovimiento * perfilUsuario.getFuerzaBase();
        }
        Debug.Log($"Fuerza de lanzamiento: {perfilUsuario.getFuerzaBase()}");
        // Aplicar la velocidad inicial a la Bola
        posicionFinalBrazo = ObtenerPosicionBrazo();
        Vector3 direccionLanzamiento = (posicionFinalBrazo - posicionInicialBrazo).normalized;
        rbBola.linearVelocity = direccionLanzamiento * fuerzaLanzamiento;
        Debug.Log($"Dirección de lanzamiento: {direccionLanzamiento}, Rapidez: {rapidezMovimiento}, Velocidad: {rbBola.linearVelocity}");
        if(isStart){            
            StartCoroutine(EsperarBoliche());

            isStart = false;
        }else{
            if(modoMultijugador.bolasRestantesJ1 == 0 && modoMultijugador.bolasRestantesJ2 == 0){
                puedeLanzar = false;
                StartCoroutine(EsperarYFinalizarJuego());

            }else{            
            StartCoroutine(EsperarYCalcularPuntuacion());

            }
 
        }

    }
    
}

private IEnumerator EsperarBoliche()
{
    puedeLanzar = false;

    posicionInicialCamara = Camera.main.transform.position;
    rotacionInicialCamara = Camera.main.transform.rotation;    

     CamaraSeguirBola camaraSeguir = Camera.main.GetComponent<CamaraSeguirBola>();

     camaraSeguir.ActualizarBola(rbBola.transform);


    float tiempoEspera = 5f;
    float tiempoTranscurrido = 0f;

    while (tiempoTranscurrido < tiempoEspera)
    {
        tiempoTranscurrido += Time.deltaTime;
        yield return null;
    }

    camaraSeguir.DetenerSeguimiento();
    Camera.main.transform.position = posicionInicialCamara;
    Camera.main.transform.rotation = rotacionInicialCamara;
    rbBola = Instantiate(Bola, mano.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
    rbBola.gameObject.tag = "BolaJugador2";
    
    turno += 1;
    puedeLanzar = true;
}

private IEnumerator EsperarYFinalizarJuego()
{
    CamaraSeguirBola camaraSeguir = Camera.main.GetComponent<CamaraSeguirBola>();

    camaraSeguir.ActualizarBola(rbBola.transform);

    float tiempoEspera = 5f;
    float tiempoTranscurrido = 0f;

    while (tiempoTranscurrido < tiempoEspera)
    {
        tiempoTranscurrido += Time.deltaTime;
        yield return null; // Esperar al siguiente frame
    }

    camaraSeguir.DetenerSeguimiento();

    Camera.main.transform.position = posicionInicialCamara;
    Camera.main.transform.rotation = rotacionInicialCamara;

    modoMultijugador.FinJuego();
}

private IEnumerator EsperarYCalcularPuntuacion()
{
    if (!isStart)
    {
        puedeLanzar = false;

        CamaraSeguirBola camaraSeguir = Camera.main.GetComponent<CamaraSeguirBola>();

        camaraSeguir.ActualizarBola(rbBola.transform);

        float tiempoEspera = 5f;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEspera)
        {
            tiempoTranscurrido += Time.deltaTime;
            yield return null; 
        }

        CalcularPuntuacion();

        camaraSeguir.DetenerSeguimiento();
        Camera.main.transform.position = posicionInicialCamara;
        Camera.main.transform.rotation = rotacionInicialCamara;
        rbBola = Instantiate(Bola, mano.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        
        turno += 1;


        if (turno % 2 == 0)
        {
            rbBola.gameObject.tag = "BolaJugador2";
        }
        else
        {
            rbBola.gameObject.tag = "BolaJugador1";
        }

       
        puedeLanzar = true;
        
    }
}

public void CalcularPuntuacion()
{
     // Obtener las bolas de cada jugador
    GameObject[] bolasJugador1 = GameObject.FindGameObjectsWithTag("BolaJugador1");
    Debug.Log($"Número de bolas del Jugador 1: {bolasJugador1.Length}");
    GameObject[] bolasJugador2 = GameObject.FindGameObjectsWithTag("BolaJugador2");

    float distanciaMinimaJugador1 = float.MaxValue;
    float distanciaMinimaJugador2 = float.MaxValue;

    // Calcular la distancia mínima para las bolas del jugador 1
    foreach (GameObject bola in bolasJugador1)
    {
        float distancia = Vector3.Distance(bola.transform.position, Boliche.transform.position);
        if (distancia < distanciaMinimaJugador1)
        {
            distanciaMinimaJugador1 = distancia;
        }
    }

    // Calcular la distancia mínima para las bolas del jugador 2
    foreach (GameObject bola in bolasJugador2)
    {
        float distancia = Vector3.Distance(bola.transform.position, Boliche.transform.position);

        if (distancia < distanciaMinimaJugador2)
        {
            distanciaMinimaJugador2 = distancia;
        }
    }

    if (modoMultijugador != null)
    {
        if (distanciaMinimaJugador1 == 0)
        {
            distanciaMinimaJugador1 = -1.0f;
        }
        if (distanciaMinimaJugador2 == 0)
        {
            distanciaMinimaJugador2 = -1.0f;
        }
        Debug.Log($"Distancia mínima Jugador 1: {distanciaMinimaJugador1}");
        Debug.Log($"Distancia mínima Jugador 2: {distanciaMinimaJugador2}");

        modoMultijugador.ActualizarDistancia(distanciaMinimaJugador1, 1);
        modoMultijugador.ActualizarDistancia(distanciaMinimaJugador2, 2);
    }
}
    private Vector3 ObtenerPosicionBrazo()
    {
        if (KinectManager.Instance == null || !KinectManager.Instance.InicializadoCorrectamente || KinectManager.Instance.kinectDevice == null || KinectManager.Instance.bodyTracker == null)
        {
            return Vector3.zero;
        }
        try
        {
            return KinectManager.Instance.wristPos;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error obteniendo posición del brazo: " + ex.Message);
            return Vector3.zero;
        }
    }
}

