using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuAccesibilidad : MonoBehaviour
{
    [Header("Navegación")]
    public string escenaConfiguracion = "MenuConfiguracion";
    
    [Header("Filtros de Daltonismo")]
    public TMP_Dropdown dropdownFiltros;
    public TextMeshProUGUI textoFiltroActual;
    public Button botonAplicarFiltro;
    
    [Header("Vista Previa")]
    public Image imagenPrevia;
    public Sprite[] imagenesEjemplo; // Imágenes para mostrar el efecto
    
    // Enum para los tipos de daltonismo
    public enum TipoDaltonismo
    {
        Normal,
        Protanopia,    // Ceguera al rojo
        Deuteronopia,  // Ceguera al verde
        Tritanopia,    // Ceguera al azul
        Protanomalia,  // Deficiencia al rojo
        Deuteranomalia, // Deficiencia al verde
        Tritanomalia   // Deficiencia al azul
    }
    
    private TipoDaltonismo filtroSeleccionado;
    
    private void Start()
    {
        ConfigurarDropdown();
        CargarFiltroGuardado();
        ConfigurarBotones();
    }
    
    private void ConfigurarDropdown()
    {
        if (dropdownFiltros != null)
        {
            // Limpiar opciones existentes
            dropdownFiltros.ClearOptions();
            
            // Agregar opciones de filtros
            var opciones = new System.Collections.Generic.List<string>
            {
                "Normal (Sin filtro)",
                "Protanopia (Ceguera al rojo)",
                "Deuteronopia (Ceguera al verde)",
                "Tritanopia (Ceguera al azul)",
                "Protanomalia (Deficiencia al rojo)",
                "Deuteranomalia (Deficiencia al verde)",
                "Tritanomalia (Deficiencia al azul)"
            };
            
            dropdownFiltros.AddOptions(opciones);
            dropdownFiltros.onValueChanged.AddListener(OnDropdownChanged);
        }
    }
    
    private void ConfigurarBotones()
    {
        if (botonAplicarFiltro != null)
        {
            botonAplicarFiltro.onClick.AddListener(AplicarFiltroSeleccionado);
        }
    }
    
    private void CargarFiltroGuardado()
    {
        // Cargar filtro guardado
        int filtroGuardado = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        
        if (dropdownFiltros != null)
        {
            dropdownFiltros.value = filtroGuardado;
        }
        
        filtroSeleccionado = (TipoDaltonismo)filtroGuardado;
        ActualizarTextoFiltro(filtroSeleccionado);
        ActualizarVistaPrevia(filtroSeleccionado);
        
        // Mostrar el filtro actualmente aplicado
        MostrarFiltroActual();
    }
    
    private void OnDropdownChanged(int indice)
    {
        filtroSeleccionado = (TipoDaltonismo)indice;
        ActualizarTextoFiltro(filtroSeleccionado);
        ActualizarVistaPrevia(filtroSeleccionado);
        
        // Habilitar botón aplicar si es diferente al actual
        if (botonAplicarFiltro != null)
        {
            int filtroActual = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
            botonAplicarFiltro.interactable = (indice != filtroActual);
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
        
        // Deshabilitar botón aplicar
        if (botonAplicarFiltro != null)
        {
            botonAplicarFiltro.interactable = false;
        }
        
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
        if (textoFiltroActual != null)
        {
            string descripcion = ObtenerDescripcionFiltro(tipo);
            textoFiltroActual.text = $"Filtro seleccionado: {descripcion}";
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
    
    public void VolverAConfiguracion()
    {
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        // Cargar escena de configuración
        SceneManager.LoadScene(escenaConfiguracion);
    }
    
    public void RestablecerFiltros()
    {
        // Restablecer a normal
        dropdownFiltros.value = 0;
        filtroSeleccionado = TipoDaltonismo.Normal;
        OnDropdownChanged(0);
    }
}