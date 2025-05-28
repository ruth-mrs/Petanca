using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public Wiimote wiimote2;
    public Canvas canvas;
    
    [Header("Botones del Menu (para fallback)")]
    public Button[] botones; // Asigna los botones directamente desde el inspector
    
    private int indiceSeleccionado = 0;
    private bool usandoFallback = false;
    
    void Start()
    {
        // Asegurar que GestorUI esté inicializado
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            Debug.Log("GestorUI inicializado correctamente");
        }
        else
        {
            Debug.LogError("GestorUI.Instance es null. Activando sistema de fallback.");
            ActivarSistemaFallback();
        }
        
        Application.targetFrameRate = 30;

        if(GestorUI.Instance == null)
        {
            GameObject go = new GameObject("GestorUI");
            go.AddComponent<GestorUI>();

            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }else{
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }

    }
    
     void ActivarSistemaFallback()
    {
        usandoFallback = true;
        
        // Si no se asignaron botones en el inspector, buscarlos automáticamente
        if (botones == null || botones.Length == 0)
        {
            botones = FindObjectsOfType<Button>();
            Debug.Log($"Encontrados {botones.Length} botones automáticamente");
        }
        
        // Resaltar el primer botón
        if (botones.Length > 0)
        {
            ResaltarBoton(0);
        }
     }


    void Update()
    {
        // Control con Wiimote únicamente
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
                if (usandoFallback)
                {
                    MoverSeleccionFallback(-1);
                }
                else if (GestorUI.Instance != null)
                {
                    GestorUI.Instance.MoverMenu(-1);
                }
            }
            else if (wiimote.Button.d_down)
            {
                if (usandoFallback)
                {
                    MoverSeleccionFallback(1);
                }
                else if (GestorUI.Instance != null)
                {
                    GestorUI.Instance.MoverMenu(1);
                }
            }

            if (wiimote.Button.a)
            {
                if (usandoFallback)
                {
                    EjecutarBotonFallback();
                }
                else if (GestorUI.Instance != null)
                {
                    GestorUI.Instance.SeleccionarBoton();
                }
            }

            if (!wiimote.Button.d_up && !wiimote.Button.d_down && GestorUI.Instance != null)
            {
                GestorUI.Instance.LiberarBoton();
            }

        }
    }
    
    void MoverSeleccionFallback(int direccion)
    {
        if (botones == null || botones.Length == 0) return;
        
        // Quitar resaltado del botón actual
        QuitarResaltado(indiceSeleccionado);
        
        // Mover índice
        indiceSeleccionado += direccion;
        
        // Wrap around
        if (indiceSeleccionado >= botones.Length)
            indiceSeleccionado = 0;
        else if (indiceSeleccionado < 0)
            indiceSeleccionado = botones.Length - 1;
            
        // Resaltar nuevo botón
        ResaltarBoton(indiceSeleccionado);
        
        Debug.Log($"Selección fallback movida a botón {indiceSeleccionado}");
    }
    
    void EjecutarBotonFallback()
    {
        if (botones == null || botones.Length == 0 || indiceSeleccionado >= botones.Length) return;
        
        Debug.Log($"Ejecutando botón fallback {indiceSeleccionado}");
        
        // Simular click en el botón
        if (botones[indiceSeleccionado] != null)
        {
            botones[indiceSeleccionado].onClick.Invoke();
        }
        else
        {
            // Si el botón no tiene onClick, usar nuestro sistema
            EjecutarOpcionSeleccionada(indiceSeleccionado);
        }
            }

            if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.a)
            {
                GestorUI.Instance.LiberarBoton();
            }
    }
    
    void ResaltarBoton(int indice)
    {
        if (botones == null || indice >= botones.Length || botones[indice] == null) return;
        
        var colors = botones[indice].colors;
        colors.normalColor = Color.yellow;
        botones[indice].colors = colors;
    }
    
    void QuitarResaltado(int indice)
    {
        if (botones == null || indice >= botones.Length || botones[indice] == null) return;
        
        var colors = botones[indice].colors;
        colors.normalColor = Color.white;
        botones[indice].colors = colors;
    }

    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {

        switch (botonSeleccionado)
        {
            case 0:
                IrAlModoPractica();
                break;
            case 1:
                IrAlModoMultijugador();
                break;
            case 2:
                IrAConfiguracion();
                break;
            case 3:
                SalirDelJuego();
                break;
        }
    }

    public void IrAlModoMultijugador()
    {
        Debug.Log("Navegando a modo multijugador");
        
        // Verificar si hay perfiles disponibles
        if (GestorPerfiles.Instancia == null || GestorPerfiles.Instancia.CantidadPerfiles() < 2)
        {
            Debug.LogWarning("Se necesitan al menos 2 perfiles para el modo multijugador");
            return;
        }
        
        // Configurar modo multijugador en SelectorPerfiles
        SelectorPerfiles.esModoMultijugador = true;
        SceneManager.LoadScene("SelectorPerfiles");
    }

    public void IrAlModoPractica()
    {
        Debug.Log("Navegando a modo práctica");
        
        // Verificar si hay perfiles disponibles
        if (GestorPerfiles.Instancia == null || GestorPerfiles.Instancia.CantidadPerfiles() == 0)
        {
            Debug.LogWarning("Se necesita al menos 1 perfil para el modo práctica");
            SceneManager.LoadScene("CrearPerfil");
            return;
        }
        
        // Configurar modo práctica en SelectorPerfiles
        SelectorPerfiles.esModoMultijugador = false;
        SceneManager.LoadScene("SelectorPerfiles");
    }

    public void IrAConfiguracion()
    {
        Debug.Log("Navegando a configuración");
        SceneManager.LoadScene("MenuConfiguracion");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }



    private void OnDestroy()
    {   
    if (GestorUI.Instance != null)
    {
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
    }
    }


}