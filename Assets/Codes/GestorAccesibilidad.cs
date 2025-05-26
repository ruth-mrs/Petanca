using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GestorAccesibilidad : MonoBehaviour
{
    public static GestorAccesibilidad Instancia { get; private set; }
    
    [Header("Post-Processing")]
    public PostProcessVolume volumenProcesamiento;
    private ColorGrading colorGrading;
    private bool sistemaInicializado = false;
    
    private void Awake()
    {
        // Singleton persistente entre escenas
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GestorAccesibilidad creado como singleton");
        }
        else
        {
            Debug.Log("⚠️ GestorAccesibilidad ya existe, destruyendo duplicado");
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (Instancia == this)
        {
            InicializarSistema();
        }
    }
    
    private void InicializarSistema()
    {
        if (sistemaInicializado) return;
        
        Debug.Log("🔧 Inicializando sistema de accesibilidad...");
        
        // Buscar o crear volumen de post-procesamiento
        if (volumenProcesamiento == null)
        {
            volumenProcesamiento = FindObjectOfType<PostProcessVolume>();
            Debug.Log(volumenProcesamiento != null ? "📍 PostProcessVolume encontrado" : "❌ PostProcessVolume no encontrado");
        }
        
        if (volumenProcesamiento == null)
        {
            CrearVolumenPostProceso();
        }
        
        // Configurar color grading
        if (ConfigurarColorGrading())
        {
            sistemaInicializado = true;
            // Aplicar filtro guardado después de la inicialización
            CargarYAplicarFiltroGuardado();
            Debug.Log("✅ Sistema de accesibilidad inicializado correctamente");
        }
        else
        {
            Debug.LogError("❌ Error al inicializar el sistema de accesibilidad");
        }
    }
    
    private void CrearVolumenPostProceso()
    {
        Debug.Log("🔨 Creando PostProcessVolume...");
        GameObject volumeObj = new GameObject("Global Post Process Volume");
        volumenProcesamiento = volumeObj.AddComponent<PostProcessVolume>();
        volumenProcesamiento.isGlobal = true;
        volumenProcesamiento.priority = 1;
        DontDestroyOnLoad(volumeObj);
        Debug.Log("✅ PostProcessVolume creado");
    }
    
    private bool ConfigurarColorGrading()
    {
        try
        {
            if (volumenProcesamiento.profile == null)
            {
                volumenProcesamiento.profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                Debug.Log("📋 Perfil de post-procesamiento creado");
            }
            
            if (!volumenProcesamiento.profile.TryGetSettings(out colorGrading))
            {
                colorGrading = volumenProcesamiento.profile.AddSettings<ColorGrading>();
                Debug.Log("🎨 ColorGrading añadido al perfil");
            }
            
            colorGrading.enabled.Override(true);
            Debug.Log("✅ ColorGrading configurado correctamente");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error configurando ColorGrading: {e.Message}");
            return false;
        }
    }
    
    private void CargarYAplicarFiltroGuardado()
    {
        if (!sistemaInicializado)
        {
            Debug.LogWarning("⚠️ Sistema no inicializado, esperando...");
            Invoke(nameof(CargarYAplicarFiltroGuardado), 0.1f);
            return;
        }
        
        int filtroGuardado = PlayerPrefs.GetInt("FiltroAccesibilidad", 0);
        Debug.Log($"💾 Cargando filtro guardado: {(MenuAccesibilidad.TipoDaltonismo)filtroGuardado}");
        AplicarFiltro((MenuAccesibilidad.TipoDaltonismo)filtroGuardado);
    }
    
    public void AplicarFiltro(MenuAccesibilidad.TipoDaltonismo tipo)
    {
        if (!sistemaInicializado)
        {
            Debug.LogWarning("⚠️ Sistema no inicializado. Inicializando...");
            InicializarSistema();
            if (!sistemaInicializado) return;
        }
        
        if (colorGrading == null)
        {
            Debug.LogError("❌ ColorGrading es null. No se puede aplicar filtro.");
            return;
        }
        
        // Guardar la preferencia inmediatamente
        PlayerPrefs.SetInt("FiltroAccesibilidad", (int)tipo);
        PlayerPrefs.Save();
        
        switch (tipo)
        {
            case MenuAccesibilidad.TipoDaltonismo.Normal:
                AplicarFiltroNormal();
                break;
            case MenuAccesibilidad.TipoDaltonismo.Protanopia:
                AplicarFiltroProtanopia();
                break;
            case MenuAccesibilidad.TipoDaltonismo.Deuteronopia:
                AplicarFiltroDeuteronopia();
                break;
            case MenuAccesibilidad.TipoDaltonismo.Tritanopia:
                AplicarFiltroTritanopia();
                break;
            case MenuAccesibilidad.TipoDaltonismo.Protanomalia:
                AplicarFiltroProtanomalia();
                break;
            case MenuAccesibilidad.TipoDaltonismo.Deuteranomalia:
                AplicarFiltroDeuteranomalia();
                break;
            case MenuAccesibilidad.TipoDaltonismo.Tritanomalia:
                AplicarFiltroTritanomalia();
                break;
        }
        
        Debug.Log($"✅ Filtro aplicado y guardado: {tipo}");
    }
    
    // Método público para verificar si el sistema está listo
    public bool SistemaListo()
    {
        return sistemaInicializado && colorGrading != null;
    }
    
    // Método público para forzar inicialización
    public void ForzarInicializacion()
    {
        sistemaInicializado = false;
        InicializarSistema();
    }
    
    private void AplicarFiltroNormal()
    {
        colorGrading.mixerRedOutRedIn.Override(100f);
        colorGrading.mixerRedOutGreenIn.Override(0f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(0f);
        colorGrading.mixerGreenOutGreenIn.Override(100f);
        colorGrading.mixerGreenOutBlueIn.Override(0f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(0f);
        colorGrading.mixerBlueOutBlueIn.Override(100f);
    }
    
    private void AplicarFiltroProtanopia()
    {
        // Ceguera al rojo - transformación de matriz
        colorGrading.mixerRedOutRedIn.Override(56.7f);
        colorGrading.mixerRedOutGreenIn.Override(43.3f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(55.8f);
        colorGrading.mixerGreenOutGreenIn.Override(44.2f);
        colorGrading.mixerGreenOutBlueIn.Override(0f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(24.2f);
        colorGrading.mixerBlueOutBlueIn.Override(75.8f);
    }
    
    private void AplicarFiltroDeuteronopia()
    {
        // Ceguera al verde
        colorGrading.mixerRedOutRedIn.Override(62.5f);
        colorGrading.mixerRedOutGreenIn.Override(37.5f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(70f);
        colorGrading.mixerGreenOutGreenIn.Override(30f);
        colorGrading.mixerGreenOutBlueIn.Override(0f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(30f);
        colorGrading.mixerBlueOutBlueIn.Override(70f);
    }
    
    private void AplicarFiltroTritanopia()
    {
        // Ceguera al azul
        colorGrading.mixerRedOutRedIn.Override(95f);
        colorGrading.mixerRedOutGreenIn.Override(5f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(0f);
        colorGrading.mixerGreenOutGreenIn.Override(43.3f);
        colorGrading.mixerGreenOutBlueIn.Override(56.7f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(47.5f);
        colorGrading.mixerBlueOutBlueIn.Override(52.5f);
    }
    
    private void AplicarFiltroProtanomalia()
    {
        // Deficiencia al rojo (menos severa)
        colorGrading.mixerRedOutRedIn.Override(81.7f);
        colorGrading.mixerRedOutGreenIn.Override(18.3f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(33.3f);
        colorGrading.mixerGreenOutGreenIn.Override(66.7f);
        colorGrading.mixerGreenOutBlueIn.Override(0f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(12.5f);
        colorGrading.mixerBlueOutBlueIn.Override(87.5f);
    }
    
    private void AplicarFiltroDeuteranomalia()
    {
        // Deficiencia al verde (menos severa)
        colorGrading.mixerRedOutRedIn.Override(80f);
        colorGrading.mixerRedOutGreenIn.Override(20f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(25.8f);
        colorGrading.mixerGreenOutGreenIn.Override(74.2f);
        colorGrading.mixerGreenOutBlueIn.Override(0f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(14.2f);
        colorGrading.mixerBlueOutBlueIn.Override(85.8f);
    }
    
    private void AplicarFiltroTritanomalia()
    {
        // Deficiencia al azul (menos severa)
        colorGrading.mixerRedOutRedIn.Override(96.7f);
        colorGrading.mixerRedOutGreenIn.Override(3.3f);
        colorGrading.mixerRedOutBlueIn.Override(0f);
        
        colorGrading.mixerGreenOutRedIn.Override(0f);
        colorGrading.mixerGreenOutGreenIn.Override(73.3f);
        colorGrading.mixerGreenOutBlueIn.Override(26.7f);
        
        colorGrading.mixerBlueOutRedIn.Override(0f);
        colorGrading.mixerBlueOutGreenIn.Override(18.3f);
        colorGrading.mixerBlueOutBlueIn.Override(81.7f);
    }
    
    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Debug.Log("🔄 GestorAccesibilidad destruido");
            Instancia = null;
        }
    }
}