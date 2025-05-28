using UnityEngine;

public class InicializadorAccesibilidad : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Prefab opcional que contenga GestorAccesibilidad")]
    public GameObject prefabGestorAccesibilidad;

    [Header("Debug")]
    public bool mostrarLogs = true;

    private void Awake()
    {
        Log("🚀 Iniciando configuración de accesibilidad URP...");

        // Si no existe tu GestorAccesibilidad, lo creamos
        if (GestorAccesibilidad.Instancia == null)
        {
            if (prefabGestorAccesibilidad != null)
            {
                Log("🔨 Instanciando GestorAccesibilidad desde prefab");
                Instantiate(prefabGestorAccesibilidad);
            }
            else
            {
                Log("🔨 Creando GestorAccesibilidad en vacío");
                var go = new GameObject("GestorAccesibilidad");
                go.AddComponent<GestorAccesibilidad>();
            }
        }
        else
        {
            Log("✅ GestorAccesibilidad ya existe");
        }
    }

    private void Start()
    {
        ConfigurarCamaraParaVolumen();
        Invoke(nameof(VerificarSistema), 0.5f);
    }

    private void ConfigurarCamaraParaVolumen()
    {
        // Busca la cámara principal
        Camera cam = Camera.main;
        if (cam == null)
            cam = FindObjectOfType<Camera>();

        if (cam == null)
        {
            LogWarning("⚠️ No se ha encontrado ninguna cámara en la escena.");
            return;
        }

        // Layer donde el Gestor crea el Volume
        int ppLayer = LayerMask.NameToLayer("PostProcessing");
        if (ppLayer == -1)
        {
            LogWarning("⚠️ El layer 'PostProcessing' no existe. Créalo en Project Settings > Tags and Layers.");
            return;
        }

        // Incluimos ese layer en la culling mask de la cámara
        cam.cullingMask |= (1 << ppLayer);
        Log($"✅ Cámara configurada para detectar layer 'PostProcessing' ({ppLayer}).");
    }

    private void VerificarSistema()
    {
        var gestor = GestorAccesibilidad.Instancia;
        if (gestor == null)
        {
            LogWarning("❌ GestorAccesibilidad no se ha instanciado.");
            return;
        }

        if (gestor.SistemaListo())
        {
            Log("✅ Sistema de accesibilidad URP listo.");
            int filtro = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
            Log($"🎨 Filtro actual: {(TipoDaltonismo)filtro}");
        }
        else
        {
            LogWarning("⚠️ Sistema no listo. Forzando inicialización...");
            gestor.ForzarInicializacion();
        }
    }



    private void Log(string msg)
    {
        if (mostrarLogs) Debug.Log($"[InicializadorAccesibilidad] {msg}");
    }

    private void LogWarning(string msg)
    {
        if (mostrarLogs) Debug.LogWarning($"[InicializadorAccesibilidad] {msg}");
    }
}
