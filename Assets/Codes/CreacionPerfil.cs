using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using WiimoteApi;

public class CreacionPerfil : MonoBehaviour
{
    [Header("Navegación")]
    public string escenaConfiguracion = "MenuConfiguracion";

    [Header("Paneles del Proceso")]
    public GameObject panelLateralidad;
    public GameObject panelTipoPerfil;
    public GameObject panelCalibracion;
    public GameObject panelNombrePerfil;

    [Header("UI de Lateralidad")]
    public Button botonDiestro;
    public Button botonZurdo;

    [Header("UI de Tipo de Perfil")]
    public Button botonAmplio;
    public Button botonReducido;

    [Header("UI de Calibración")]
    public TextMeshProUGUI textoInstrucciones;
    public GameObject panelPrincipalCalibracion;
    public GameObject panelProcesoCalibracion;
    public RectTransform indicadorCargaCircular;
    public TextMeshProUGUI textoEstadoCalibracion;
    public Button botonComenzarCalibracion;

    [Header("Información del Perfil")]
    public TextMeshProUGUI textoTipoPerfil;
    public TextMeshProUGUI textoLateralidad;
    public Image imagenPerfilMovimiento;
    public Sprite imagenPerfilReducido;
    public Sprite imagenPerfilAmplio;
    public RectTransform contenedorImagen;

    [Header("UI de Nombre")]
    public TMP_InputField campoNombre;
    public Button botonNombreAleatorio;
    public Button botonGuardarPerfil;

    [Header("UI General")]
    public TextMeshProUGUI tituloPanel;
    public Button botonVolver;
    public Canvas canvas;

    // Variables para almacenar las selecciones
    private bool esZurdo = false;
    private bool perfilReducido = false;
    private float aceleracionMaximaCaptada = 0f;

    // Referencia al Wiimote
    private Wiimote mote;
    private bool calibracionEnCurso = false;

    // Modo edición
    private bool modoEdicion = false;
    private PerfilUsuario perfilEdicion = null;

    // GestorUI
    private GestorUI gestorUI;

    private void Start()
    {
        // Verificar si hay Wiimote disponible
        if (WiimoteManager.Wiimotes.Count > 0)
        {
            mote = WiimoteManager.Wiimotes[0];
        }
        else
        {
            Debug.LogWarning("No se detectó ningún Wiimote. La calibración podría no funcionar correctamente.");
        }

        // Configurar GestorUI
        gestorUI = gameObject.GetComponent<GestorUI>();
        if (gestorUI == null)
        {
            gestorUI = gameObject.AddComponent<GestorUI>();
        }

        // Comprobar si estamos en modo edición
        modoEdicion = PlayerPrefs.GetInt("ModoEdicion", 0) == 1;

        if (modoEdicion && tituloPanel != null)
        {
            tituloPanel.text = "Editar Perfil";
        }

        // Cargar datos del perfil a editar si estamos en modo edición
        if (modoEdicion)
        {
            int indicePerfil = PlayerPrefs.GetInt("IndicePerfilEditar", -1);

            if (indicePerfil >= 0 && indicePerfil < GestorPerfiles.Instancia.perfilesUsuarios.Count)
            {
                perfilEdicion = GestorPerfiles.Instancia.perfilesUsuarios[indicePerfil];

                // Precargar valores existentes
                esZurdo = perfilEdicion.esZurdo;
                perfilReducido = perfilEdicion.perfilReducido;
                aceleracionMaximaCaptada = perfilEdicion.aceleracionMaximaCalibrada;

                if (campoNombre != null)
                    campoNombre.text = perfilEdicion.nombreUsuario;

                // Iniciar desde lateralidad
                MostrarPanelLateralidad();

                // Actualizar UI según los valores cargados
                ActualizarBotonesLateralidad();
                ActualizarBotonesTipoPerfil();
                ActualizarInfoPerfilCalibración();
            }
            else
            {
                Debug.LogError("No se pudo encontrar el perfil para editar");
                modoEdicion = false;
                MostrarPanelLateralidad();
            }
        }
        else
        {
            // Iniciar en el primer panel para creación normal
            MostrarPanelLateralidad();
        }

        // Configurar eventos de botones
        ConfigurarEventosBotones();
    }

    void Update()
    {
        // Control de calibración
        if (calibracionEnCurso && mote != null)
        {
            mote.ReadWiimoteData();
        }

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
                if (gestorUI != null)
                {
                    gestorUI.MoverMenu(-1);
                }
            }
            else if (wiimote.Button.d_down)
            {
                if (gestorUI != null)
                {
                    gestorUI.MoverMenu(1);
                }
            }

            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                if (gestorUI != null)
                {
                    gestorUI.LiberarBoton();
                }
            }

            if (wiimote.Button.a)
            {
                if (gestorUI != null)
                {
                    gestorUI.SeleccionarBoton();
                }
            }
        }
        else
        {
            // Mouse/Keyboard fallback
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (gestorUI != null)
                {
                    gestorUI.MoverMenu(-1);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (gestorUI != null)
                {
                    gestorUI.MoverMenu(1);
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (gestorUI != null)
                {
                    gestorUI.SeleccionarBoton();
                }
            }

            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && 
                !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                if (gestorUI != null)
                {
                    gestorUI.LiberarBoton();
                }
            }
        }
    }

    // Actualizar la información mostrada en el panel de calibración
    private void ActualizarInfoPerfilCalibración()
    {
        // Actualizar texto del tipo de perfil
        if (textoTipoPerfil != null)
        {
            textoTipoPerfil.text = perfilReducido ? "Perfil reducido" : "Perfil amplio";
        }

        // Actualizar texto de lateralidad
        if (textoLateralidad != null)
        {
            textoLateralidad.text = esZurdo ? "Zurdo" : "Diestro";
        }

        // Actualizar imagen según el tipo de perfil
        if (imagenPerfilMovimiento != null)
        {
            imagenPerfilMovimiento.sprite = perfilReducido ? imagenPerfilReducido : imagenPerfilAmplio;
        }

        // Girar imagen según lateralidad si es necesario
        if (contenedorImagen != null)
        {
            // Para zurdo, girar horizontalmente
            contenedorImagen.localScale = new Vector3(
                esZurdo ? -1 : 1, // Escala X invertida para zurdo
                1,
                1);
        }
    }

    private void ConfigurarEventosBotones()
    {
        // Botones de lateralidad
        if (botonDiestro != null)
            botonDiestro.onClick.AddListener(() => SeleccionarLateralidad(false));

        if (botonZurdo != null)
            botonZurdo.onClick.AddListener(() => SeleccionarLateralidad(true));

        // Botones de tipo de perfil
        if (botonAmplio != null)
            botonAmplio.onClick.AddListener(() => SeleccionarTipoPerfil(false));

        if (botonReducido != null)
            botonReducido.onClick.AddListener(() => SeleccionarTipoPerfil(true));

        // Botón de calibración
        if (botonComenzarCalibracion != null)
            botonComenzarCalibracion.onClick.AddListener(ComenzarCalibracion);

        // Botones de nombre
        if (botonNombreAleatorio != null)
            botonNombreAleatorio.onClick.AddListener(GenerarNombreAleatorio);

        if (botonGuardarPerfil != null)
            botonGuardarPerfil.onClick.AddListener(GuardarPerfil);

        // Botón volver
        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAConfiguracion);
    }

    // Mostrar solo el panel adecuado
    private void MostrarSoloPanelActual(GameObject panelActual)
    {
        if (panelLateralidad != null)
            panelLateralidad.SetActive(panelActual == panelLateralidad);

        if (panelTipoPerfil != null)
            panelTipoPerfil.SetActive(panelActual == panelTipoPerfil);

        if (panelCalibracion != null)
            panelCalibracion.SetActive(panelActual == panelCalibracion);

        if (panelNombrePerfil != null)
            panelNombrePerfil.SetActive(panelActual == panelNombrePerfil);

        // Reinicializar GestorUI para el panel actual
        InicializarGestorUIParaPanel(panelActual);
    }

    private void InicializarGestorUIParaPanel(GameObject panelActual)
    {
        if (gestorUI != null && canvas != null)
        {
            // Remove old event handler only if it was previously assigned
            gestorUI.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
            
            // Wait one frame to ensure the panel is fully active before initializing
            StartCoroutine(InicializarGestorUIConRetraso());
        }
    }

    private System.Collections.IEnumerator InicializarGestorUIConRetraso()
    {
        yield return null; // Wait one frame
        
        if (gestorUI != null && canvas != null)
        {
            try
            {
                gestorUI.Inicializar(canvas);
                gestorUI.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error initializing GestorUI: {ex.Message}");
            }
        }
    }

    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);
        // El GestorUI ya ejecuta el onClick automáticamente
    }

    // Navegación entre paneles
    public void MostrarPanelLateralidad()
    {
        MostrarSoloPanelActual(panelLateralidad);
        StartCoroutine(ActualizarBotonesConRetraso(() => ActualizarBotonesLateralidad()));
    }

    public void MostrarPanelTipoPerfil()
    {
        MostrarSoloPanelActual(panelTipoPerfil);
        StartCoroutine(ActualizarBotonesConRetraso(() => ActualizarBotonesTipoPerfil()));
    }

    public void MostrarPanelCalibracion()
    {
        MostrarSoloPanelActual(panelCalibracion);

        // Mostrar panel principal de calibración y ocultar el de proceso
        if (panelPrincipalCalibracion != null)
            panelPrincipalCalibracion.SetActive(true);

        if (panelProcesoCalibracion != null)
            panelProcesoCalibracion.SetActive(false);

        // Preparar UI de calibración
        if (textoInstrucciones != null)
        {
            textoInstrucciones.text = "Vamos a continuar con la calibración,\n" +
                "realice el movimiento de forma que le resulte natural\n" +
                "a semejanza del perfil seleccionado";
        }

        // Actualizar información del perfil seleccionado
        ActualizarInfoPerfilCalibración();
    }

    public void MostrarPanelNombre()
    {
        MostrarSoloPanelActual(panelNombrePerfil);

        // En modo edición, mostrar nombre existente
        if (modoEdicion && perfilEdicion != null && campoNombre != null)
        {
            campoNombre.text = perfilEdicion.nombreUsuario;
        }
    }

    private System.Collections.IEnumerator ActualizarBotonesConRetraso(System.Action accionActualizar)
    {
        yield return null; // Wait one frame for the panel to be fully active
        accionActualizar?.Invoke();
    }

    // Selección de lateralidad
    public void SeleccionarLateralidad(bool zurdo)
    {
        esZurdo = zurdo;
        ActualizarBotonesLateralidad();

        // Avanzar al siguiente paso
        MostrarPanelTipoPerfil();
    }

    // Selección de tipo de perfil
    public void SeleccionarTipoPerfil(bool reducido)
    {
        perfilReducido = reducido;
        ActualizarBotonesTipoPerfil();

        // Avanzar al siguiente paso
        MostrarPanelCalibracion();
    }

    // Actualizar UI según selecciones
    private void ActualizarBotonesLateralidad()
    {
        if (botonDiestro != null)
        {
            ColorBlock colores = botonDiestro.colors;
            colores.normalColor = !esZurdo ? new Color(0.7f, 0.9f, 1f) : Color.white;
            botonDiestro.colors = colores;
        }

        if (botonZurdo != null)
        {
            ColorBlock colores = botonZurdo.colors;
            colores.normalColor = esZurdo ? new Color(0.7f, 0.9f, 1f) : Color.white;
            botonZurdo.colors = colores;
        }
    }

    private void ActualizarBotonesTipoPerfil()
    {
        if (botonAmplio != null)
        {
            ColorBlock colores = botonAmplio.colors;
            colores.normalColor = !perfilReducido ? new Color(0.7f, 0.9f, 1f) : Color.white;
            botonAmplio.colors = colores;
        }

        if (botonReducido != null)
        {
            ColorBlock colores = botonReducido.colors;
            colores.normalColor = perfilReducido ? new Color(0.7f, 0.9f, 1f) : Color.white;
            botonReducido.colors = colores;
        }
    }

    // Iniciar proceso de calibración
    public void IniciarCalibracion()
    {
        if (mote == null && GestorWiimotes.Instance?.wiimote == null)
        {
            // No hay Wiimote, usar calibración con mouse
            if (textoInstrucciones != null)
            {
                textoInstrucciones.text = "No se detectó ningún Wiimote.\n" +
                    "Se usará calibración con mouse/teclado.\n" +
                    "Esto proporcionará valores predeterminados.";
            }

            // Permitir continuar con mouse después de unos segundos
            StartCoroutine(MostrarBotonContinuar());
            return;
        }

        StartCoroutine(ProcesoCalibracion());
    }

    private IEnumerator MostrarBotonContinuar()
    {
        yield return new WaitForSeconds(3f);

        // Establecer valores predeterminados para calibración sin Wiimote
        aceleracionMaximaCaptada = 2.5f; // Valor predeterminado razonable

        if (textoInstrucciones != null)
        {
            textoInstrucciones.text = "Usando valores predeterminados de calibración.\n\n" +
                $"Aceleración configurada: {aceleracionMaximaCaptada:F2}\n\n" +
                "Continuando al siguiente paso...";
        }

        yield return new WaitForSeconds(2f);

        // Avanzar al siguiente paso
        MostrarPanelNombre();
    }

    public void ComenzarCalibracion()
    {
        // Ocultar panel principal y mostrar el de proceso
        if (panelPrincipalCalibracion != null)
            panelPrincipalCalibracion.SetActive(false);

        if (panelProcesoCalibracion != null)
            panelProcesoCalibracion.SetActive(true);

        // Iniciar el proceso de calibración
        IniciarCalibracion();
    }

    private IEnumerator ProcesoCalibracion()
    {
        Debug.Log("Iniciando proceso de calibración");

        if (textoEstadoCalibracion != null)
        {
            if (mote != null)
            {
                textoEstadoCalibracion.text = "Prepárate para la calibración...\n\n" +
                    "Cuando estés listo, mantén pulsado el botón B y realiza\n" +
                    "un movimiento de lanzamiento. Suelta el botón al terminar.";
            }
            else
            {
                textoEstadoCalibracion.text = "Calibración con mouse/teclado...\n\n" +
                    "Mantén pulsado el botón izquierdo del mouse y muévelo\n" +
                    "para simular un movimiento de lanzamiento. Suelta para terminar.";
            }
        }

        if (indicadorCargaCircular != null)
        {
            indicadorCargaCircular.gameObject.SetActive(true);
        }

        // Esperar a que el usuario pulse el botón (B del Wiimote o click izquierdo)
        bool botonPulsado = false;

        while (!botonPulsado)
        {
            if (mote != null)
            {
                mote.ReadWiimoteData();
                if (mote.Button.b)
                {
                    botonPulsado = true;
                    calibracionEnCurso = true;
                    aceleracionMaximaCaptada = 0f;

                    if (textoEstadoCalibracion != null)
                    {
                        textoEstadoCalibracion.text = "Calibración en proceso...\n" +
                            "Mantén el botón hasta completar el movimiento";
                    }
                }
            }
            else
            {
                // Mouse fallback
                if (Input.GetMouseButtonDown(0))
                {
                    botonPulsado = true;
                    calibracionEnCurso = true;
                    aceleracionMaximaCaptada = 0f;

                    if (textoEstadoCalibracion != null)
                    {
                        textoEstadoCalibracion.text = "Calibración en proceso...\n" +
                            "Mantén el botón del mouse hasta completar el movimiento";
                    }
                }
            }

            yield return null;
        }

        // Bucle de calibración
        if (mote == null)
        {
            // Mouse calibration
            float calibrationTime = 0f;
            float maxCalibrationTime = 5f; // Máximo 5 segundos de calibración
            
            while (calibracionEnCurso && calibrationTime < maxCalibrationTime)
            {
                calibrationTime += Time.deltaTime;
                
                if (Input.GetMouseButtonUp(0))
                {
                    calibracionEnCurso = false;
                    break;
                }

                // Simulate acceleration based on mouse movement
                Vector3 mouseMovement = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
                float mouseMagnitude = mouseMovement.magnitude;

                if (mouseMagnitude > aceleracionMaximaCaptada)
                {
                    aceleracionMaximaCaptada = mouseMagnitude;
                }

                yield return null;
            }

            // Asegurar un valor mínimo razonable
            aceleracionMaximaCaptada = Mathf.Max(aceleracionMaximaCaptada * 5f, 1.5f);

            if (textoEstadoCalibracion != null)
            {
                textoEstadoCalibracion.text = $"¡Calibración completada!\n\n" +
                    $"Aceleración máxima: {aceleracionMaximaCaptada:F2}\n\n" +
                    $"Continuando al siguiente paso...";
            }
        }
        else
        {
            // Wiimote calibration (existing code)
            while (calibracionEnCurso)
            {
                mote.ReadWiimoteData();

                if (!mote.Button.b)
                {
                    calibracionEnCurso = false;

                    if (textoEstadoCalibracion != null)
                    {
                        textoEstadoCalibracion.text = $"¡Calibración completada!\n\n" +
                            $"Aceleración máxima: {aceleracionMaximaCaptada:F2}\n\n" +
                            $"Continuando al siguiente paso...";
                    }
                    break;
                }

                float[] datos = mote.Accel.GetCalibratedAccelData();
                if (datos != null && datos.Length == 3)
                {
                    float magnitud = Mathf.Sqrt(
                        datos[0] * datos[0] +
                        datos[1] * datos[1] +
                        datos[2] * datos[2]);

                    if (magnitud > aceleracionMaximaCaptada)
                    {
                        aceleracionMaximaCaptada = magnitud;
                    }
                }

                yield return null;
            }
        }

        if (indicadorCargaCircular != null)
        {
            indicadorCargaCircular.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2f);

        MostrarPanelNombre();
    }

    public void GenerarNombreAleatorio()
    {
        if (campoNombre != null)
        {
            campoNombre.text = GestorPerfiles.Instancia.GenerarNombreAleatorio();
        }
    }

    // Guardar perfil
    public void GuardarPerfil()
    {
        if (campoNombre == null)
        {
            Debug.LogError("Campo de nombre no está asignado");
            return;
        }

        string nombre = campoNombre.text?.Trim();

        // Verificar que hay un nombre
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogWarning("Debes introducir un nombre válido para el perfil");
            
            // Mostrar mensaje en UI si es posible
            if (textoEstadoCalibracion != null)
            {
                textoEstadoCalibracion.text = "Por favor, introduce un nombre válido para el perfil.";
            }
            return;
        }

        // Verificar que el gestor de perfiles existe
        if (GestorPerfiles.Instancia == null)
        {
            Debug.LogError("GestorPerfiles no está disponible");
            return;
        }

        try
        {
            if (modoEdicion && perfilEdicion != null)
            {
                // Actualizar perfil existente
                GestorPerfiles.Instancia.ActualizarPerfil(
                    perfilEdicion,
                    nombre,
                    esZurdo,
                    perfilReducido,
                    aceleracionMaximaCaptada
                );
                Debug.Log($"Perfil '{nombre}' actualizado correctamente");
            }
            else
            {
                // Crear nuevo perfil
                GestorPerfiles.Instancia.CrearPerfil(
                    nombre,
                    esZurdo,
                    perfilReducido,
                    aceleracionMaximaCaptada
                );
                Debug.Log($"Perfil '{nombre}' creado correctamente");
            }

            // Volver a la configuración
            VolverAConfiguracion();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al guardar el perfil: {ex.Message}");
            
            if (textoEstadoCalibracion != null)
            {
                textoEstadoCalibracion.text = "Error al guardar el perfil. Inténtalo de nuevo.";
            }
        }
    }

    // Volver al menú de configuración
    public void VolverAConfiguracion()
    {
        SceneManager.LoadScene(escenaConfiguracion);
    }

    public void VolverAlMenuPerfiles()
    {
        // Volver a la escena de edición de perfiles
        SceneManager.LoadScene("EdicionPerfiles");
    }
}