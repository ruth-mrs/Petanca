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
    
    [Header("Botones de acción")]
    public Button botonEditar;
    public Button botonEliminar;
    public Button botonCrearNuevo;
    public Button botonVolver;
    
    [Header("UI")]
    public Canvas canvas;
    
    // Perfil seleccionado actualmente
    private PerfilUsuario perfilSeleccionado;
    private int indicePerfilSeleccionado = 0;
    
    // GestorUI.Instance
    
    private void Start()
    {
        // Verificar que tenemos acceso al gestor de perfiles
        if (GestorPerfiles.Instancia == null)
        {
            Debug.LogError("No se encontró el GestorPerfiles. Asegúrate de inicializarlo primero.");
            return;
        }
        
        GestorUI.Instance.Inicializar(canvas);
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;

        GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        
        // Cargar lista de perfiles
        CargarListaPerfiles();
        
        // Configurar eventos de botones
        if (botonEditar != null)
            botonEditar.onClick.AddListener(EditarPerfilSeleccionado);
            
        if (botonEliminar != null)
            botonEliminar.onClick.AddListener(EliminarPerfilSeleccionado);
        
        if (botonCrearNuevo != null)
            botonCrearNuevo.onClick.AddListener(CrearNuevoPerfil);
            
        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAConfiguracion);
            
        // Inicialmente, deshabilitar botones hasta que se seleccione un perfil
        ActualizarEstadoBotones();
        
        // Seleccionar el primer perfil por defecto
        if (GestorPerfiles.Instancia.perfilesUsuarios.Count > 0)
        {
            indicePerfilSeleccionado = 0;
            SeleccionarPerfilPorIndice(indicePerfilSeleccionado);
        }
    }
    
    void Update()
    {
        // Control de navegación con Wiimote
        Wiimote wiimote = GestorWiimotes.Instance?.wiimote;

        if (wiimote != null)
        {
            int ret;
            do
            {
                ret = wiimote.ReadWiimoteData();
            } while (ret > 0);

            // Control de navegación con D-pad para botones
            if (wiimote.Button.d_up)
            {
                GestorUI.Instance.MoverMenu(-1);
            }
            else if (wiimote.Button.d_down)
            {
                GestorUI.Instance.MoverMenu(1);
            }

            // Control de scroll de perfiles con botones 1 y 2
            if (wiimote.Button.one)
            {
                // Botón 1: Perfil anterior
                CambiarPerfilSeleccionado(-1);
            }
            else if (wiimote.Button.two)
            {
                // Botón 2: Perfil siguiente
                CambiarPerfilSeleccionado(1);
            }

            // Liberar estado de botones
            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                GestorUI.Instance.LiberarBoton();
            }

            // Seleccionar con botón A
            if (wiimote.Button.a)
            {
                GestorUI.Instance.SeleccionarBoton();
            }
        }
 }
        
    
    
    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);
        if (botonSeleccionado == 4){
            VolverAConfiguracion();
        }
    }
    
    // Nuevo método para cambiar perfil seleccionado con botones 1 y 2
    private void CambiarPerfilSeleccionado(int direccion)
    {
        if (GestorPerfiles.Instancia.perfilesUsuarios.Count == 0) return;
        
        // Calcular nuevo índice
        indicePerfilSeleccionado += direccion;
        
        // Hacer wrap-around (circular)
        if (indicePerfilSeleccionado < 0)
            indicePerfilSeleccionado = GestorPerfiles.Instancia.perfilesUsuarios.Count - 1;
        else if (indicePerfilSeleccionado >= GestorPerfiles.Instancia.perfilesUsuarios.Count)
            indicePerfilSeleccionado = 0;
        
        // Seleccionar el nuevo perfil
        SeleccionarPerfilPorIndice(indicePerfilSeleccionado);
    }
    
    // Método para seleccionar perfil por índice
    private void SeleccionarPerfilPorIndice(int indice)
    {
        if (indice >= 0 && indice < GestorPerfiles.Instancia.perfilesUsuarios.Count)
        {
            PerfilUsuario perfil = GestorPerfiles.Instancia.perfilesUsuarios[indice];
            SeleccionarPerfil(perfil);
        }
    }
    
    // Cargar la lista de perfiles disponibles (SOLO NOMBRES)
    private void CargarListaPerfiles()
    {
        // Limpiar contenedor
        if (contenedorPerfiles != null)
        {
            foreach (Transform child in contenedorPerfiles.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Crear un elemento por cada perfil (SOLO NOMBRE)
            foreach (PerfilUsuario perfil in GestorPerfiles.Instancia.perfilesUsuarios)
            {
                GameObject item = Instantiate(prefabItemPerfil, contenedorPerfiles.transform);
                
                // Configurar SOLO el nombre
                TextMeshProUGUI texto = item.GetComponentInChildren<TextMeshProUGUI>();
                if (texto != null)
                {
                    texto.text = perfil.nombreUsuario;
                }
                
                // Configurar botón principal para seleccionar
                Button boton = item.GetComponent<Button>();
                if (boton != null)
                {
                    PerfilUsuario perfilActual = perfil; // Captura para evitar problemas de closure
                    boton.onClick.AddListener(() => SeleccionarPerfil(perfilActual));
                    
                    // Marcar perfil activo visualmente
                    if (perfil == GestorPerfiles.Instancia.perfilActual)
                    {
                        ColorBlock colores = boton.colors;
                        colores.normalColor = new Color(0.7f, 0.9f, 1f); // Azul claro
                        boton.colors = colores;
                    }
                }
            }
        }
        
        // Seleccionar el perfil actual por defecto
        if (GestorPerfiles.Instancia.perfilActual != null)
        {
            int indice = GestorPerfiles.Instancia.perfilesUsuarios.IndexOf(GestorPerfiles.Instancia.perfilActual);
            if (indice >= 0)
            {
                indicePerfilSeleccionado = indice;
                SeleccionarPerfil(GestorPerfiles.Instancia.perfilActual);
            }
        }
    }
    
    // Crear nuevo perfil
    public void CrearNuevoPerfil()
    {
        // Resetear indicadores de edición
        PlayerPrefs.SetInt("ModoEdicion", 0);
        PlayerPrefs.SetInt("IndicePerfilEditar", -1);
        PlayerPrefs.Save();
        
        // Ir a la escena de creación
        SceneManager.LoadScene(escenaCreacionPerfil);
    }
    
    public void SeleccionarPerfil(PerfilUsuario perfil)
    {
        perfilSeleccionado = perfil;
        
        // Actualizar índice seleccionado
        indicePerfilSeleccionado = GestorPerfiles.Instancia.perfilesUsuarios.IndexOf(perfil);
        
        // Actualizar SOLO el panel de detalles (derecha)
        if (textoNombre != null)
            textoNombre.text = perfil.nombreUsuario;
            
        if (textoLateralidad != null)
            textoLateralidad.text = perfil.esZurdo ? "Zurdo" : "Diestro";
            
        if (textoTipoPerfil != null)
            textoTipoPerfil.text = perfil.perfilReducido ? "Reducido" : "Amplio";
            
        if (textoAceleracion != null)
            textoAceleracion.text = perfil.aceleracionMaximaCalibrada.ToString("F2");
            
        if (textoFactorAyuda != null)
            textoFactorAyuda.text = perfil.factorAyuda.ToString("F2");
        
        // Habilitar botones del panel de detalles
        ActualizarEstadoBotones();
        
        // Actualizar perfil actual en el gestor
        GestorPerfiles.Instancia.perfilActual = perfil;
        GestorPerfiles.Instancia.GuardarPerfiles();
        
        // Actualizar visual de la lista (solo colores, no recrear)
        ActualizarVisualLista();
        
        Debug.Log($"Perfil seleccionado: {perfil.nombreUsuario} (Índice: {indicePerfilSeleccionado})");
    }
    
    // Nuevo método para actualizar solo los colores sin recrear toda la lista
    private void ActualizarVisualLista()
    {
        if (contenedorPerfiles == null) return;
        
        int index = 0;
        foreach (Transform child in contenedorPerfiles.transform)
        {
            Button boton = child.GetComponent<Button>();
            if (boton != null && index < GestorPerfiles.Instancia.perfilesUsuarios.Count)
            {
                PerfilUsuario perfil = GestorPerfiles.Instancia.perfilesUsuarios[index];
                ColorBlock colores = boton.colors;
                
                if (perfil == perfilSeleccionado)
                    colores.normalColor = new Color(0.7f, 0.9f, 1f); // Seleccionado (azul claro)
                else
                    colores.normalColor = Color.white; // Normal
                    
                boton.colors = colores;
            }
            index++;
        }
    }
    
    private void ActualizarEstadoBotones()
    {
        bool perfilSeleccionado = this.perfilSeleccionado != null;
        bool permiteEliminar = perfilSeleccionado && GestorPerfiles.Instancia.perfilesUsuarios.Count > 1;
        
        if (botonEditar != null)
            botonEditar.interactable = perfilSeleccionado;
            
        if (botonEliminar != null)
            botonEliminar.interactable = permiteEliminar;
    }
    
    // Editar el perfil seleccionado
    public void EditarPerfilSeleccionado()
    {
        if (perfilSeleccionado == null)
        return;
    int indice = GestorPerfiles.Instancia.perfilesUsuarios.IndexOf(perfilSeleccionado);
    PlayerPrefs.SetInt("IndicePerfilEditar", indice);
    PlayerPrefs.SetInt("ModoEdicion", 1);
    PlayerPrefs.Save();
    SceneManager.LoadScene(escenaCreacionPerfil);
    }
    
    // Eliminar el perfil seleccionado
    public void EliminarPerfilSeleccionado()
    {
        if (perfilSeleccionado == null)
            return;
            
        // Verificar que no es el único perfil
        if (GestorPerfiles.Instancia.perfilesUsuarios.Count <= 1)
        {
            Debug.LogWarning("No se puede eliminar el único perfil existente");
            return;
        }
        
        // Confirmar eliminación (aquí podrías mostrar un diálogo)
        GestorPerfiles.Instancia.EliminarPerfil(perfilSeleccionado);
        
        // Ajustar índice si es necesario
        if (indicePerfilSeleccionado >= GestorPerfiles.Instancia.perfilesUsuarios.Count)
        {
            indicePerfilSeleccionado = GestorPerfiles.Instancia.perfilesUsuarios.Count - 1;
        }
        
        // Actualizar lista
        CargarListaPerfiles();
        
        // Seleccionar otro perfil
        if (GestorPerfiles.Instancia.perfilesUsuarios.Count > 0)
        {
            SeleccionarPerfilPorIndice(indicePerfilSeleccionado);
        }
    }
    
    // Volver al menú de configuración
    public void VolverAConfiguracion()
    {
        SceneManager.LoadScene(escenaConfiguracion);
    }

        private void OnDestroy()
{
    if (GestorUI.Instance != null)
    {
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
    }
}
}