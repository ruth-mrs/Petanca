using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using WiimoteApi;
using System.Collections.Generic;

public class SelectorPerfiles : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI textoPerfilActual;
    public TextMeshProUGUI textoJugador;
    public TextMeshProUGUI textoInstrucciones;
    public TextMeshProUGUI textoContador; // "Perfil X de Y"
    
    [Header("Botones")]
    public Button botonContinuar;
    public Button botonVolver;
    public Button botonAnterior;
    public Button botonSiguiente;
    
    [Header("Navegación")]
    public float tiempoEsperaBotones = 0.2f;
    public Canvas canvas;
    
    private Wiimote mote;
    private List<DatosPerfilUsuario> perfilesDisponibles;
    private int indicePerfilActual = 0;
    private bool esperandoJugador2 = false;
    
    // Perfiles seleccionados (estáticos para persistir entre escenas)
    public static DatosPerfilUsuario perfilJugador1 = null;
    public static DatosPerfilUsuario perfilJugador2 = null;
    public static bool esModoMultijugador = false;
    
    private float ultimoInputTiempo = 0f;

    void Start()
    {
        InicializarWiimote();
        ConfigurarEventosBotones();
        InicializarGestorUI();
        CargarPerfiles();
        ActualizarUI();
    }

    void InicializarWiimote()
    {
        // Buscar Wiimote disponible
        if (GestorWiimotes.Instance?.wiimote != null)
        {
            mote = GestorWiimotes.Instance.wiimote;
        }
        else
        {
            WiimoteManager.FindWiimotes();
            if (WiimoteManager.Wiimotes.Count > 0)
            {
                mote = WiimoteManager.Wiimotes[0];
                mote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            }
        }
        
        if (mote != null)
        {
            mote.SendPlayerLED(true, false, false, false);
            Debug.Log("Wiimote inicializado para SelectorPerfiles");
        }
        else
        {
            Debug.LogWarning("No se encontró Wiimote para selector de perfiles");
        }
    }

    void ConfigurarEventosBotones()
    {
        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(ConfirmarSeleccion);
            
        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAlMenu);
            
        if (botonAnterior != null)
            botonAnterior.onClick.AddListener(() => CambiarPerfil(-1));
            
        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(() => CambiarPerfil(1));
    }

    void InicializarGestorUI()
    {
        if (GestorUI.Instance != null && canvas != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }
    }

    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);
        // GestorUI ya maneja el onClick automáticamente
    }

    void CargarPerfiles()
    {
        if (GestorPerfiles.Instancia != null)
        {
            perfilesDisponibles = GestorPerfiles.Instancia.ObtenerPerfiles();
        }
        
        if (perfilesDisponibles == null || perfilesDisponibles.Count == 0)
        {
            Debug.LogWarning("No hay perfiles disponibles");
            perfilesDisponibles = new List<DatosPerfilUsuario>();
            // Crear un perfil por defecto
            perfilesDisponibles.Add(new DatosPerfilUsuario("Perfil por defecto", false, false));
        }
        
        // Empezar con el perfil actualmente seleccionado en el gestor
        if (GestorPerfiles.Instancia != null)
        {
            indicePerfilActual = GestorPerfiles.Instancia.indicePerfilActual;
            // Asegurar que el índice está en rango
            if (indicePerfilActual >= perfilesDisponibles.Count)
                indicePerfilActual = 0;
        }
    }

    void Update()
    {
        // Control específico con botones 1 y 2 del Wiimote (independiente del GestorUI)
        if (mote != null)
        {
            mote.ReadWiimoteData();
            
            // Control de navegación con debounce
            if (Time.time - ultimoInputTiempo > tiempoEsperaBotones)
            {
                if (mote.Button.one) // Anterior
                {
                    CambiarPerfil(-1);
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.two) // Siguiente
                {
                    CambiarPerfil(1);
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.b) // Cancelar/Volver
                {
                    VolverAlMenu();
                    ultimoInputTiempo = Time.time;
                }
            }
        }

        // Control del GestorUI para navegación por botones
        Wiimote wiimote = GestorWiimotes.Instance?.wiimote;
        if (wiimote != null)
        {
            if (wiimote.Button.d_up)
            {
                if (GestorUI.Instance != null)
                    GestorUI.Instance.MoverMenu(-1);
            }
            else if (wiimote.Button.d_down)
            {
                if (GestorUI.Instance != null)
                    GestorUI.Instance.MoverMenu(1);
            }

            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                if (GestorUI.Instance != null)
                    GestorUI.Instance.LiberarBoton();
            }

            if (wiimote.Button.a)
            {
                if (GestorUI.Instance != null)
                    GestorUI.Instance.SeleccionarBoton();
            }
        }
    }

    void CambiarPerfil(int direccion)
    {
        if (perfilesDisponibles.Count == 0) return;
        
        indicePerfilActual += direccion;
        
        if (indicePerfilActual >= perfilesDisponibles.Count)
            indicePerfilActual = 0;
        else if (indicePerfilActual < 0)
            indicePerfilActual = perfilesDisponibles.Count - 1;
            
        ActualizarUI();
        ActualizarLEDsWiimote();
        
        Debug.Log($"Perfil cambiado a índice {indicePerfilActual}: {perfilesDisponibles[indicePerfilActual].nombreUsuario}");
    }

    void ActualizarLEDsWiimote()
    {
        if (mote == null) return;
        
        // Usar los LEDs para mostrar el índice del perfil actual
        mote.SendPlayerLED(
            indicePerfilActual % 4 == 0,
            indicePerfilActual % 4 == 1,
            indicePerfilActual % 4 == 2,
            indicePerfilActual % 4 == 3
        );
    }

    void ActualizarUI()
    {
        if (perfilesDisponibles.Count == 0) return;
        
        DatosPerfilUsuario perfilActual = perfilesDisponibles[indicePerfilActual];
        
        // Información del perfil
        if (textoPerfilActual != null)
        {
            string info = $"<b>{perfilActual.nombreUsuario}</b>\n\n";
            info += $"<color=#FFD700>Lateralidad:</color> {(perfilActual.esZurdo ? "Zurdo" : "Diestro")}\n";
            info += $"<color=#FFD700>Tipo:</color> {(perfilActual.perfilReducido ? "Reducido" : "Amplio")}\n";
            info += $"<color=#FFD700>Fuerza base:</color> {perfilActual.fuerzaBase:F1}\n";
            info += $"<color=#FFD700>Aceleración máxima:</color> {perfilActual.aceleracionMaximaCalibrada:F2}\n";
            info += $"<color=#FFD700>Factor de ayuda:</color> {perfilActual.factorAyuda:F2}x";
            
            textoPerfilActual.text = info;
        }
        
        // Contador de perfiles
        if (textoContador != null)
        {
            textoContador.text = $"Perfil {indicePerfilActual + 1} de {perfilesDisponibles.Count}";
        }
        
        // Indicador de jugador
        if (textoJugador != null)
        {
            if (!esperandoJugador2)
            {
                textoJugador.text = esModoMultijugador ? "<color=#00FF00>JUGADOR 1</color>" : "<color=#00BFFF>JUGADOR</color>";
            }
            else
            {
                textoJugador.text = "<color=#FF6347>JUGADOR 2</color>";
            }
        }
        
        // Instrucciones
        if (textoInstrucciones != null)
        {
            string instrucciones = "<color=#FFFF00>Controles:</color>\n";
            instrucciones += "• Botón 1/2: Cambiar perfil\n";
            instrucciones += "• D-Pad ↑/↓: Navegar botones\n";
            instrucciones += "• Botón A: Confirmar\n";
            instrucciones += "• Botón B: Volver al menú";
            
            textoInstrucciones.text = instrucciones;
        }
    }

    public void ConfirmarSeleccion()
    {
        if (perfilesDisponibles.Count == 0) return;
        
        DatosPerfilUsuario perfilSeleccionado = perfilesDisponibles[indicePerfilActual];
        
        if (!esperandoJugador2)
        {
            // Selección del jugador 1
            perfilJugador1 = perfilSeleccionado;
            
            if (esModoMultijugador)
            {
                // Pasar a selección del jugador 2
                esperandoJugador2 = true;
                indicePerfilActual = 0; // Resetear selección
                ActualizarUI();
                
                // Cambiar LEDs para indicar jugador 2
                if (mote != null)
                    mote.SendPlayerLED(false, true, false, false);
                    
                Debug.Log($"Jugador 1 seleccionado: {perfilJugador1.nombreUsuario}. Ahora selecciona Jugador 2.");
            }
            else
            {
                // Ir directamente al modo práctica
                Debug.Log($"Perfil seleccionado para modo práctica: {perfilSeleccionado.nombreUsuario}");
                IniciarJuego();
            }
        }
        else
        {
            // Selección del jugador 2
            perfilJugador2 = perfilSeleccionado;
            Debug.Log($"Jugador 2 seleccionado: {perfilJugador2.nombreUsuario}. Iniciando multijugador.");
            IniciarJuego();
        }
    }

    void IniciarJuego()
    {
        if (esModoMultijugador)
        {
            Debug.Log($"Iniciando multijugador - J1: {perfilJugador1?.nombreUsuario}, J2: {perfilJugador2?.nombreUsuario}");
            SceneManager.LoadScene("ModoMultijugador");
        }
        else
        {
            Debug.Log($"Iniciando modo práctica - Jugador: {perfilJugador1?.nombreUsuario}");
            SceneManager.LoadScene("ModoPractica");
        }
    }

    void VolverAlMenu()
    {
        // Limpiar selecciones
        perfilJugador1 = null;
        perfilJugador2 = null;
        esperandoJugador2 = false;
        
        Debug.Log("Volviendo al menú principal");
        SceneManager.LoadScene("MenuPrincipal");
    }

    // Método público para configurar el modo desde otra escena
    public void ConfigurarModoJuego(bool multijugador)
    {
        esModoMultijugador = multijugador;
        esperandoJugador2 = false;
        
        Debug.Log($"Modo configurado: {(multijugador ? "Multijugador" : "Práctica")}");
        ActualizarUI();
    }

    void OnDestroy()
    {
        // Limpiar eventos
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
        }
        
        // No limpiar el Wiimote si lo gestiona GestorWiimotes
        if (mote != null && GestorWiimotes.Instance?.wiimote == null)
        {
            WiimoteManager.Cleanup(mote);
        }
    }
}