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

    // Referencias a perfiles actualizadas
    private PerfilUsuario perfilUsuario1;
    private PerfilUsuario perfilUsuario2;
    private DatosPerfilUsuario datosPerfilJugador1;
    private DatosPerfilUsuario datosPerfilJugador2;
    private DatosPerfilUsuario perfilActualSeleccionado;

    private int turno = 1;

    public Animator animator;

    private Vector3 limiteInferiorPista = new Vector3(-2, 0.05f, 7.78f);
    private Vector3 limiteSuperiorPista = new Vector3(2, 0.05f, 22.78f);

    public GameObject cuerpo;
    public GameObject piernas;

    public Material azul;
    public Material rojo;

    private bool turnAssigned = false;
    private float tiempoEspera = 3f;

    void Start()
    {
        InicializarPerfiles();
        InicializarJuego();
        ConfigurarKinect();
    }

    void InicializarPerfiles()
    {
        // Obtener perfiles desde ModoMultijugador
        if (modoMultijugador != null)
        {
            datosPerfilJugador1 = modoMultijugador.ObtenerDatosPerfilJugador(1);
            datosPerfilJugador2 = modoMultijugador.ObtenerDatosPerfilJugador(2);
            perfilUsuario1 = modoMultijugador.ObtenerPerfilJugador(1);
            perfilUsuario2 = modoMultijugador.ObtenerPerfilJugador(2);
        }

        // Fallback si no se encuentran los perfiles
        if (datosPerfilJugador1 == null || datosPerfilJugador2 == null)
        {
            Debug.LogWarning("No se encontraron perfiles desde ModoMultijugador. Usando fallback.");
            
            if (GestorPerfiles.Instancia != null)
            {
                var perfilesDisponibles = GestorPerfiles.Instancia.ObtenerPerfiles();
                
                if (datosPerfilJugador1 == null)
                {
                    datosPerfilJugador1 = perfilesDisponibles.Count > 0 ? 
                        perfilesDisponibles[0] : 
                        new DatosPerfilUsuario("Jugador 1", false, false, 2.5f);
                    
                    perfilUsuario1 = GestorPerfiles.Instancia.CrearPerfilUsuarioMonoBehaviour(datosPerfilJugador1);
                }
                
                if (datosPerfilJugador2 == null)
                {
                    datosPerfilJugador2 = perfilesDisponibles.Count > 1 ? 
                        perfilesDisponibles[1] : 
                        new DatosPerfilUsuario("Jugador 2", true, false, 2.5f);
                    
                    perfilUsuario2 = GestorPerfiles.Instancia.CrearPerfilUsuarioMonoBehaviour(datosPerfilJugador2);
                }
            }
            else
            {
                // Crear perfiles por defecto
                datosPerfilJugador1 = new DatosPerfilUsuario("Jugador 1", false, false, 2.5f);
                datosPerfilJugador2 = new DatosPerfilUsuario("Jugador 2", true, false, 2.5f);
                
                Debug.LogWarning("Usando perfiles por defecto en LanzamientoMultijugador");
            }
        }

        Debug.Log($"LanzamientoMultijugador iniciado:");
        Debug.Log($"Jugador 1: {datosPerfilJugador1.nombreUsuario} (Zurdo: {datosPerfilJugador1.esZurdo}, Fuerza: {datosPerfilJugador1.fuerzaBase})");
        Debug.Log($"Jugador 2: {datosPerfilJugador2.nombreUsuario} (Zurdo: {datosPerfilJugador2.esZurdo}, Fuerza: {datosPerfilJugador2.fuerzaBase})");
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
            Debug.Log("Kinect asignada correctamente desde LanzamientoMultijugador.");
        }
    }

    void Update()
    {
        if (!turnAssigned)
        {
            AsignarTurno();
        }

        if (wiimote != null)
        {
            if (modoMultijugador.mostrarFinJuego)
            {
                if (wiimote.Button.d_up)
                {
                    modoMultijugador.moverMenu(-1);
                }
                else if (wiimote.Button.d_down)
                {
                    modoMultijugador.moverMenu(1);
                }
                else if (wiimote.Button.a)
                {
                    modoMultijugador.SeleccionarBoton();
                }
                if (!wiimote.Button.d_up && !wiimote.Button.d_down)
                {
                    modoMultijugador.LiberarBoton();
                }
            }

            if (modoMultijugador.mostrarPausa)
            {
                if (wiimote.Button.d_up)
                {
                    modoMultijugador.moverMenu(-1);
                }
                else if (wiimote.Button.d_down)
                {
                    modoMultijugador.moverMenu(1);
                }
                else if (wiimote.Button.a)
                {
                    modoMultijugador.SeleccionarBoton();
                }
                else if (wiimote.Button.plus)
                {
                    modoMultijugador.SalirMenu();
                }
                if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.plus)
                {
                    modoMultijugador.LiberarBoton();
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
                        if (modoMultijugador != null && !isStart)
                        {
                            if (turno % 2 == 0)
                            {
                                modoMultijugador.ReducirBolas(2);
                            }
                            else
                            {
                                modoMultijugador.ReducirBolas(1);
                            }
                        }
                        SoltarBola();
                    }
                    else if (wiimote.Button.plus && !modoMultijugador.mostrarPausa && !modoMultijugador.mostrarFinJuego)
                    {
                        modoMultijugador.PausarJuego();
                    }
                }
            }
        }
    }

    void AsignarTurno()
    {
        if (turno % 2 == 0)
        {
            // Turno del Jugador 2
            wiimote = GestorWiimotes.Instance?.wiimote2;
            perfilActualSeleccionado = datosPerfilJugador2;

            // Cambiar color a rojo
            Renderer renderer = cuerpo.GetComponent<Renderer>();
            if (renderer.material != rojo)
            {
                renderer.material = rojo;
                renderer = piernas.GetComponent<Renderer>();
                renderer.material = rojo;
            }
        }
        else
        {
            // Turno del Jugador 1
            wiimote = GestorWiimotes.Instance?.wiimote;
            perfilActualSeleccionado = datosPerfilJugador1;

            // Cambiar color a azul
            Renderer renderer = cuerpo.GetComponent<Renderer>();
            if (renderer.material != azul)
            {
                renderer.material = azul;
                renderer = piernas.GetComponent<Renderer>();
                renderer.material = azul;
            }
        }

        // Configurar mano según lateralidad del perfil actual
        if (perfilActualSeleccionado.esZurdo)
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
            Debug.LogError($"No se encontró la mano para el jugador {(turno % 2 == 0 ? 2 : 1)}");
        }

        turnAssigned = true;
        
        Debug.Log($"Turno asignado al Jugador {(turno % 2 == 0 ? 2 : 1)} - {perfilActualSeleccionado.nombreUsuario} (Zurdo: {perfilActualSeleccionado.esZurdo})");
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
        if (rbBola != null && perfilActualSeleccionado != null)
        {
            var (pecho, muñeca) = KinectManager.Instance.ObtenerDireccionBrazoExtendido();
            Vector3 direccionCruda = (muñeca - pecho).normalized;

            if (direccionCruda.magnitude < 0.1f || float.IsNaN(direccionCruda.magnitude))
            {
                var (hombro, codo, muñeca2) = KinectManager.Instance.ObtenerUltimasPosicionesBrazo();
                direccionCruda = (muñeca2 - hombro);
            }

            Vector3 direccionConAltura = new Vector3(direccionCruda.x, Mathf.Max(direccionCruda.y, 0.3f), direccionCruda.z).normalized;

            Vector3 direccionLanzamiento = direccionConAltura;

            float[] accel = wiimote.Accel.GetCalibratedAccelData();
            float rapidezMovimiento = Mathf.Sqrt(accel[0] * accel[0] + accel[1] * accel[1] + accel[2] * accel[2]);

            // Usar la fuerza del perfil del jugador actual
            float fuerzaBase = perfilActualSeleccionado.fuerzaBase;
            float factorAyuda = perfilActualSeleccionado.factorAyuda;
            float fuerzaLanzamiento = rapidezMovimiento * fuerzaBase * factorAyuda;

            int jugadorActual = turno % 2 == 0 ? 2 : 1;
            Debug.Log($"Lanzamiento Jugador {jugadorActual} - Rapidez: {rapidezMovimiento:F2}, Fuerza base: {fuerzaBase:F2}, Factor ayuda: {factorAyuda:F2}, Fuerza final: {fuerzaLanzamiento:F2}");

            bolaEnganchada = false;
            rbBola.isKinematic = false;
            rbBola.linearVelocity = direccionLanzamiento * fuerzaLanzamiento;

            if (isStart)
            {
                StartCoroutine(EsperarBoliche());
            }
            else
            {
                if (modoMultijugador.bolasRestantesJ1 == 0 && modoMultijugador.bolasRestantesJ2 == 0)
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

        var camara = SeguirCamara();

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEspera)
        {
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        RecuperarCamara(camara);

        if (EstaDentroDeLaPista(Boliche.transform.position))
        {
            isStart = false;
            instanciarBola(2);
            ReiniciarCuerpo();
            AvanzarTurno();
        }

        puedeLanzar = true;
    }

    private IEnumerator EsperarYFinalizarJuego()
    {
        puedeLanzar = false;
        var camara = SeguirCamara();
        
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEspera)
        {
            tiempoTranscurrido += Time.deltaTime;
            yield return null; 
        }

        RecuperarCamara(camara);

        modoMultijugador.FinJuego();
    }

    private IEnumerator EsperarYCalcularPuntuacion()
    {
        if (!isStart)
        {
            puedeLanzar = false;

            var camara = SeguirCamara();

            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoEspera)
            {
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }

            CalcularPuntuacion();
            RecuperarCamara(camara);
            instanciarBola(turno % 2 == 0 ? 1 : 2);
            ReiniciarCuerpo();
            AvanzarTurno();          
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

    public void instanciarBola(int jugador)
    {
        if (rbBola != null)
        {
            rbBola = Instantiate(Bola, Bola.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rbBola.linearVelocity = Vector3.zero;
            rbBola.angularVelocity = Vector3.zero;

            if (jugador == 1)
            {
                rbBola.gameObject.tag = "BolaJugador1";
            }
            else if (jugador == 2)
            {
                rbBola.gameObject.tag = "BolaJugador2";
            }
        }
    }

    public void AvanzarTurno()
    {
        turno += 1;
        turnAssigned = false;
    }

    public void ReiniciarCuerpo()
    {
        if (perfilActualSeleccionado != null)
        {
            Transform upperArmBone = animator.GetBoneTransform(perfilActualSeleccionado.esZurdo ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
            Transform lowerArmBone = animator.GetBoneTransform(perfilActualSeleccionado.esZurdo ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);

            if (upperArmBone && lowerArmBone)
            {
                upperArmBone.localRotation = Quaternion.identity;
                lowerArmBone.localRotation = Quaternion.identity;
            }
        }
    }

    public void CalcularPuntuacion()
    {
        GameObject[] bolasJugador1 = GameObject.FindGameObjectsWithTag("BolaJugador1");
        GameObject[] bolasJugador2 = GameObject.FindGameObjectsWithTag("BolaJugador2");

        float distanciaMinimaJugador1 = float.MaxValue;
        float distanciaMinimaJugador2 = float.MaxValue;

        foreach (GameObject bola in bolasJugador1)
        {
            float distancia = Vector3.Distance(bola.transform.position, Boliche.transform.position);
            if (distancia < distanciaMinimaJugador1)
            {
                distanciaMinimaJugador1 = distancia;
            }
        }

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

            modoMultijugador.ActualizarDistancia(distanciaMinimaJugador1, 1);
            modoMultijugador.ActualizarDistancia(distanciaMinimaJugador2, 2);
        }
    }

    private bool EstaDentroDeLaPista(Vector3 posicion)
    {
        Debug.Log($"Posición de la bola: {posicion}, Límites de la pista: Inferior {limiteInferiorPista}, Superior {limiteSuperiorPista}");

        return posicion.x >= limiteInferiorPista.x && posicion.x <= limiteSuperiorPista.x &&
               posicion.z >= limiteInferiorPista.z && posicion.z <= limiteSuperiorPista.z;
    }

    // Métodos públicos para obtener información de perfiles
    public DatosPerfilUsuario ObtenerPerfilJugador(int numeroJugador)
    {
        return numeroJugador == 1 ? datosPerfilJugador1 : datosPerfilJugador2;
    }

    public DatosPerfilUsuario ObtenerPerfilActual()
    {
        return perfilActualSeleccionado;
    }
}