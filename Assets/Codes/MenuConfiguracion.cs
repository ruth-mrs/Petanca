using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WiimoteApi;
using TMPro;

public class MenuConfiguracion : MonoBehaviour
{
    [Header("Control de Audio")]
    public Slider sliderVolumenGeneral;
    public TextMeshProUGUI textoVolumen;
    
    [Header("Navegación")]
    private string escenaCrearPerfil = "CrearPerfil";
    private string escenaEditarPerfil = "EdicionPerfiles";
    private string escenaMenuPrincipal = "MenuPrincipal";
    private string escenaAccesibilidad = "MenuAccesibilidad";
    
    [Header("Estado de UI")]
    public Button botonEditarPerfil;  // Para habilitarlo/deshabilitarlo según existan perfiles
    public Canvas canvas;
    public bool wiimotebuttonused = false;
    
    private void Start()
    {
        // Cargar volumen guardado o usar valor predeterminado (75%)
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenGeneral", 0.75f);
        
        // Inicializar slider
        if (sliderVolumenGeneral != null)
        {
            sliderVolumenGeneral.value = volumenGuardado;
            ActualizarTextoVolumen(volumenGuardado);
            
            // Asignar listener al cambio de valor
            sliderVolumenGeneral.onValueChanged.AddListener(CambiarVolumen);
        }
        
        // Aplicar el volumen guardado al audio global
        AudioListener.volume = volumenGuardado;
        
        // Verificar estado del botón editar perfil
        ActualizarEstadoBotones();
        
        // Inicializar GestorUI
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }
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

            if (wiimote.Button.one)
            {
                BajarVolumen();
            }
            else if (wiimote.Button.two)
            {
                SubirVolumen();
            }


            if (!wiimote.Button.one && !wiimote.Button.two)
            {
                wiimotebuttonused = false;
            }

            if (wiimote.Button.a)
            {
                GestorUI.Instance.SeleccionarBoton();
            }
             if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.one && !wiimote.Button.two && !wiimote.Button.a)   
            {
                GestorUI.Instance.LiberarBoton();
            }
        }
    }
    
    private void ActualizarEstadoBotones()
    {
        // Verificar si existe el gestor de perfiles y hay perfiles disponibles
        if (GestorPerfiles.Instancia != null)
        {
            // Habilitar el botón de editar solo si hay perfiles
            if (botonEditarPerfil != null)
            {
                int cantidadPerfiles = GestorPerfiles.Instancia.CantidadPerfiles();
                botonEditarPerfil.interactable = cantidadPerfiles > 0;
                
                Debug.Log($"MenuConfiguracion - Perfiles disponibles: {cantidadPerfiles}, Botón editar habilitado: {botonEditarPerfil.interactable}");
            }
        }
        else
        {
            // Si no hay gestor de perfiles, deshabilitar el botón
            if (botonEditarPerfil != null)
            {
                botonEditarPerfil.interactable = false;
                Debug.LogWarning("MenuConfiguracion - GestorPerfiles no disponible, botón editar deshabilitado");
            }
        }
    }
    
    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        switch (botonSeleccionado)
        {
            case 0:
                IrACrearPerfil();
                break;
            case 1:
                IrAEditarPerfil();
                break;
            case 2:
                IrAOpcionesAccesibilidad();
                break;
            case 3:
                VolverAlMenuPrincipal();
                break;
        }
    }    
    
    public void CambiarVolumen(float nuevoVolumen)
    {
        // Actualizar volumen global
        AudioListener.volume = nuevoVolumen;
        
        // Actualizar texto si existe
        ActualizarTextoVolumen(nuevoVolumen);
        
        // Guardar valor para persistencia
        PlayerPrefs.SetFloat("VolumenGeneral", nuevoVolumen);
        PlayerPrefs.Save();
    }

    private void SubirVolumen()
    {
        if (sliderVolumenGeneral != null && !wiimotebuttonused)
        {
            // Incrementar en 5% (0.05) con límite máximo de 1.0
            float nuevoVolumen = Mathf.Clamp(sliderVolumenGeneral.value + 0.05f, 0f, 1f);
            sliderVolumenGeneral.value = nuevoVolumen;
            
            // El listener del slider se encargará de llamar a CambiarVolumen()
            // pero por si acaso, lo llamamos manualmente
            CambiarVolumen(nuevoVolumen);
            wiimotebuttonused = true; // Marcar que se ha usado el botón de Wiimote
        }
    }

    // Método para bajar volumen
    private void BajarVolumen()
    {
        if (sliderVolumenGeneral != null && !wiimotebuttonused)
        {
            // Decrementar en 5% (0.05) con límite mínimo de 0.0
            float nuevoVolumen = Mathf.Clamp(sliderVolumenGeneral.value - 0.05f, 0f, 1f);
            sliderVolumenGeneral.value = nuevoVolumen;
            
            // El listener del slider se encargará de llamar a CambiarVolumen()
            // pero por si acaso, lo llamamos manualmente
            CambiarVolumen(nuevoVolumen);
            wiimotebuttonused = true; // Marcar que se ha usado el botón de Wiimote
        }
    }
    
    private void ActualizarTextoVolumen(float volumen)
    {
        if (textoVolumen != null)
        {
            // Mostrar como porcentaje
            textoVolumen.text = Mathf.RoundToInt(volumen * 100) + "%";
        }
    }
    
    public void IrACrearPerfil()
    {
        // Limpiar PlayerPrefs de edición para asegurar modo creación
        PlayerPrefs.SetInt("ModoEdicion", 0);
        PlayerPrefs.SetInt("IndicePerfilEditar", -1);
        
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        Debug.Log("Navegando a CrearPerfil en modo creación");
        
        // Cargar escena de creación de perfil
        SceneManager.LoadScene(escenaCrearPerfil);
    }

    public void IrAEditarPerfil()
    {
        // Verificar si hay perfiles para editar
        if (GestorPerfiles.Instancia == null)
        {
            Debug.LogWarning("GestorPerfiles no está disponible.");
            return;
        }
        
        int cantidadPerfiles = GestorPerfiles.Instancia.CantidadPerfiles();
        if (cantidadPerfiles == 0)
        {
            Debug.LogWarning("No hay perfiles para editar.");
            return;
        }
        
        // Limpiar PlayerPrefs de edición
        PlayerPrefs.DeleteKey("ModoEdicion");
        PlayerPrefs.DeleteKey("IndicePerfilEditar");
        
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        Debug.Log($"Navegando a EdicionPerfiles con {cantidadPerfiles} perfiles disponibles");
        
        // Cargar escena de edición de perfiles
        SceneManager.LoadScene(escenaEditarPerfil);
    }
    
    public void IrAOpcionesAccesibilidad()
    {
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        Debug.Log("Navegando a MenuAccesibilidad");
        
        // Cargar escena de opciones de accesibilidad
        SceneManager.LoadScene(escenaAccesibilidad);
    }
    
    public void VolverAlMenuPrincipal()
    {
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        Debug.Log("Volviendo al MenuPrincipal");
        
        // Cargar escena del menú principal
        SceneManager.LoadScene("MenuPrincipal");
    }

    private void OnDestroy()
    {
      if (GestorUI.Instance != null)
      {
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
      }
    }
}