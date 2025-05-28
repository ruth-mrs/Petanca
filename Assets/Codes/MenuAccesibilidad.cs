using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using WiimoteApi;
using UnityEngine.Rendering.PostProcessing;


public class MenuAccesibilidad : MonoBehaviour
{
    
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
    public Button botonDeuteranopia;
    public Button botonTritanopia;
    public Button botonProtanomalia;
    public Button botonDeuteranomalia;
    public Button botonTritanomalia;
    
    [Header("Vista Previa")]
    public Image imagenPrevia;
    public Sprite[] imagenesEjemplo;
    
    [Header("UI")]
    public Canvas canvas;
    
    // Variables privadas
    private Button[] botonesFiltros;
    private TipoDaltonismo filtroSeleccionado = TipoDaltonismo.Normal;
    
    private void Start()
    {
        ConfigurarUI();
        CargarSeleccionActual();
        
        InicializarBotonesFiltros();
        //ConfigurarBotones();
        CargarFiltroGuardado();

        var camara = Camera.main;
        var postLayer = camara?.GetComponent<PostProcessLayer>();
        if (postLayer != null)
        {
            postLayer.volumeLayer = LayerMask.GetMask("PostProcessing");
            Debug.Log("🎯 Se fijó la capa PostProcessing en la cámara.");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró PostProcessLayer en la cámara.");
        }
        
        GestorUI.Instance.Inicializar(canvas);
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
        GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
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

            if (wiimote.Button.d_up)
            {
                GestorUI.Instance.MoverMenu(-1);
            }
            else if (wiimote.Button.d_down)
            {
                GestorUI.Instance.MoverMenu(1);
            }

            if (wiimote.Button.a)
            {
                GestorUI.Instance.SeleccionarBoton();
            }

            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                GestorUI.Instance.LiberarBoton();
            }


        }
    }
    
    private void InicializarBotonesFiltros()
    {
        botonesFiltros = new Button[]
        {
            botonProtanopia,
            botonDeuteranopia,
            botonTritanopia,
            botonProtanomalia,
            botonDeuteranomalia,
            botonTritanomalia
        };
    }
    
    /*
    private void ConfigurarBotones()
    {
        if (botonProtanopia != null)
            botonProtanopia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Protanopia));
        
        if (botonDeuteranopia != null)
            botonDeuteranopia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Deuteranopia));
        
        if (botonTritanopia != null)
            botonTritanopia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Tritanopia));
        
        if (botonProtanomalia != null)
            botonProtanomalia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Protanomalia));
        
        if (botonDeuteranomalia != null)
            botonDeuteranomalia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Deuteranomalia));
        
        if (botonTritanomalia != null)
            botonTritanomalia.onClick.AddListener(() => SeleccionarFiltro(TipoDaltonismo.Tritanomalia));
        
        if (botonAplicarFiltro != null)
            botonAplicarFiltro.onClick.AddListener(AplicarFiltroSeleccionado);
        
        if (botonRestablecerFiltros != null)
            botonRestablecerFiltros.onClick.AddListener(RestablecerFiltros);
        
        if (botonVolverConfiguracion != null)
            botonVolverConfiguracion.onClick.AddListener(VolverAConfiguracion);
    }
    */
    
    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        switch (botonSeleccionado)
        {
            case 0:
                SeleccionarFiltro(TipoDaltonismo.Protanopia);
                break;
            case 1:
                SeleccionarFiltro(TipoDaltonismo.Deuteranopia);
                break;
            case 2:
                SeleccionarFiltro(TipoDaltonismo.Tritanopia);
                break;
            case 3:
                SeleccionarFiltro(TipoDaltonismo.Protanomalia);
                break;
            case 4:
                SeleccionarFiltro(TipoDaltonismo.Deuteranomalia);
                break;
            case 5:
                SeleccionarFiltro(TipoDaltonismo.Tritanomalia);
                break;
            case 6:
                RestablecerFiltros();
                break;
            case 7:
                AplicarFiltroSeleccionado();
                break;
            case 8:
                VolverAConfiguracion();
                break;   
        }        
      
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
        int filtroGuardado = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        filtroSeleccionado = (TipoDaltonismo)filtroGuardado;
        
        ActualizarTextoFiltro(filtroSeleccionado);
        ActualizarVistaPrevia(filtroSeleccionado);
        MostrarFiltroActual();
        ActualizarEstadoBotones();
    }
    
    private void ActualizarEstadoBotones()
    {
        if (botonAplicarFiltro != null)
        {
            int filtroActual = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
            botonAplicarFiltro.interactable = ((int)filtroSeleccionado != filtroActual);
        }
        
        ActualizarVisualizacionBotones();
    }
    
    private void ActualizarVisualizacionBotones()
    {
        if (botonesFiltros == null) return;
        
        TipoDaltonismo[] tiposFiltros = {
            TipoDaltonismo.Protanopia,
            TipoDaltonismo.Deuteranopia,
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
        Debug.Log($"{ObtenerDescripcionFiltro(filtroSeleccionado)}");
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
            TipoDaltonismo.Deuteranopia => "Deuteranopia",
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
            dropdownFiltros.options.Add(new Dropdown.OptionData("Deuteranopia (Ceguera al Verde)"));
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
            case TipoDaltonismo.Deuteranopia: return "Deuteranopia";
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

        private void OnDestroy()
{
    if (GestorUI.Instance != null)
    {
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
    }
}
}