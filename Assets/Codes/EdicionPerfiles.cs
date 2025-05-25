using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

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
    
    // Botones de acción
    public Button botonEditar;
    public Button botonEliminar;
    public Button botonCrearNuevo;
    
    // Perfil seleccionado actualmente
    private PerfilUsuario perfilSeleccionado;
    
    private void Start()
    {
        // Verificar que tenemos acceso al gestor de perfiles
        if (GestorPerfiles.Instancia == null)
        {
            Debug.LogError("No se encontró el GestorPerfiles. Asegúrate de inicializarlo primero.");
            return;
        }
        
        // Cargar lista de perfiles
        CargarListaPerfiles();
        
        // Configurar eventos de botones
        if (botonEditar != null)
            botonEditar.onClick.AddListener(EditarPerfilSeleccionado);
            
        if (botonEliminar != null)
            botonEliminar.onClick.AddListener(EliminarPerfilSeleccionado);
        
        if (botonCrearNuevo != null)
            botonCrearNuevo.onClick.AddListener(CrearNuevoPerfil);
            
        // Inicialmente, deshabilitar botones hasta que se seleccione un perfil
        ActualizarEstadoBotones();
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
            SeleccionarPerfil(GestorPerfiles.Instancia.perfilActual);
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
            
        // Guardar índice del perfil a editar
        int indice = GestorPerfiles.Instancia.perfilesUsuarios.IndexOf(perfilSeleccionado);
        PlayerPrefs.SetInt("IndicePerfilEditar", indice);
        PlayerPrefs.SetInt("ModoEdicion", 1);
        PlayerPrefs.Save();
        
        // Ir a la escena de creación/edición
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
        
        // Actualizar lista
        CargarListaPerfiles();
        
        // Seleccionar otro perfil
        if (GestorPerfiles.Instancia.perfilActual != null)
        {
            SeleccionarPerfil(GestorPerfiles.Instancia.perfilActual);
        }
    }
    
    // Volver al menú de configuración
    public void VolverAConfiguracion()
    {
        SceneManager.LoadScene(escenaConfiguracion);
    }
}