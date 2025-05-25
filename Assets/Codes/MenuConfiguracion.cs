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
    public string escenaCrearPerfil = "CrearPerfil";
    public string escenaEditarPerfil = "EdicionPerfiles";
    public string escenaMenuPrincipal = "MenuPrincipal";
    public string escenaAccesibilidad = "MenuAccesibilidad";
    
    [Header("Estado de UI")]
    public Button botonEditarPerfil;  // Para habilitarlo/deshabilitarlo según existan perfiles
    private GestorUI gestorUI;
    public Canvas canvas;
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
        gestorUI = gameObject.GetComponent<GestorUI>();
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

            if (wiimote.Button.d_up)
            {
                gestorUI.MoverMenu(-1);

            }
            else if (wiimote.Button.d_down)
            {
                gestorUI.MoverMenu(1);
            }
        }

        if (!wiimote.Button.d_up && !wiimote.Button.d_down)
        {
            gestorUI.LiberarBoton(); // Liberar el estado de "botón presionado"
        }


        if (wiimote.Button.a)
        {
            gestorUI.SeleccionarBoton();
        }


    }
    
    private void ActualizarEstadoBotones()
    {
        // Verificar si existe el gestor de perfiles
        if (GestorPerfiles.Instancia != null)
        {
            // Habilitar el botón de editar solo si hay perfiles
            if (botonEditarPerfil != null)
            {
                botonEditarPerfil.interactable = 
                    GestorPerfiles.Instancia.perfilesUsuarios != null && 
                    GestorPerfiles.Instancia.perfilesUsuarios.Count > 0;
            }
        }
        else
        {
            // Si no hay gestor de perfiles, deshabilitar el botón
            if (botonEditarPerfil != null)
                botonEditarPerfil.interactable = false;
        }
    }
    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);

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
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        // Cargar escena de creación de perfil
        SceneManager.LoadScene(escenaCrearPerfil);
    }

    
    public void IrAEditarPerfil()
    {
        // Verificar si hay perfiles para editar
        if (GestorPerfiles.Instancia == null || 
            GestorPerfiles.Instancia.perfilesUsuarios == null || 
            GestorPerfiles.Instancia.perfilesUsuarios.Count == 0)
        {
            Debug.LogWarning("No hay perfiles para editar.");
            return;
        }
        
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        // Cargar escena de edición de perfil
        SceneManager.LoadScene(escenaEditarPerfil);
    }
    
    public void IrAOpcionesAccesibilidad()
    {
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        // Cargar escena de opciones de accesibilidad
        SceneManager.LoadScene(escenaAccesibilidad);
    }
    
    public void VolverAlMenuPrincipal()
    {
        // Guardar cualquier configuración pendiente
        PlayerPrefs.Save();
        
        // Cargar escena del menú principal
        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}