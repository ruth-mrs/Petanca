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
    
    // Referencias al perfil del jugador
    private PerfilUsuario perfilUsuario;
    private DatosPerfilUsuario datosPerfilJugador;

    private Vector3 limiteInferiorPista = new Vector3(-2, 0.05f, 7.78f);
    private Vector3 limiteSuperiorPista = new Vector3(2, 0.05f, 22.78f);

    public Animator animator;

    private float tiempoEspera = 3f;

    void Start()
    {
        InicializarPerfil();
        InicializarJuego();
        ConfigurarKinect();
    }

    void InicializarPerfil()
    {
        // Obtener perfil desde ModoPractica
        if (modoPractica != null)
        {
            datosPerfilJugador = modoPractica.ObtenerDatosPerfilJugador();
            perfilUsuario = modoPractica.ObtenerPerfilJugador();
        }

        // Fallback si no se encuentra el perfil
        if (datosPerfilJugador == null && GestorPerfiles.Instancia != null)
        {
            datosPerfilJugador = GestorPerfiles.Instancia.ObtenerPerfilActual();
            perfilUsuario = GestorPerfiles.Instancia.CrearPerfilUsuarioMonoBehaviour(datosPerfilJugador);
        }

        // Último fallback - crear perfil por defecto
        if (datosPerfilJugador == null)
        {
            datosPerfilJugador = new DatosPerfilUsuario("Jugador", false, false, 2.5f);
            Debug.LogWarning("Usando perfil por defecto en Lanzamiento");
        }

        Debug.Log($"Lanzamiento iniciado con perfil: {datosPerfilJugador.nombreUsuario}");
        Debug.Log($"- Zurdo: {datosPerfilJugador.esZurdo}");
        Debug.Log($"- Fuerza base: {datosPerfilJugador.fuerzaBase}");
        Debug.Log($"- Factor ayuda: {datosPerfilJugador.factorAyuda}");
    }

    void InicializarJuego()
    {
        if (isStart)
        {
            rbBola = Boliche.GetComponent<Rigidbody>();
            if (rbBola != null)
            {
                rbBola.linearVelocity = Vector3.zero;
                rbBola.angularVelocity = Vector3.zero;
            }
        }
    }

    void ConfigurarKinect()
    {
        if (KinectManager.Instance != null && KinectManager.Instance.InicializadoCorrectamente)
        {
            KinectManager.Instance.animator = animator;
        }

        // Configurar mano según lateralidad del perfil
        if (datosPerfilJugador != null && datosPerfilJugador.esZurdo)
        {
            mano = GameObject.Find("mixamorig7:LeftHandMiddle1");
            if (KinectManager.Instance != null)
                KinectManager.Instance.esZurdo = true;
        }
        else
        {
            mano = GameObject.Find("mixamorig7:RightHandMiddle1");
            if (KinectManager.Instance != null)
                KinectManager.Instance.esZurdo = false;
        }

        if (mano == null)
        {
            Debug.LogError("No se encontró el objeto de la mano. Verificar jerarquía del modelo.");
        }
    }

    void Update()
    {
        wiimote = GestorWiimotes.Instance?.wiimote;
        if (wiimote != null)
        {
            if (modoPractica.mostrarFinJuego)
            {
                if (wiimote.Button.d_up)
                {
                    modoPractica.moverMenu(-1);
                }
                else if (wiimote.Button.d_down)
                {
                    modoPractica.moverMenu(1);
                }
                else if (wiimote.Button.a)
                {
                    modoPractica.SeleccionarBoton();
                }
                
                if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.a)
                {
                    modoPractica.LiberarBoton();
                }
            }

            if (modoPractica.mostrarPausa)
            {
                if (wiimote.Button.d_up)
                {
                    modoPractica.moverMenu(-1);
                }
                else if (wiimote.Button.d_down)
                {
                    modoPractica.moverMenu(1);
                }
                else if (wiimote.Button.a)
                {
                    modoPractica.SeleccionarBoton();
                }
                else if (wiimote.Button.plus)
                {
                    modoPractica.SalirMenu();
                }                
                if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.plus && !wiimote.Button.a)

                {
                    modoPractica.LiberarBoton();
                }
            }
            else
            { 
                if (puedeLanzar)
                {
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
                        if (modoPractica != null && !isStart)
                        {
                            modoPractica.ReducirBolas();
                        }
                        SoltarBola();
                    }
                    else if (wiimote.Button.plus && !modoPractica.mostrarPausa && !modoPractica.mostrarFinJuego)
                    {
                        modoPractica.PausarJuego();
                    }
                }
            }
        }
    }

    void EngancharBola()
    {
        if (rbBola != null && mano != null)
        {
            if (!bolaEnganchada)
            {
                if (!rbBola.isKinematic)
                {
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
    
            // Usar el factor de fuerza del perfil actualizado
            float fuerzaBase = datosPerfilJugador != null ? datosPerfilJugador.fuerzaBase : 2.5f;
            float factorAyuda = datosPerfilJugador != null ? datosPerfilJugador.factorAyuda : 1.0f;
            
            float fuerzaLanzamiento = rapidezMovimiento * fuerzaBase * factorAyuda;
    
            Debug.Log($"Lanzamiento - Rapidez: {rapidezMovimiento:F2}, Fuerza base: {fuerzaBase:F2}, Factor ayuda: {factorAyuda:F2}, Fuerza final: {fuerzaLanzamiento:F2}");
    
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

        if (EstaDentroDeLaPista(Boliche.transform.position))
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
        CalcularPuntuacion();
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

    private void InstanciarBola()
    {
        rbBola = Instantiate(Bola, Bola.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        rbBola.gameObject.tag = "Bola";
        rbBola.linearVelocity = Vector3.zero;
        rbBola.angularVelocity = Vector3.zero;
        rbBola.isKinematic = true;
    }

    public void CalcularPuntuacion()
    {
        GameObject[] bolas = GameObject.FindGameObjectsWithTag("Bola");

        float distanciaMinima = float.MaxValue;
        GameObject bolaMasCercana = null;

        foreach (GameObject bola in bolas)
        {
            float distancia = Vector3.Distance(bola.transform.position, Boliche.transform.position);

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                bolaMasCercana = bola;
            }
        }

        if (distanciaMinima != float.MaxValue)
        {
            Debug.Log($"Distancia que se enviará al HUD: {distanciaMinima:F6}");

            modoPractica.ActualizarDistancia(distanciaMinima);
            
            // Actualizar visualización de la bola más cercana
            if (bolaMasCercana != null)
            {
                modoPractica.ActualizarBolaMasCercana(bolaMasCercana, distanciaMinima);
            }
        }
    }

    private bool EstaDentroDeLaPista(Vector3 posicion)
    {
        return posicion.x >= limiteInferiorPista.x && posicion.x <= limiteSuperiorPista.x &&
               posicion.z >= limiteInferiorPista.z && posicion.z <= limiteSuperiorPista.z;
    }

    // Método público para obtener el perfil actual
    public DatosPerfilUsuario ObtenerPerfilActual()
    {
        return datosPerfilJugador;
    }
}