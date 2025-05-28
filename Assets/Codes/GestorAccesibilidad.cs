using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

    public enum TipoDaltonismo
    {
        Normal = 0,
        Protanopia = 1,
        Deuteranopia = 2,
        Tritanopia = 3,
        Protanomalia = 4,
        Deuteranomalia = 5,
        Tritanomalia = 6
    }

public class GestorAccesibilidad : MonoBehaviour
{
    public static GestorAccesibilidad Instancia { get; private set; }

    [Header("Volume")]
    public Volume volumenProcesamiento;
    private ColorAdjustments colorAdjustments;
    private bool sistemaInicializado = false;

    private void Awake()
    {
        // Singleton persistente entre escenas
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GestorAccesibilidadURP creado como singleton");
        }
        else
        {
            Debug.Log("⚠️ GestorAccesibilidadURP ya existe, destruyendo duplicado");
            Destroy(gameObject);
            return;
        }

        if (volumenProcesamiento != null)
            volumenProcesamiento.weight = 1f;
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

        Debug.Log("🔧 Inicializando sistema de accesibilidad URP...");

        if (volumenProcesamiento == null)
        {
            volumenProcesamiento = FindObjectOfType<Volume>();
            Debug.Log(volumenProcesamiento != null ? "📍 Volume encontrado" : "❌ Volume no encontrado");
        }

        if (volumenProcesamiento == null)
        {
            CrearVolume();
        }

        if (ConfigurarColorAdjustments())
        {
            sistemaInicializado = true;
            CargarYAplicarFiltroGuardado();
            Debug.Log("✅ Sistema de accesibilidad URP inicializado correctamente");
        }
        else
        {
            Debug.LogError("❌ Error al inicializar el sistema de accesibilidad URP");
        }
    }

    private void CrearVolume()
    {
        Debug.Log("🔨 Creando Volume global...");
        GameObject volumeObj = new GameObject("Global Volume");
        volumeObj.layer = LayerMask.NameToLayer("PostProcessing"); // asegúrate que ese layer exista o usa Default
        volumenProcesamiento = volumeObj.AddComponent<Volume>();
        volumenProcesamiento.isGlobal = true;
        volumenProcesamiento.priority = 1f;

        // Crear y asignar un perfil nuevo
        volumenProcesamiento.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        DontDestroyOnLoad(volumeObj);
        Debug.Log("✅ Volume creado");
    }

    private bool ConfigurarColorAdjustments()
    {
        try
        {
            if (volumenProcesamiento.profile == null)
            {
                volumenProcesamiento.profile = ScriptableObject.CreateInstance<VolumeProfile>();
                Debug.Log("📋 Perfil de Volume creado");
            }

            if (!volumenProcesamiento.profile.TryGet<ColorAdjustments>(out colorAdjustments))
            {
                colorAdjustments = volumenProcesamiento.profile.Add<ColorAdjustments>(true);
                Debug.Log("🎨 ColorAdjustments añadido al perfil");
            }

            colorAdjustments.active = true;
            Debug.Log("✅ ColorAdjustments configurado correctamente");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error configurando ColorAdjustments: {e.Message}");
            return false;
        }
    }

        public bool SistemaListo()
    {
        return sistemaInicializado;
    }

    public void ForzarInicializacion()
    {
        InicializarSistema();
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
        Debug.Log($"💾 Cargando filtro guardado: {filtroGuardado}");
        AplicarFiltro((TipoDaltonismo)filtroGuardado);
    }

    public void AplicarFiltro(TipoDaltonismo tipo)
    {
        if (!sistemaInicializado)
        {
            Debug.LogWarning("⚠️ Sistema no inicializado. Inicializando...");
            InicializarSistema();
            if (!sistemaInicializado) return;
        }

        if (colorAdjustments == null)
        {
            Debug.LogError("❌ ColorAdjustments es null. No se puede aplicar filtro.");
            return;
        }

        PlayerPrefs.SetInt("FiltroAccesibilidad", (int)tipo);
        PlayerPrefs.Save();

        // Reset matrix antes de aplicar filtro
        colorAdjustments.colorFilter.value = Color.white;

        switch (tipo)
        {
            case TipoDaltonismo.Normal:
                AplicarFiltroNormal();
                break;
            case TipoDaltonismo.Protanopia:
                AplicarFiltroProtanopia();
                break;
            case TipoDaltonismo.Deuteranopia:
                AplicarFiltroDeuteranopia();
                break;
            case TipoDaltonismo.Tritanopia:
                AplicarFiltroTritanopia();
                break;
            case TipoDaltonismo.Protanomalia:
                AplicarFiltroProtanomalia();
                break;
            case TipoDaltonismo.Deuteranomalia:
                AplicarFiltroDeuteranomalia();
                break;
            case TipoDaltonismo.Tritanomalia:
                AplicarFiltroTritanomalia();
                break;
        }

        Debug.Log($"✅ Filtro aplicado y guardado: {tipo}");
    }

    private void AplicarFiltroNormal()
    {
        colorAdjustments.colorFilter.Override(Color.white);
    }

    // Aquí uso el colorFilter para simular las matrices, no es exacto pero para probar va bien.
    // Para algo más avanzado necesitarías un shader personalizado.

    private void AplicarFiltroProtanopia()
    {
        colorAdjustments.colorFilter.Override(new Color(0.567f, 0.433f, 0f));
    }

    private void AplicarFiltroDeuteranopia()
    {
        colorAdjustments.colorFilter.Override(new Color(0.625f, 0.375f, 0f));
    }

    private void AplicarFiltroTritanopia()
    {
        colorAdjustments.colorFilter.Override(new Color(0.95f, 0.05f, 0f));
    }

    private void AplicarFiltroProtanomalia()
    {
        colorAdjustments.colorFilter.Override(new Color(0.817f, 0.183f, 0f));
    }

    private void AplicarFiltroDeuteranomalia()
    {
        colorAdjustments.colorFilter.Override(new Color(0.8f, 0.2f, 0f));
    }

    private void AplicarFiltroTritanomalia()
    {
        colorAdjustments.colorFilter.Override(new Color(0.967f, 0.033f, 0f));
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Debug.Log("🔄 GestorAccesibilidadURP destruido");
            Instancia = null;
        }
    }
}
