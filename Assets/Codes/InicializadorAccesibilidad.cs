using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class InicializadorAccesibilidad : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabGestorAccesibilidad;
    
    private void Awake()
    {
        // Verificar si ya existe el gestor
        if (GestorAccesibilidad.Instancia == null)
        {
            // Si no existe, crear uno
            if (prefabGestorAccesibilidad != null)
            {
                Instantiate(prefabGestorAccesibilidad);
            }
            else
            {
                // Crear gestor básico
                GameObject gestorObj = new GameObject("GestorAccesibilidad");
                gestorObj.AddComponent<GestorAccesibilidad>();
                Debug.Log("GestorAccesibilidad creado automáticamente");
            }
        }
        
        // Configurar cámara automáticamente
        ConfigurarCamaraConLayer();
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
                Debug.Log("✅ PostProcessLayer añadido a la cámara");
            }
            
            // Intentar usar layer "PostProcess" o "Everything" como fallback
            int postProcessLayer = LayerMask.NameToLayer("PostProcess");
            if (postProcessLayer != -1)
            {
                postLayer.volumeLayer = 1 << postProcessLayer; // Layer específica
                Debug.Log("✅ Usando layer 'PostProcess'");
            }
            else
            {
                postLayer.volumeLayer = -1; // Everything
                Debug.Log("✅ Usando layer 'Everything' (fallback)");
            }
            
            postLayer.volumeTrigger = mainCamera.transform;
            Debug.Log("✅ Cámara configurada automáticamente");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró cámara principal");
        }
    }
}