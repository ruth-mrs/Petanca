using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ModoMultijugador : MonoBehaviour
{
    [Header("Configuración del Juego")]
    public int bolasRestantesJ1 = 3;
    public int bolasRestantesJ2 = 3;
    public double distanciaJ1 = -1.0;
    public double distanciaJ2 = -1.0;

    [Header("UI References")]
    public Canvas canvas;
    public GUIStyle estilo;

    [Header("UI Jugadores")]
    public TMP_Text textoJ1Nombre, textoJ1Bolas, textoJ1Distancia;
    public TMP_Text textoJ2Nombre, textoJ2Bolas, textoJ2Distancia;
    public GameObject coronaJ1, coronaJ2;
    public LineRenderer lineaJ1, lineaJ2;
    public GameObject panelResultado;
    public TMP_Text textoGanador, textoDistanciaGanadora;

    [Header("Estado del Juego")]
    public bool mostrarFinJuego = false;
    public bool mostrarPausa = false;
    private bool cambioPausa = false;

    // Perfiles de los jugadores
    private PerfilUsuario perfilJugador1;
    private PerfilUsuario perfilJugador2;
    private DatosPerfilUsuario datosPerfilJugador1;
    private DatosPerfilUsuario datosPerfilJugador2;

    // Posiciones para las líneas
    private Vector3 posBolaJ1 = Vector3.zero;
    private Vector3 posBolaJ2 = Vector3.zero;
    private Vector3 posBoliche = Vector3.zero;

    void Start()
    {
        InicializarPerfiles();
        InicializarUI();
        InicializarJuego();
    }

    void InicializarPerfiles()
    {
        // Obtener perfiles seleccionados desde SelectorPerfiles
        datosPerfilJugador1 = SelectorPerfiles.perfilJugador1;
        datosPerfilJugador2 = SelectorPerfiles.perfilJugador2;
        
        // Validar que tenemos ambos perfiles
        if (datosPerfilJugador1 == null || datosPerfilJugador2 == null)
        {
            Debug.LogError("Faltan perfiles para modo multijugador. Usando perfiles por defecto.");
            
            if (GestorPerfiles.Instancia != null)
            {
                var perfilesDisponibles = GestorPerfiles.Instancia.ObtenerPerfiles();
                
                if (datosPerfilJugador1 == null)
                {
                    datosPerfilJugador1 = perfilesDisponibles.Count > 0 ? 
                        perfilesDisponibles[0] : 
                        new DatosPerfilUsuario("Jugador 1", false, false, 2.5f);
                }
                
                if (datosPerfilJugador2 == null)
                {
                    datosPerfilJugador2 = perfilesDisponibles.Count > 1 ? 
                        perfilesDisponibles[1] : 
                        new DatosPerfilUsuario("Jugador 2", false, false, 2.5f);
                }
            }
            else
            {
                // Crear perfiles por defecto si no hay gestor
                datosPerfilJugador1 = new DatosPerfilUsuario("Jugador 1", false, false, 2.5f);
                datosPerfilJugador2 = new DatosPerfilUsuario("Jugador 2", false, false, 2.5f);
            }
        }
        
        // Crear MonoBehaviours de los perfiles
        if (GestorPerfiles.Instancia != null)
        {
            perfilJugador1 = GestorPerfiles.Instancia.CrearPerfilUsuarioMonoBehaviour(datosPerfilJugador1);
            perfilJugador2 = GestorPerfiles.Instancia.CrearPerfilUsuarioMonoBehaviour(datosPerfilJugador2);
            
            DontDestroyOnLoad(perfilJugador1.gameObject);
            DontDestroyOnLoad(perfilJugador2.gameObject);
        }
        
        Debug.Log($"Modo Multijugador iniciado:");
        Debug.Log($"Jugador 1: {datosPerfilJugador1.nombreUsuario} (Zurdo: {datosPerfilJugador1.esZurdo}, Reducido: {datosPerfilJugador1.perfilReducido})");
        Debug.Log($"Jugador 2: {datosPerfilJugador2.nombreUsuario} (Zurdo: {datosPerfilJugador2.esZurdo}, Reducido: {datosPerfilJugador2.perfilReducido})");
    }

    void InicializarUI()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            canvas.enabled = false;
        }

        // Configurar nombres de los jugadores con información adicional
        if (textoJ1Nombre && datosPerfilJugador1 != null)
        {
            string nombreJ1 = datosPerfilJugador1.nombreUsuario;
            if (datosPerfilJugador1.esZurdo) nombreJ1 += " (Z)";
            if (datosPerfilJugador1.perfilReducido) nombreJ1 += " (R)";
            textoJ1Nombre.text = nombreJ1;
        }
        else if (textoJ1Nombre)
        {
            textoJ1Nombre.text = "JUGADOR 1";
        }

        if (textoJ2Nombre && datosPerfilJugador2 != null)
        {
            string nombreJ2 = datosPerfilJugador2.nombreUsuario;
            if (datosPerfilJugador2.esZurdo) nombreJ2 += " (Z)";
            if (datosPerfilJugador2.perfilReducido) nombreJ2 += " (R)";
            textoJ2Nombre.text = nombreJ2;
        }
        else if (textoJ2Nombre)
        {
            textoJ2Nombre.text = "JUGADOR 2";
        }
    }

    void InicializarJuego()
    {
        // Configurar elementos específicos de cada perfil
        if (datosPerfilJugador1 != null || datosPerfilJugador2 != null)
        {
            // Activar ayudas visuales si algún jugador tiene perfil reducido
            if ((datosPerfilJugador1 != null && datosPerfilJugador1.perfilReducido) ||
                (datosPerfilJugador2 != null && datosPerfilJugador2.perfilReducido))
            {
                ActivarAyudasVisuales();
            }

            // Configurar cámaras según lateralidad
            ConfigurarCamarasSegunLateralidad();
        }

        ActualizarUI();
    }

    void ActivarAyudasVisuales()
    {
        GameObject[] ayudas = GameObject.FindGameObjectsWithTag("AyudaVisual");
        foreach (GameObject ayuda in ayudas)
        {
            ayuda.SetActive(true);
        }
        Debug.Log("Ayudas visuales activadas (uno o más jugadores tienen perfil reducido)");
    }

    void ConfigurarCamarasSegunLateralidad()
    {
        // Configurar cámaras según la lateralidad de cada jugador
        if (datosPerfilJugador1 != null && datosPerfilJugador1.esZurdo)
        {
            ConfigurarCamaraParaZurdo(1);
        }
        
        if (datosPerfilJugador2 != null && datosPerfilJugador2.esZurdo)
        {
            ConfigurarCamaraParaZurdo(2);
        }
    }

    void ConfigurarCamaraParaZurdo(int numeroJugador)
    {
        // Configuración específica de cámara para jugadores zurdos
        var camara = GameObject.Find($"CamaraJugador{numeroJugador}")?.GetComponent<Camera>();
        if (camara != null)
        {
            Vector3 pos = camara.transform.position;
            pos.x = -Mathf.Abs(pos.x);
            camara.transform.position = pos;
            Debug.Log($"Cámara configurada para Jugador {numeroJugador} (zurdo)");
        }
    }

    void Update()
    {
        ActualizarUI();
        ActualizarCoronas();
        ActualizarLineas();
    }

    public void ActualizarDistancia(double nuevaDistancia, int jugador)
    {
        if (jugador == 1)
        {
            distanciaJ1 = nuevaDistancia;
        }
        else if (jugador == 2)
        {
            distanciaJ2 = nuevaDistancia;
        }
    }

    public void FinJuego()
    {
        mostrarFinJuego = true;
        canvas.enabled = true;
        MostrarPanelResultado();
    }

    public void ReducirBolas(int jugador)
    {
        if (jugador == 1 && bolasRestantesJ1 > 0)
        {
            bolasRestantesJ1--;
        }
        else if (jugador == 2 && bolasRestantesJ2 > 0)
        {
            bolasRestantesJ2--;
        }

        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoJ1Bolas) textoJ1Bolas.text = $"Bolas: {bolasRestantesJ1}";
        if (textoJ2Bolas) textoJ2Bolas.text = $"Bolas: {bolasRestantesJ2}";

        if (textoJ1Distancia)
        {
            textoJ1Distancia.text = distanciaJ1 == -1.0 ? "Distancia: --" : $"Distancia: {distanciaJ1:F2} m";
        }
        if (textoJ2Distancia)
        {
            textoJ2Distancia.text = distanciaJ2 == -1.0 ? "Distancia: --" : $"Distancia: {distanciaJ2:F2} m";
        }
    }

    private void ActualizarCoronas()
    {
        if (coronaJ1 && coronaJ2)
        {
            if (distanciaJ1 != -1.0 && (distanciaJ1 < distanciaJ2 || distanciaJ2 == -1.0))
            {
                coronaJ1.SetActive(true);
                coronaJ2.SetActive(false);
            }
            else if (distanciaJ2 != -1.0)
            {
                coronaJ1.SetActive(false);
                coronaJ2.SetActive(true);
            }
            else
            {
                coronaJ1.SetActive(false);
                coronaJ2.SetActive(false);
            }
        }
    }

    private void ActualizarLineas()
    {
        if (lineaJ1 && posBolaJ1 != Vector3.zero && posBoliche != Vector3.zero)
        {
            lineaJ1.enabled = true;
            lineaJ1.positionCount = 2;
            lineaJ1.SetPosition(0, new Vector3(posBolaJ1.x, 0.05f, posBolaJ1.z));
            lineaJ1.SetPosition(1, new Vector3(posBoliche.x, 0.05f, posBoliche.z));
        }
        else if (lineaJ1)
        {
            lineaJ1.enabled = false;
        }

        if (lineaJ2 && posBolaJ2 != Vector3.zero && posBoliche != Vector3.zero)
        {
            lineaJ2.enabled = true;
            lineaJ2.positionCount = 2;
            lineaJ2.SetPosition(0, new Vector3(posBolaJ2.x, 0.05f, posBolaJ2.z));
            lineaJ2.SetPosition(1, new Vector3(posBoliche.x, 0.05f, posBoliche.z));
        }
        else if (lineaJ2)
        {
            lineaJ2.enabled = false;
        }
    }

    private void MostrarPanelResultado()
    {
        if (panelResultado && textoGanador && textoDistanciaGanadora)
        {
            panelResultado.SetActive(true);
            
            string nombreJ1 = datosPerfilJugador1?.nombreUsuario ?? "JUGADOR 1";
            string nombreJ2 = datosPerfilJugador2?.nombreUsuario ?? "JUGADOR 2";
            
            if (distanciaJ1 != -1.0 && (distanciaJ1 < distanciaJ2 || distanciaJ2 == -1.0))
            {
                textoGanador.text = $"¡Ganador: {nombreJ1}!";
                textoGanador.color = new Color32(0x04, 0x6e, 0xc9, 255); // #046ec9
                textoDistanciaGanadora.text = $"Distancia más cercana: {distanciaJ1:F2} m";
            }
            else if (distanciaJ2 != -1.0)
            {
                textoGanador.text = $"¡Ganador: {nombreJ2}!";
                textoGanador.color = new Color32(0xc5, 0x32, 0x2a, 255); // #c5322a
                textoDistanciaGanadora.text = $"Distancia más cercana: {distanciaJ2:F2} m";
            }
            else
            {
                textoGanador.text = "¡Empate!";
                textoDistanciaGanadora.text = "";
            }
        }
    }

    // Métodos públicos para obtener perfiles
    public PerfilUsuario ObtenerPerfilJugador(int numeroJugador)
    {
        return numeroJugador == 1 ? perfilJugador1 : perfilJugador2;
    }

    public DatosPerfilUsuario ObtenerDatosPerfilJugador(int numeroJugador)
    {
        return numeroJugador == 1 ? datosPerfilJugador1 : datosPerfilJugador2;
    }

    public float ObtenerFuerzaAjustada(float fuerzaBase, int numeroJugador)
    {
        var datos = ObtenerDatosPerfilJugador(numeroJugador);
        if (datos != null)
        {
            return fuerzaBase * datos.factorAyuda;
        }
        return fuerzaBase;
    }

    // Métodos para actualizar posiciones (para las líneas)
    public void ActualizarPosicionBola(Vector3 posicion, int jugador)
    {
        if (jugador == 1)
            posBolaJ1 = posicion;
        else if (jugador == 2)
            posBolaJ2 = posicion;
    }

    public void ActualizarPosicionBoliche(Vector3 posicion)
    {
        posBoliche = posicion;
    }

    // Métodos de navegación UI
    public void moverMenu(int movimiento)
    {
        Debug.Log("Movimiento del menú: " + movimiento);
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.MoverMenu(movimiento);
        }
    }

    public void SeleccionarBoton()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.SeleccionarBoton();
        }
    }

    public void LiberarBoton()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.LiberarBoton();
        }
    }

    public void PausarJuego()
    {
        if (!cambioPausa)
        {
            cambioPausa = true;
            mostrarPausa = true;
            canvas.enabled = true;
            GestorUI.Instance.MoverMenu(0);
            StartCoroutine(DesactivarCambioPausa());
        }
    }

    public void SalirMenu()
    {
        if (!cambioPausa)
        {
            mostrarPausa = false;
            canvas.enabled = false;
            cambioPausa = true;
            StartCoroutine(DesactivarCambioPausa());
        }
    }

    private IEnumerator DesactivarCambioPausa()
    {
        yield return new WaitForSeconds(0.5f);
        cambioPausa = false;
    }

    public void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        if (botonSeleccionado == 0)
        {
            SceneManager.LoadScene("PetancaMultijugador");
        }
        else if (botonSeleccionado == 1)
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    void OnDestroy()
    {
        // Limpiar referencias de perfiles seleccionados
        SelectorPerfiles.perfilJugador1 = null;
        SelectorPerfiles.perfilJugador2 = null;
        
        // Limpiar eventos UI
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
        }
    }
}