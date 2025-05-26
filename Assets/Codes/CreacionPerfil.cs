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

        // Control de navegación con Wiimote
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
            gestorUI.Inicializar(canvas);
            gestorUI.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
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
        ActualizarBotonesLateralidad();
    }

    public void MostrarPanelTipoPerfil()
    {
        MostrarSoloPanelActual(panelTipoPerfil);
        ActualizarBotonesTipoPerfil();
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
        if (mote == null)
        {
            // No hay Wiimote, mostrar error y permitir continuar
            if (textoInstrucciones != null)
            {
                textoInstrucciones.text = "¡Error! No se detectó ningún Wiimote.\n" +
                    "Puedes continuar sin calibración, pero el juego no se ajustará correctamente.";
            }

            // Permitir saltar la calibración después de unos segundos
            StartCoroutine(MostrarBotonContinuar());
            return;
        }

        StartCoroutine(ProcesoCalibracion());
    }

    private IEnumerator MostrarBotonContinuar()
    {
        yield return new WaitForSeconds(3f);

        // Añadir un botón para continuar sin calibración
        if (textoInstrucciones != null)
        {
            textoInstrucciones.text += "\n\nPulsa continuar para seguir sin calibración.";
        }

        // Aquí deberías activar un botón para continuar
        // Por simplicidad, avanzamos automáticamente tras una pausa
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

        // Verificar si los objetos están asignados
        if (textoEstadoCalibracion == null)
        {
            Debug.LogError("textoEstadoCalibracion no está asignado en el Inspector");
        }
        else
        {
            Debug.Log("textoEstadoCalibracion encontrado: " + textoEstadoCalibracion.gameObject.name);
            textoEstadoCalibracion.text = "Prepárate para la calibración...\n\n" +
                "Cuando estés listo, mantén pulsado el botón B y realiza\n" +
                "un movimiento de lanzamiento. Suelta el botón al terminar.";
            Debug.Log("Texto actualizado a: " + textoEstadoCalibracion.text);
        }

        if (indicadorCargaCircular == null)
        {
            Debug.LogError("indicadorCargaCircular no está asignado en el Inspector");
        }
        else
        {
            indicadorCargaCircular.gameObject.SetActive(true);
            Debug.Log("Indicador de carga activado");
        }

        // Esperar a que el usuario pulse el botón B
        bool botonPulsado = false;

        while (!botonPulsado)
        {
            if (mote != null) // Verificar que mote exista antes de usarlo
            {
                mote.ReadWiimoteData();

                if (mote.Button.b)
                {
                    botonPulsado = true;

                    // Iniciar captura de movimiento
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
                // Si no hay mote, simular después de un tiempo para pruebas
                yield return new WaitForSeconds(2f);
                botonPulsado = true;
                calibracionEnCurso = true;

                if (textoEstadoCalibracion != null)
                {
                    textoEstadoCalibracion.text = "Calibración simulada en proceso...\n" +
                        "(No se detectó Wiimote)";
                }
            }

            yield return null;
        }

        // Tiempo simulado para la calibración si no hay botón real
        if (mote == null)
        {
            yield return new WaitForSeconds(3f);
            calibracionEnCurso = false;
            aceleracionMaximaCaptada = 1.0f; // Valor simulado

            if (textoEstadoCalibracion != null)
            {
                textoEstadoCalibracion.text = $"¡Calibración simulada completada!\n\n" +
                    $"Aceleración máxima: {aceleracionMaximaCaptada:F2}\n\n" +
                    $"Continuando al siguiente paso...";
            }
        }
        else
        {
            // Bucle mientras está pulsado el botón B
            while (calibracionEnCurso)
            {
                mote.ReadWiimoteData();

                // Verificar si soltó el botón
                if (!mote.Button.b)
                {
                    calibracionEnCurso = false;

                    // Mostrar resultados
                    if (textoEstadoCalibracion != null)
                    {
                        textoEstadoCalibracion.text = $"¡Calibración completada!\n\n" +
                            $"Aceleración máxima: {aceleracionMaximaCaptada:F2}\n\n" +
                            $"Continuando al siguiente paso...";
                    }

                    break;
                }

                // Capturar datos de aceleración
                float[] datos = mote.Accel.GetCalibratedAccelData();
                if (datos != null && datos.Length == 3)
                {
                    float magnitud = Mathf.Sqrt(
                        datos[0] * datos[0] +
                        datos[1] * datos[1] +
                        datos[2] * datos[2]);

                    // Actualizar valor máximo
                    if (magnitud > aceleracionMaximaCaptada)
                    {
                        aceleracionMaximaCaptada = magnitud;
                    }
                }

                yield return null;
            }
        }

        // Desactivar animación de carga circular
        if (indicadorCargaCircular != null)
        {
            indicadorCargaCircular.gameObject.SetActive(false);
        }

        // Esperar un poco antes de continuar
        yield return new WaitForSeconds(2f);

        Debug.Log("Proceso de calibración completado, mostrando panel de nombre");

        // Avanzar al siguiente panel
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
        string nombre = campoNombre.text;

        // Verificar que hay un nombre
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogWarning("Debes introducir un nombre para el perfil");
            return;
        }

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
        }

        // Volver a la configuración
        VolverAConfiguracion();
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