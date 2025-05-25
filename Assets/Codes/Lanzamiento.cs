using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using WiimoteApi;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;

public class Lanzamiento : MonoBehaviour
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
    public ModoPractica modoPractica;

    public double distanciaAlBoliche = -1.0;
    private bool puedeLanzar = true;    

    private Vector3 posicionInicialCamara;
    private Quaternion rotacionInicialCamara;
    private Vector3 posicionInicialBrazo;
    private Vector3 posicionFinalBrazo;
    private PerfilUsuario perfilUsuario;
    private bool kinectListo = false;
    void Start()
    {
        if(isStart){
            rbBola = Boliche.GetComponent<Rigidbody>();
            if (rbBola != null)
            {
                rbBola.linearVelocity = Vector3.zero;
                rbBola.angularVelocity = Vector3.zero;
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
        wiimote = GestorWiimotes.Instance?.wiimote;
        if (wiimote != null)
        {

            if(modoPractica.mostrarFinJuego)
            {
                if(wiimote.Button.d_up){
                    modoPractica.moverMenu(-1);
                }
                else if(wiimote.Button.d_down){
                    modoPractica.moverMenu(1);
                
                }else if(wiimote.Button.a){                
                    modoPractica.SeleccionarBoton();

                }
                
                if (!wiimote.Button.d_up && !wiimote.Button.d_down)
                {
                    modoPractica.LiberarBoton();
                }
              
            }

            if (modoPractica.mostrarPausa){
                if(wiimote.Button.d_up){
                    modoPractica.moverMenu(-1);
                }
                else if(wiimote.Button.d_down){
                    modoPractica.moverMenu(1);
                
                }else if(wiimote.Button.a){                
                    modoPractica.SeleccionarBoton();

                }else if(wiimote.Button.plus){                
                    modoPractica.SalirMenu();

                }                
                if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.plus)
                {
                    modoPractica.LiberarBoton();
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
                if(modoPractica != null && !isStart)
                {
                    modoPractica.ReducirBolas();
                }
                SoltarBola();
        
            
            }else if(wiimote.Button.plus){
                modoPractica.PausarJuego();
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

            rbBola.transform.position = mano.transform.position - new Vector3(0, 0.1f, 0);
            
            rbBola.transform.rotation = mano.transform.rotation;

        }
    }

    void SoltarBola()
    {
        if (rbBola != null)
        {
            bolaEnganchada = false;

            rbBola.isKinematic = false; 
            // Obtener posición final del brazo (muñeca)

            // Calcular la dirección del lanzamiento como vector entre las dos posiciones
            float[] accel = wiimote.Accel.GetCalibratedAccelData();
            // Calcular la rapidez del movimiento de la mano (magnitud de la aceleración)
            float rapidezMovimiento = Mathf.Sqrt(accel[0] * accel[0] + accel[1] * accel[1] + accel[2] * accel[2]);

            // Escalar la velocidad inicial de la Bola en función de la rapidez del movimiento
            float fuerzaLanzamiento = rapidezMovimiento * perfilUsuario.getFuerzaBase();
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
                if(modoPractica.bolasRestantes == 0){
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
        rbBola.gameObject.tag = "Bola";
        
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

        modoPractica.FinJuego();
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
            rbBola.gameObject.tag = "Bola";
        
            puedeLanzar = true;
            
        }
    }

    public void CalcularPuntuacion()
    {
        // Encuentra todas las bolas en la escena con el tag "Bola"
        GameObject[] bolas = GameObject.FindGameObjectsWithTag("Bola");

        // Inicializa la distancia mínima con un valor alto
        float distanciaMinima = float.MaxValue;
        GameObject bolaMasCercana = null;

        // Itera sobre todas las bolas para encontrar la más cercana al boliche
        foreach (GameObject bola in bolas)
        {
            float distancia = Vector3.Distance(bola.transform.position, Boliche.transform.position);

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                bolaMasCercana = bola;
            }
        }

        // Actualiza la distancia más cercana en el HUD
        if (distanciaMinima != float.MaxValue)
        {
            Debug.Log("Distancia más cercana al boliche: " + distanciaMinima);
            modoPractica.ActualizarDistancia(distanciaMinima);
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