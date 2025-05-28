using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using WiimoteApi;

public class EdicionPerfiles : MonoBehaviour
{
    [Header("Navegación")]
    public string escenaConfiguracion = "MenuConfiguracion";
    public string escenaCreacionPerfil = "CrearPerfil";
    
    [Header("UI de Lista")]
    public GameObject contenedorPerfiles;
    public GameObject prefabItemPerfil;
    
    [Header("UI de Detalle")]
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoLateralidad;
    public TextMeshProUGUI textoTipoPerfil;
    public TextMeshProUGUI textoAceleracion;
    public TextMeshProUGUI textoFactorAyuda;
    public TextMeshProUGUI textoFechaCreacion;
    public TextMeshProUGUI textoIndiceActual; // Para mostrar "Perfil X de Y"
    
    [Header("Botones de acción")]
    public Button botonEditar;
    public Button botonEliminar;
    public Button botonCrearNuevo;
    public Button botonVolver;
    
    [Header("UI")]
    public Canvas canvas;
    
    [Header("Navegación Wiimote")]
    public float tiempoEsperaBotones = 0.3f;
    
    // Control de navegación
    private int indicePerfilSeleccionado = 0;
    private float ultimoInputTiempo = 0f;
    private Wiimote mote;
    
    private void Start()
    {
        // Verificar que tenemos acceso al gestor de perfiles
        if (GestorPerfiles.Instancia == null)
        {
            Debug.LogError("No se encontró el GestorPerfiles. Asegúrate de inicializarlo primero.");
            return;
        }
        
        // Inicializar Wiimote
        InicializarWiimote();
        
        // Inicializar GestorUI si existe
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }
        
        // Configurar eventos de botones
        ConfigurarEventosBotones();
        
        // Cargar lista de perfiles
        CargarListaPerfiles();
        
        // Seleccionar el perfil actual del gestor
        indicePerfilSeleccionado = GestorPerfiles.Instancia.indicePerfilActual;
        ActualizarVisualizacionPerfil();
        ActualizarEstadoBotones();
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
            // Configurar LEDs para indicar perfil actual
            ActualizarLEDsWiimote();
        }
        else
        {
            Debug.LogWarning("No se encontró Wiimote para EdicionPerfiles");
        }
    }
    
    void ConfigurarEventosBotones()
    {
        if (botonEditar != null)
            botonEditar.onClick.AddListener(EditarPerfilSeleccionado);
            
        if (botonEliminar != null)
            botonEliminar.onClick.AddListener(EliminarPerfilSeleccionado);
        
        if (botonCrearNuevo != null)
            botonCrearNuevo.onClick.AddListener(CrearNuevoPerfil);
            
        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAConfiguracion);
    }
    
    void Update()
    {
        // Control con Wiimote
        if (mote != null)
        {
            mote.ReadWiimoteData();
            
            // Control de navegación con debounce
            if (Time.time - ultimoInputTiempo > tiempoEsperaBotones)
            {
                if (mote.Button.one) // Perfil anterior
                {
                    CambiarPerfilSeleccionado(-1);
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.two) // Perfil siguiente
                {
                    CambiarPerfilSeleccionado(1);
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.a) // Editar perfil actual
                {
                    EditarPerfilSeleccionado();
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.b) // Volver al menú
                {
                    VolverAConfiguracion();
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.minus) // Eliminar perfil
                {
                    EliminarPerfilSeleccionado();
                    ultimoInputTiempo = Time.time;
                }
                else if (mote.Button.plus) // Crear nuevo perfil
                {
                    CrearNuevoPerfil();
                    ultimoInputTiempo = Time.time;
                }
            }
            
            // Control de menú con D-pad (si existe GestorUI)
            if (GestorUI.Instance != null)
            {
                if (mote.Button.d_up)
                    GestorUI.Instance.MoverMenu(-1);
                else if (mote.Button.d_down)
                    GestorUI.Instance.MoverMenu(1);
                    
                if (!mote.Button.d_up && !mote.Button.d_down)
                    GestorUI.Instance.LiberarBoton();
            }
        }
    }
    
    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);
        // GestorUI ya maneja el onClick automáticamente
    }
    
    // Cambiar perfil seleccionado con navegación circular
    private void CambiarPerfilSeleccionado(int direccion)
    {
        int cantidadPerfiles = GestorPerfiles.Instancia.CantidadPerfiles();
        if (cantidadPerfiles == 0) return;
        
        // Calcular nuevo índice con wrap-around
        indicePerfilSeleccionado += direccion;
        
        if (indicePerfilSeleccionado < 0)
            indicePerfilSeleccionado = cantidadPerfiles - 1;
        else if (indicePerfilSeleccionado >= cantidadPerfiles)
            indicePerfilSeleccionado = 0;
        
        // Actualizar perfil actual en el gestor
        GestorPerfiles.Instancia.CambiarPerfilActual(indicePerfilSeleccionado);
        
        // Actualizar visualización
        ActualizarVisualizacionPerfil();
        ActualizarVisualLista();
        ActualizarLEDsWiimote();
        
        Debug.Log($"Cambiado a perfil {indicePerfilSeleccionado + 1} de {cantidadPerfiles}");
    }
    
    // Actualizar la visualización del perfil actual
    private void ActualizarVisualizacionPerfil()
    {
        DatosPerfilUsuario perfil = GestorPerfiles.Instancia.ObtenerPerfilActual();
        if (perfil == null) return;
        
        // Actualizar textos de detalle
        if (textoNombre != null)
            textoNombre.text = perfil.nombreUsuario;
            
        if (textoLateralidad != null)
            textoLateralidad.text = perfil.esZurdo ? "Zurdo" : "Diestro";
            
        if (textoTipoPerfil != null)
            textoTipoPerfil.text = perfil.perfilReducido ? "Reducido" : "Amplio";
            
        if (textoAceleracion != null)
            textoAceleracion.text = perfil.aceleracionMaximaCalibrada.ToString("F2") + " m/s²";
            
        if (textoFactorAyuda != null)
            textoFactorAyuda.text = perfil.factorAyuda.ToString("F2") + "x";
            
        if (textoFechaCreacion != null)
        {
            System.DateTime fecha = new System.DateTime(perfil.fechaCreacion);
            textoFechaCreacion.text = fecha.ToString("dd/MM/yyyy HH:mm");
        }
        
        if (textoIndiceActual != null)
        {
            int total = GestorPerfiles.Instancia.CantidadPerfiles();
            textoIndiceActual.text = $"Perfil {indicePerfilSeleccionado + 1} de {total}";
        }
    }
    
    // Cargar la lista de perfiles (solo nombres)
    private void CargarListaPerfiles()
    {
        if (contenedorPerfiles == null) return;
        
        // Limpiar contenedor
        foreach (Transform child in contenedorPerfiles.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Crear elementos de la lista
        List<DatosPerfilUsuario> perfiles = GestorPerfiles.Instancia.ObtenerPerfiles();
        
        for (int i = 0; i < perfiles.Count; i++)
        {
            DatosPerfilUsuario perfil = perfiles[i];
            GameObject item = Instantiate(prefabItemPerfil, contenedorPerfiles.transform);
            
            // Configurar texto del nombre
            TextMeshProUGUI texto = item.GetComponentInChildren<TextMeshProUGUI>();
            if (texto != null)
            {
                texto.text = perfil.nombreUsuario;
            }
            
            // Configurar botón para seleccionar
            Button boton = item.GetComponent<Button>();
            if (boton != null)
            {
                int indiceCapturado = i; // Captura para evitar problemas de closure
                boton.onClick.AddListener(() => SeleccionarPerfilPorIndice(indiceCapturado));
            }
        }
        
        ActualizarVisualLista();
    }
    
    // Seleccionar perfil por índice
    private void SeleccionarPerfilPorIndice(int indice)
    {
        if (indice < 0 || indice >= GestorPerfiles.Instancia.CantidadPerfiles())
            return;
            
        indicePerfilSeleccionado = indice;
        GestorPerfiles.Instancia.CambiarPerfilActual(indice);
        
        ActualizarVisualizacionPerfil();
        ActualizarVisualLista();
        ActualizarLEDsWiimote();
        ActualizarEstadoBotones();
    }
    
    // Actualizar colores de la lista sin recrearla
    private void ActualizarVisualLista()
    {
        if (contenedorPerfiles == null) return;
        
        int index = 0;
        foreach (Transform child in contenedorPerfiles.transform)
        {
            Button boton = child.GetComponent<Button>();
            if (boton != null)
            {
                ColorBlock colores = boton.colors;
                
                if (index == indicePerfilSeleccionado)
                    colores.normalColor = new Color(0.7f, 0.9f, 1f); // Seleccionado (azul claro)
                else
                    colores.normalColor = Color.white; // Normal
                    
                boton.colors = colores;
            }
            index++;
        }
    }
    
    // Actualizar LEDs del Wiimote para indicar perfil actual
    private void ActualizarLEDsWiimote()
    {
        if (mote == null) return;
        
        // Usar los 4 LEDs para mostrar el índice del perfil (binario)
        int indice = indicePerfilSeleccionado % 16; // Máximo 16 perfiles representables
        
        mote.SendPlayerLED(
            (indice & 1) != 0,      // LED 1
            (indice & 2) != 0,      // LED 2
            (indice & 4) != 0,      // LED 3
            (indice & 8) != 0       // LED 4
        );
    }
    
    private void ActualizarEstadoBotones()
    {
        bool hayPerfil = GestorPerfiles.Instancia.CantidadPerfiles() > 0;
        bool permiteEliminar = hayPerfil && GestorPerfiles.Instancia.CantidadPerfiles() > 1;
        
        if (botonEditar != null)
            botonEditar.interactable = hayPerfil;
            
        if (botonEliminar != null)
            botonEliminar.interactable = permiteEliminar;
    }
    
    // Crear nuevo perfil
    public void CrearNuevoPerfil()
    {
        // Resetear indicadores de edición
        PlayerPrefs.SetInt("ModoEdicion", 0);
        PlayerPrefs.SetInt("IndicePerfilEditar", -1);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(escenaCreacionPerfil);
    }
    
    // Editar el perfil seleccionado
    public void EditarPerfilSeleccionado()
    {
        if (GestorPerfiles.Instancia.CantidadPerfiles() == 0) return;
        
        // Configurar modo edición
        PlayerPrefs.SetInt("ModoEdicion", 1);
        PlayerPrefs.SetInt("IndicePerfilEditar", indicePerfilSeleccionado);
        PlayerPrefs.Save();
        
        Debug.Log($"Editando perfil en índice: {indicePerfilSeleccionado}");
        SceneManager.LoadScene(escenaCreacionPerfil);
    }
    
    // Eliminar el perfil seleccionado
    public void EliminarPerfilSeleccionado()
    {
        if (GestorPerfiles.Instancia.CantidadPerfiles() <= 1)
        {
            Debug.LogWarning("No se puede eliminar el único perfil existente");
            return;
        }
        
        DatosPerfilUsuario perfil = GestorPerfiles.Instancia.ObtenerPerfilActual();
        if (perfil == null) return;
        
        Debug.Log($"Eliminando perfil: {perfil.nombreUsuario}");
        
        // Eliminar perfil
        GestorPerfiles.Instancia.EliminarPerfil(indicePerfilSeleccionado);
        
        // Ajustar índice si es necesario
        if (indicePerfilSeleccionado >= GestorPerfiles.Instancia.CantidadPerfiles())
        {
            indicePerfilSeleccionado = GestorPerfiles.Instancia.CantidadPerfiles() - 1;
        }
        
        // El gestor ya actualiza su índice interno, sincronizar
        indicePerfilSeleccionado = GestorPerfiles.Instancia.indicePerfilActual;
        
        // Actualizar visualización
        CargarListaPerfiles();
        ActualizarVisualizacionPerfil();
        ActualizarEstadoBotones();
        ActualizarLEDsWiimote();
    }
    
    // Volver al menú de configuración
    public void VolverAConfiguracion()
    {
        SceneManager.LoadScene(escenaConfiguracion);
    }
    
    void OnDestroy()
    {
        // Limpiar eventos
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
        }
        
        // No necesitamos limpiar el Wiimote si lo gestiona GestorWiimotes
        if (mote != null && GestorWiimotes.Instance?.wiimote == null)
        {
            WiimoteManager.Cleanup(mote);
        }
    }
}