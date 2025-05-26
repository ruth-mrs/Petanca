using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using WiimoteApi;

public class MenuAccesibilidad : MonoBehaviour
{
    public enum TipoDaltonismo
    {
        Normal = 0,
        Protanopia = 1,
        Deuteronopia = 2,
        Tritanopia = 3,
        Protanomalia = 4,
        Deuteranomalia = 5,
        Tritanomalia = 6
    }
    
    [Header("UI Components")]
    public Dropdown dropdownFiltros;
    public Button botonAplicar;
    public Text textoEstado;
    
    [Header("Navegación")]
    public string escenaConfiguracion = "MenuConfiguracion";
    
    [Header("Filtros de Daltonismo")]
    public TextMeshProUGUI textoFiltroActual;
    public Button botonAplicarFiltro;
    public Button botonRestablecerFiltros;
    public Button botonVolverConfiguracion;
    
    [Header("Botones de Filtros")]
    public Button botonProtanopia;
    public Button botonDeuteronopia;
    public Button botonTritanopia;
    public Button botonProtanomalia;
    public Button botonDeuteranomalia;
    public Button botonTritanomalia;
    
    [Header("Vista Previa")]
    public Image imagenPrevia;
    public Sprite[] imagenesEjemplo;
    
    [Header("UI")]
    private GestorUI gestorUI;
    public Canvas canvas;
    
    // Variables privadas
    private Button[] botonesFiltros;
    private TipoDaltonismo filtroSeleccionado = TipoDaltonismo.Normal;
    
    private void Start()
    {
        ConfigurarUI();
        CargarSeleccionActual();
        
        InicializarBotonesFiltros();
        ConfigurarBotones();
        CargarFiltroGuardado();
        
        // Configurar GestorUI
        gestorUI = gameObject.GetComponent<GestorUI>();
        if (gestorUI == null)
        {
            gestorUI = gameObject.AddComponent<GestorUI>();
        }
        gestorUI.Inicializar(canvas);
        gestorUI.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
    }
    
    void Update()
    {
        Wiimote wiimote = GestorWiimotes.Instance?.wiimote;

        if (wiimote != null)
        {
            int ret;
            do
            {
                ret = wiimote.ReadWiimoteData();
            } while (ret > 0);

            // Control de navegación con D-pad
            if (wiimote.Button.d_up)
            {
                gestorUI.MoverMenu(-1);
            }
            else if (wiimote.Button.d_down)
            {
                gestorUI.MoverMenu(1);
            }

            // Liberar estado de botones
            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                gestorUI.LiberarBoton();
            }

            // Seleccionar con botón A
            if (wiimote.Button.a)
            {
                gestorUI.SeleccionarBoton();
            }
        }
        else
        {
            // Keyboard/Mouse fallback controls
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                gestorUI.MoverMenu(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                gestorUI.MoverMenu(1);
            }

            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && 
                !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                gestorUI.LiberarBoton();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                gestorUI.SeleccionarBoton();
            }
        }
    }
    
    private void InicializarBotonesFiltros()
    {
        // Crear array con los botones de filtros de daltonismo
        botonesFiltros = new Button[]
        {
            botonProtanopia,
            botonDeuteronopia,
            botonTritanopia,
            botonProtanomalia,
            botonDeuteranomalia,
            botonTritanomalia
        };
    }
    
    private void ConfigurarBotones()
    {
        // Configurar botones de filtros
        if (botonProtanopia != null)
            botonProtanopia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Protanopia));
        
        if (botonDeuteronopia != null)
            botonDeuteronopia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Deuteronopia));
        
        if (botonTritanopia != null)
            botonTritanopia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Tritanopia));
        
        if (botonProtanomalia != null)
            botonProtanomalia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Protanomalia));
        
        if (botonDeuteranomalia != null)
            botonDeuteranomalia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Deuteranomalia));
        
        if (botonTritanomalia != null)
            botonTritanomalia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Tritanomalia));
        
        // Otros botones
        if (botonAplicarFiltro != null)
            botonAplicarFiltro.onClick.AddListener(AplicarFiltroSeleccionado);
        
        if (botonRestablecerFiltros != null)
            botonRestablecerFiltros.onClick.AddListener(RestablecerFiltros);
        
        if (botonVolverConfiguracion != null)
            botonVolverConfiguracion.onClick.AddListener(VolverAConfiguracion);
    }
    
    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);
        // El GestorUI ya ejecuta el onClick automáticamente
    }
    
    private void SeleccionarFiltro(TipoDaltonismo tipo)
    {
        filtroSeleccionado = tipo;
        ActualizarTextoFiltro(tipo);
        ActualizarVistaPrevia(tipo);
        ActualizarEstadoBotones();
        
        Debug.Log($"Filtro seleccionado: {ObtenerDescripcionFiltro(tipo)}");
    }
    
    private void CargarFiltroGuardado()
    {
        // Cargar filtro guardado
        int filtroGuardado = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        filtroSeleccionado = (TipoDaltonismo)filtroGuardado;
        
        ActualizarTextoFiltro(filtroSeleccionado);
        ActualizarVistaPrevia(filtroSeleccionado);
        MostrarFiltroActual();
        ActualizarEstadoBotones();
    }
    
    private void ActualizarEstadoBotones()
    {
        // Habilitar botón aplicar si es diferente al actual
        if (botonAplicarFiltro != null)
        {
            int filtroActual = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
            botonAplicarFiltro.interactable = ((int)filtroSeleccionado != filtroActual);
        }
        
        // Actualizar colores de botones de filtros para mostrar cuál está seleccionado
        ActualizarVisualizacionBotones();
    }
    
    private void ActualizarVisualizacionBotones()
    {
        if (botonesFiltros == null) return;
        
        // Solo actualizar botones de filtros de daltonismo
        TipoDaltonismo[] tiposFiltros = {
            TipoDaltonismo.Protanopia,
            TipoDaltonismo.Deuteronopia,
            TipoDaltonismo.Tritanopia,
            TipoDaltonismo.Protanomalia,
            TipoDaltonismo.Deuteranomalia,
            TipoDaltonismo.Tritanomalia
        };
        
        for (int i = 0; i < botonesFiltros.Length && i < tiposFiltros.Length; i++)
        {
            if (botonesFiltros[i] != null)
            {
                var colors = botonesFiltros[i].colors;
                
                if (tiposFiltros[i] == filtroSeleccionado)
                {
                    // Botón seleccionado - color verde claro
                    colors.normalColor = new Color(0.7f, 1f, 0.7f, 1f);
                }
                else
                {
                    // Botón no seleccionado - color normal
                    colors.normalColor = Color.white;
                }
                
                botonesFiltros[i].colors = colors;
            }
        }
    }
    
    public void AplicarFiltroSeleccionado()
    {
        // Verificar si hay gestor de accesibilidad
        if (GestorAccesibilidad.Instancia == null)
        {
            Debug.LogWarning("No se encontró el Gestor de Accesibilidad. Asegúrate de que esté en la escena del menú principal.");
            return;
        }
        
        // Aplicar filtro
        GestorAccesibilidad.Instancia.AplicarFiltro(filtroSeleccionado);
        
        // Guardar configuración
        PlayerPrefs.SetInt("FiltroAccesibilidad", (int)filtroSeleccionado);
        PlayerPrefs.Save();
        
        // Actualizar estado de botones
        ActualizarEstadoBotones();
        
        // Actualizar texto para mostrar que se aplicó
        MostrarFiltroActual();
        
        // Mostrar confirmación
        Debug.Log($"Filtro aplicado: {ObtenerDescripcionFiltro(filtroSeleccionado)}");
    }
    
    private void MostrarFiltroActual()
    {
        int filtroActual = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        TipoDaltonismo tipoActual = (TipoDaltonismo)filtroActual;
        
        if (textoFiltroActual != null)
        {
            string descripcion = ObtenerDescripcionFiltro(tipoActual);
            textoFiltroActual.text = $"Filtro aplicado: {descripcion}";
        }
    }
    
    private void ActualizarTextoFiltro(TipoDaltonismo tipo)
    {
        // Solo mostrar como seleccionado si no es el mismo que el aplicado
        int filtroActual = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        if ((int)tipo != filtroActual)
        {
            if (textoFiltroActual != null)
            {
                string descripcion = ObtenerDescripcionFiltro(tipo);
                textoFiltroActual.text = $"Filtro seleccionado: {descripcion}";
            }
        }
        else
        {
            MostrarFiltroActual();
        }
    }
    
    private string ObtenerDescripcionFiltro(TipoDaltonismo tipo)
    {
        return tipo switch
        {
            TipoDaltonismo.Normal => "Normal",
            TipoDaltonismo.Protanopia => "Protanopia",
            TipoDaltonismo.Deuteronopia => "Deuteronopia",
            TipoDaltonismo.Tritanopia => "Tritanopia",
            TipoDaltonismo.Protanomalia => "Protanomalia",
            TipoDaltonismo.Deuteranomalia => "Deuteranomalia",
            TipoDaltonismo.Tritanomalia => "Tritanomalia",
            _ => "Desconocido"
        };
    }
    
    private void ActualizarVistaPrevia(TipoDaltonismo tipo)
    {
        if (imagenPrevia != null && imagenesEjemplo != null && imagenesEjemplo.Length > 0)
        {
            // Cambiar imagen de ejemplo según el filtro
            int indiceImagen = Mathf.Min((int)tipo, imagenesEjemplo.Length - 1);
            imagenPrevia.sprite = imagenesEjemplo[indiceImagen];
        }
    }
    
    private void ConfigurarUI()
    {
        if (dropdownFiltros != null)
        {
            dropdownFiltros.options.Clear();
            dropdownFiltros.options.Add(new Dropdown.OptionData("Normal"));
            dropdownFiltros.options.Add(new Dropdown.OptionData("Protanopia (Ceguera al Rojo)"));
            dropdownFiltros.options.Add(new Dropdown.OptionData("Deuteronopia (Ceguera al Verde)"));
            dropdownFiltros.options.Add(new Dropdown.OptionData("Tritanopia (Ceguera al Azul)"));
            dropdownFiltros.options.Add(new Dropdown.OptionData("Protanomalia (Deficiencia al Rojo)"));
            dropdownFiltros.options.Add(new Dropdown.OptionData("Deuteranomalia (Deficiencia al Verde)"));
            dropdownFiltros.options.Add(new Dropdown.OptionData("Tritanomalia (Deficiencia al Azul)"));
            
            dropdownFiltros.onValueChanged.AddListener(OnFiltroSeleccionado);
        }
        
        if (botonAplicar != null)
        {
            botonAplicar.onClick.AddListener(AplicarFiltroSeleccionado);
        }
    }
    
    private void CargarSeleccionActual()
    {
        int filtroGuardado = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        if (dropdownFiltros != null)
        {
            dropdownFiltros.value = filtroGuardado;
        }
        ActualizarTextoEstado((TipoDaltonismo)filtroGuardado);
    }
    
    public void OnFiltroSeleccionado(int indice)
    {
        TipoDaltonismo tipo = (TipoDaltonismo)indice;
        ActualizarTextoEstado(tipo);
    }
    
    private void ActualizarTextoEstado(TipoDaltonismo tipo)
    {
        if (textoEstado != null)
        {
            textoEstado.text = $"Filtro actual: {ObtenerNombreFiltro(tipo)}";
        }
    }
    
    private string ObtenerNombreFiltro(TipoDaltonismo tipo)
    {
        switch (tipo)
        {
            case TipoDaltonismo.Normal: return "Normal";
            case TipoDaltonismo.Protanopia: return "Protanopia";
            case TipoDaltonismo.Deuteronopia: return "Deuteronopia";
            case TipoDaltonismo.Tritanopia: return "Tritanopia";
            case TipoDaltonismo.Protanomalia: return "Protanomalia";
            case TipoDaltonismo.Deuteranomalia: return "Deuteranomalia";
            case TipoDaltonismo.Tritanomalia: return "Tritanomalia";
            default: return "Desconocido";
        }
    }
    
    public void VolverAConfiguracion()
    {
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        // Cargar escena de configuración
        SceneManager.LoadScene(escenaConfiguracion);
    }
    
    public void RestablecerFiltros()
    {
        // Restablecer a normal (sin filtro)
        filtroSeleccionado = TipoDaltonismo.Normal;
        
        // Aplicar inmediatamente el filtro normal
        if (GestorAccesibilidad.Instancia != null)
        {
            GestorAccesibilidad.Instancia.AplicarFiltro(filtroSeleccionado);
            PlayerPrefs.SetInt("FiltroAccesibilidad", (int)filtroSeleccionado);
            PlayerPrefs.Save();
        }
        
        ActualizarTextoFiltro(filtroSeleccionado);
        ActualizarVistaPrevia(filtroSeleccionado);
        ActualizarEstadoBotones();
        
        Debug.Log("Filtros restablecidos a Normal");
    }
}