using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class InicializadorAccesibilidad : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabGestorAccesibilidad;
    
    [Header("Debug")]
    public bool mostrarLogs = true;
    
    private void Awake()
    {
        Log("🚀 Iniciando configuración de accesibilidad...");
        
        // Verificar si ya existe el gestor
        if (GestorAccesibilidad.Instancia == null)
        {
            CrearGestorAccesibilidad();
        }
        else
        {
            Log("✅ GestorAccesibilidad ya existe");
        }
    }
    
    private void Start()
    {
        // Configurar cámara después de que todo esté inicializado
        ConfigurarCamaraConLayer();
        
        // Verificar que todo funcione correctamente
        Invoke(nameof(VerificarSistema), 0.5f);
    }
    
    private void CrearGestorAccesibilidad()
    {
        if (prefabGestorAccesibilidad != null)
        {
            Log("🔨 Creando GestorAccesibilidad desde prefab");
            Instantiate(prefabGestorAccesibilidad);
        }
        else
        {
            Log("🔨 Creando GestorAccesibilidad básico");
            GameObject gestorObj = new GameObject("GestorAccesibilidad");
            gestorObj.AddComponent<GestorAccesibilidad>();
        }
    }
    
    private void ConfigurarCamaraConLayer()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            PostProcessLayer postLayer = mainCamera.GetComponent<PostProcessLayer>();
            if (postLayer == null)
            {
                postLayer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                Log("✅ PostProcessLayer añadido a la cámara");
            }
            
            // Configurar layer de volumen
            int postProcessLayer = LayerMask.NameToLayer("PostProcess");
            if (postProcessLayer != -1)
            {
                postLayer.volumeLayer = 1 << postProcessLayer;
                Log("✅ Usando layer 'PostProcess'");
            }
            else
            {
                postLayer.volumeLayer = -1; // Everything
                Log("✅ Usando layer 'Everything' (fallback)");
            }
            
            postLayer.volumeTrigger = mainCamera.transform;
            
            // Configurar antialias si no está configurado
            if (postLayer.antialiasingMode == PostProcessLayer.Antialiasing.None)
            {
                postLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
            }
            
            Log("✅ Cámara configurada completamente");
        }
        else
        {
            LogWarning("⚠️ No se encontró cámara principal");
        }
    }
    
    private void VerificarSistema()
    {
        if (GestorAccesibilidad.Instancia != null)
        {
            if (GestorAccesibilidad.Instancia.SistemaListo())
            {
                Log("✅ Sistema de accesibilidad funcionando correctamente");
                
                // Mostrar filtro actual
                int filtroActual = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
                Log($"🎨 Filtro actual: {(MenuAccesibilidad.TipoDaltonismo)filtroActual}");
            }
            else
            {
                LogWarning("⚠️ Sistema de accesibilidad no está listo. Forzando inicialización...");
                GestorAccesibilidad.Instancia.ForzarInicializacion();
            }
        }
        else
        {
            LogWarning("❌ GestorAccesibilidad no existe");
        }
    }
    
    private void Log(string mensaje)
    {
        if (mostrarLogs)
        {
            Debug.Log($"[InicializadorAccesibilidad] {mensaje}");
        }
    }
    
    private void LogWarning(string mensaje)
    {
        if (mostrarLogs)
        {
            Debug.LogWarning($"[InicializadorAccesibilidad] {mensaje}");
        }
    }
}