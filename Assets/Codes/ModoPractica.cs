using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ModoPractica : MonoBehaviour
{
    [Header("Configuración del Juego")]
    public int bolasRestantes = 3;
    public double distancia = -1.0;

    [Header("UI References")]
    public Canvas canvas;
    public TMP_Text textoTitulo;
    public TMP_Text textoBolas;
    public TMP_Text textoDistancia;
    public TMP_Text textoNombreJugador; // Nuevo para mostrar el nombre del perfil
    public GameObject panelResultado;
    public TMP_Text textoFinJuego;
    public TMP_Text textoDistanciaFinal;

    [Header("Configuración Visual")]
    public Material materialNormal;
    public Material materialResaltado;

    [Header("Estado del Juego")]
    public bool mostrarFinJuego = false;
    public bool mostrarPausa = false;
    private bool cambioPausa = false;

    // Perfil del jugador
    private PerfilUsuario perfilJugador;
    private DatosPerfilUsuario datosPerfilJugador;

    private GameObject bolaMasCercanaActual;

    void Start()
    {
        InicializarPerfil();
        InicializarUI();
        InicializarJuego();
    }

    void InicializarPerfil()
    {
        // Obtener el perfil seleccionado desde SelectorPerfiles
        datosPerfilJugador = SelectorPerfiles.perfilJugador1;
        
        if (datosPerfilJugador == null)
        {
            Debug.LogWarning("No se encontró perfil seleccionado. Usando perfil por defecto.");
            
            // Usar el perfil actual del gestor como fallback
            if (GestorPerfiles.Instancia != null)
            {
                datosPerfilJugador = GestorPerfiles.Instancia.ObtenerPerfilActual();
            }
            
            // Si aún no hay perfil, crear uno por defecto
            if (datosPerfilJugador == null)
            {
                datosPerfilJugador = new DatosPerfilUsuario("Jugador", false, false, 2.5f);
                Debug.Log("Creado perfil por defecto para modo práctica");
            }
        }
        
        // Crear el MonoBehaviour del perfil para el juego
        if (GestorPerfiles.Instancia != null)
        {
            perfilJugador = GestorPerfiles.Instancia.CrearPerfilUsuarioMonoBehaviour(datosPerfilJugador);
            DontDestroyOnLoad(perfilJugador.gameObject); // Mantener durante el juego
        }
        
        Debug.Log($"Modo Práctica iniciado con perfil: {datosPerfilJugador.nombreUsuario}");
        Debug.Log($"- Zurdo: {datosPerfilJugador.esZurdo}");
        Debug.Log($"- Perfil reducido: {datosPerfilJugador.perfilReducido}");
        Debug.Log($"- Fuerza base: {datosPerfilJugador.fuerzaBase}");
        Debug.Log($"- Factor ayuda: {datosPerfilJugador.factorAyuda}");
    }

    void InicializarUI()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            canvas.enabled = false;
        }
    }

    void InicializarJuego()
    {
        // Configurar elementos específicos del perfil
        if (datosPerfilJugador != null)
        {
            // Activar ayudas visuales para perfiles reducidos
            if (datosPerfilJugador.perfilReducido)
            {
                ActivarAyudasVisuales();
            }

            // Configurar cámara según lateralidad
            if (datosPerfilJugador.esZurdo)
            {
                ConfigurarParaZurdo();
            }
        }

        ActualizarUI();
    }

    void ActivarAyudasVisuales()
    {
        // Activar elementos de ayuda visual para perfiles reducidos
        GameObject[] ayudas = GameObject.FindGameObjectsWithTag("AyudaVisual");
        foreach (GameObject ayuda in ayudas)
        {
            ayuda.SetActive(true);
        }
        Debug.Log("Ayudas visuales activadas para perfil reducido");
    }

    void ConfigurarParaZurdo()
    {
        // Configurar vista de cámara para zurdo si es necesario
        var camara = Camera.main;
        if (camara != null)
        {
            // Ejemplo: ajustar la posición X de la cámara
            Vector3 pos = camara.transform.position;
            pos.x = -Mathf.Abs(pos.x);
            camara.transform.position = pos;
        }
        Debug.Log("Configuración aplicada para jugador zurdo");
    }

    void Update()
    {
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoTitulo) textoTitulo.text = "MODO PRÁCTICA";
        
        if (textoBolas) textoBolas.text = $"Bolas restantes: {bolasRestantes}";
        
        if (textoDistancia)
        {
            textoDistancia.text = distancia == -1.0
                ? "Aún falta por lanzar"
                : $"Distancia más cercana: {distancia:F2} m";
        }

        // Mostrar información del perfil
        if (textoNombreJugador && datosPerfilJugador != null)
        {
            string infoJugador = $"<b>{datosPerfilJugador.nombreUsuario}</b>";
            if (datosPerfilJugador.esZurdo) infoJugador += " (Zurdo)";
            if (datosPerfilJugador.perfilReducido) infoJugador += " (Perfil Reducido)";
            
            textoNombreJugador.text = infoJugador;
        }
    }

    public void ActualizarDistancia(double nuevaDistancia)
    {
        distancia = nuevaDistancia;
        ActualizarUI();
    }

   
    public void ActualizarBolaMasCercana(GameObject nuevaBola, double nuevaDistancia)
    {
        if (bolaMasCercanaActual != null && bolaMasCercanaActual != nuevaBola)
        {
            var rendererAnterior = bolaMasCercanaActual.GetComponent<Renderer>();
            if (rendererAnterior != null && materialNormal != null)
            {
                rendererAnterior.material = materialNormal;
            }
        }

        if (nuevaBola != null)
        {
            var rendererNuevo = nuevaBola.GetComponent<Renderer>();
            if (rendererNuevo != null && materialResaltado != null)
            {
                rendererNuevo.material = materialResaltado;
            }
            bolaMasCercanaActual = nuevaBola;
            distancia = nuevaDistancia;
            ActualizarUI();
        }
    }

    public void FinJuego()
    {
        mostrarFinJuego = true;
        canvas.enabled = true;
        MostrarPanelResultado();
    }

    private void MostrarPanelResultado()
    {
        if (panelResultado && textoFinJuego && textoDistanciaFinal)
        {
            panelResultado.SetActive(true);
            
            string nombreJugador = datosPerfilJugador?.nombreUsuario ?? "Jugador";
            textoFinJuego.text = $"¡Fin del juego, {nombreJugador}!";
            
            textoDistanciaFinal.text = distancia == -1.0
                ? "No se registró distancia."
                : $"Distancia más cercana: {distancia:F2} m";
        }
    }

    public void ReducirBolas()
    {
        if (bolasRestantes > 0)
        {
            bolasRestantes--;
            ActualizarUI();
        }
    }

    // Métodos públicos para obtener información del perfil
    public PerfilUsuario ObtenerPerfilJugador()
    {
        return perfilJugador;
    }

    public DatosPerfilUsuario ObtenerDatosPerfilJugador()
    {
        return datosPerfilJugador;
    }

    public float ObtenerFuerzaAjustada(float fuerzaBase)
    {
        if (datosPerfilJugador != null)
        {
            return fuerzaBase * datosPerfilJugador.factorAyuda;
        }
        return fuerzaBase;
    }

    // Métodos de navegación UI
    public void moverMenu(int movimiento)
    {
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
            SceneManager.LoadScene("PetancaSolitario");
        }
        else if (botonSeleccionado == 1)
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    private void OnDestroy()
    {
        // Limpiar referencia del perfil seleccionado
        SelectorPerfiles.perfilJugador1 = null;
        
        // Limpiar eventos UI
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
        }
    }
}