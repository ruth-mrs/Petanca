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
    private PerfilUsuario perfilUsuario;


    private Vector3 limiteInferiorPista = new Vector3(-2, 0.05f, 7.78f);
    private Vector3 limiteSuperiorPista = new Vector3(2, 0.05f, 22.78f);

    public Animator animator;

    private float tiempoEspera = 3f;


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
            KinectManager.Instance.animator = animator;
        }

        perfilUsuario.esZurdo = true;

        if(perfilUsuario.esZurdo){
            mano = GameObject.Find("mixamorig7:LeftHandMiddle1");
            KinectManager.Instance.esZurdo = true;

        }else{
            mano = GameObject.Find("mixamorig7:RightHandMiddle1");
            KinectManager.Instance.esZurdo = false;
        }
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
        
            
            }else if(wiimote.Button.plus && !modoPractica.mostrarPausa && !modoPractica.mostrarFinJuego){
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
                if(!rbBola.isKinematic){
                rbBola.linearVelocity = Vector3.zero;
                rbBola.angularVelocity = Vector3.zero;
                }
                rbBola.isKinematic = true;
                bolaEnganchada = true;
            }
            rbBola.transform.position = mano.transform.position - new Vector3(0, 0.1f, 0);
            
        }
    }

    void SoltarBola()
    {
        if (rbBola != null)
        {   
            var (pecho, muñeca) = KinectManager.Instance.ObtenerDireccionBrazoExtendido();
            Vector3 direccionCruda = (muñeca - pecho).normalized;

            if (direccionCruda.magnitude < 0.1f || float.IsNaN(direccionCruda.magnitude))
            {
                var (hombro, codo, muñeca2) = KinectManager.Instance.ObtenerUltimasPosicionesBrazo();
                direccionCruda = (muñeca2 - hombro);
            }

            Vector3 direccionConAltura = new Vector3(
                direccionCruda.x,
                Mathf.Max(direccionCruda.y, 0.3f),
                direccionCruda.z
            ).normalized;

            Vector3 direccionLanzamiento = direccionConAltura;
    
            float[] accel = wiimote.Accel.GetCalibratedAccelData();
            float rapidezMovimiento = Mathf.Sqrt(accel[0] * accel[0] + accel[1] * accel[1] + accel[2] * accel[2]);
    
            float fuerzaLanzamiento = rapidezMovimiento * perfilUsuario.getFuerzaBase();
    
            bolaEnganchada = false;
            rbBola.isKinematic = false;
            rbBola.linearVelocity = direccionLanzamiento * fuerzaLanzamiento;
    
            if (isStart)
            {
                StartCoroutine(EsperarBoliche());
            }
            else
            {
                if (modoPractica.bolasRestantes == 0)
                {
                    puedeLanzar = false;
                    StartCoroutine(EsperarYFinalizarJuego());
                }
                else
                {
                    StartCoroutine(EsperarYCalcularPuntuacion());
                }
            }
        }
    }

    private IEnumerator EsperarBoliche()
    {
        puedeLanzar = false;

        var camaraSeguir = SeguirCamara();


        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEspera)
        {
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        RecuperarCamara(camaraSeguir);


        if(EstaDentroDeLaPista(Boliche.transform.position))
        {
            isStart = false;
            InstanciarBola();
        }
        puedeLanzar = true;
    }

    private IEnumerator EsperarYFinalizarJuego()
    {
       
        puedeLanzar = false;

        var camaraSeguir = SeguirCamara();
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEspera)
        {
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        RecuperarCamara(camaraSeguir);
        modoPractica.FinJuego();
    }

    private IEnumerator EsperarYCalcularPuntuacion()
    {
        if (!isStart)
        {
            puedeLanzar = false;

            var camaraSeguir = SeguirCamara();
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoEspera)
            {
                tiempoTranscurrido += Time.deltaTime;
                yield return null; 
            }

            CalcularPuntuacion();
            RecuperarCamara(camaraSeguir);
            InstanciarBola();        
            puedeLanzar = true;
            
        }
    }


    private CamaraSeguirBola SeguirCamara()
    {
        posicionInicialCamara = Camera.main.transform.position;
        rotacionInicialCamara = Camera.main.transform.rotation;

        CamaraSeguirBola camaraSeguir = Camera.main.GetComponent<CamaraSeguirBola>();

        camaraSeguir.ActualizarBola(rbBola.transform);

        return camaraSeguir;
    }

    private void RecuperarCamara(CamaraSeguirBola camaraSeguir)
    {
        camaraSeguir.DetenerSeguimiento();
        Camera.main.transform.position = posicionInicialCamara;
        Camera.main.transform.rotation = rotacionInicialCamara;

    }

    private void InstanciarBola(){
        rbBola = Instantiate(Bola, Bola.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        rbBola.gameObject.tag = "Bola";
        rbBola.linearVelocity = Vector3.zero;
        rbBola.angularVelocity = Vector3.zero;
        rbBola.isKinematic = true;
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
            modoPractica.ActualizarDistancia(distanciaMinima);
        }
    }

    private bool EstaDentroDeLaPista(Vector3 posicion){
            
    return posicion.x >= limiteInferiorPista.x && posicion.x <= limiteSuperiorPista.x &&
           posicion.z >= limiteInferiorPista.z && posicion.z <= limiteSuperiorPista.z;
    }
  
}