using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GestorAudio : MonoBehaviour
{
    // Singleton para acceso global
    private static GestorAudio instancia;
    public static GestorAudio Instancia
    {
        get { return instancia; }
    }
    
    [System.Serializable]
    public class ConfiguracionMusica
    {
        public string nombreGrupo;
        public List<string> escenas = new List<string>();
        public AudioClip musicaAmbiente;
        public float volumenRelativo = 1.0f;
        public bool repetir = true;
    }
    
    [Header("Configuración de Música")]
    public List<ConfiguracionMusica> configuracionesMusica = new List<ConfiguracionMusica>();
    
    // Audio Sources
    private AudioSource musicaActual;
    private string grupoActual = "";
    
    // Control de transiciones
    [Header("Transiciones")]
    public float tiempoDesvanecimiento = 1.0f;
    private AudioSource musicaSiguiente;
    private float tiempoTransicion = 0f;
    private bool enTransicion = false;
    
    private void Awake()
    {
        // Implementación del singleton
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instancia = this;
        DontDestroyOnLoad(gameObject);
        
        // Crear componentes de audio
        musicaActual = gameObject.AddComponent<AudioSource>();
        musicaSiguiente = gameObject.AddComponent<AudioSource>();
        
        // Configuración inicial
        musicaActual.loop = true;
        musicaSiguiente.loop = true;
        musicaSiguiente.volume = 0f;

        // Iniciar volumen al 15%
        AudioListener.volume = 0.15f;
        musicaActual.volume = 0.15f;
        
        // Registrar para eventos de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode modo)
    {
        // Buscar qué música debe reproducirse en esta escena
        string nombreEscena = scene.name;
        ConfiguracionMusica config = ObtenerConfiguracionParaEscena(nombreEscena);
        
        if (config != null)
        {
            // Si es un grupo diferente, cambiar la música
            if (grupoActual != config.nombreGrupo)
            {
                CambiarMusica(config);
            }
        }
    }
    
    private ConfiguracionMusica ObtenerConfiguracionParaEscena(string nombreEscena)
    {
        foreach (ConfiguracionMusica config in configuracionesMusica)
        {
            if (config.escenas.Contains(nombreEscena))
            {
                return config;
            }
        }
        
        return null; // No hay música específica para esta escena
    }
    
    private void CambiarMusica(ConfiguracionMusica nuevaConfig)
    {
        // Si no hay música para reproducir, detener la actual
        if (nuevaConfig == null || nuevaConfig.musicaAmbiente == null)
        {
            StopAllCoroutines();
            musicaActual.Stop();
            grupoActual = "";
            return;
        }
        
        // Si es el mismo clip, no hacer nada
        if (musicaActual.clip == nuevaConfig.musicaAmbiente && musicaActual.isPlaying)
        {
            return;
        }
        
        // Iniciar transición
        IniciarTransicion(nuevaConfig);
        
        // Actualizar grupo actual
        grupoActual = nuevaConfig.nombreGrupo;
    }
    
    private void IniciarTransicion(ConfiguracionMusica nuevaConfig)
    {
        // Configurar nueva música
        musicaSiguiente.clip = nuevaConfig.musicaAmbiente;
        musicaSiguiente.volume = 0f;
        musicaSiguiente.loop = nuevaConfig.repetir;
        
        // Comenzar a reproducir la nueva música
        musicaSiguiente.Play();
        
        // Comenzar transición
        enTransicion = true;
        tiempoTransicion = 0f;
    }
    
    private void Update()
    {
        if (enTransicion)
        {
            tiempoTransicion += Time.deltaTime;
            float t = tiempoTransicion / tiempoDesvanecimiento;
            
            if (t >= 1.0f)
            {
                // Transición completa
                FinalizarTransicion();
            }
            else
            {
                // Actualizar volúmenes
                musicaActual.volume = Mathf.Lerp(1, 0, t) * AudioListener.volume;
                musicaSiguiente.volume = Mathf.Lerp(0, 1, t) * AudioListener.volume;
            }
        }
        else if (musicaActual.isPlaying)
        {
            // Mantener el volumen relativo al volumen maestro
            musicaActual.volume = AudioListener.volume;
        }
    }
    
    private void FinalizarTransicion()
    {
        // Detener música anterior
        musicaActual.Stop();
        
        // Intercambiar referencias
        AudioSource temp = musicaActual;
        musicaActual = musicaSiguiente;
        musicaSiguiente = temp;
        
        // Ajustar volumen final
        musicaActual.volume = AudioListener.volume;
        
        // Fin de la transición
        enTransicion = false;
    }
    
    // Métodos públicos para controlar la música
    public void DetenerMusica()
    {
        musicaActual.Stop();
        musicaSiguiente.Stop();
        enTransicion = false;
    }
    
    public void ReanudarMusica()
    {
        if (!musicaActual.isPlaying && musicaActual.clip != null)
        {
            musicaActual.Play();
        }
    }
    
    public void PausarMusica()
    {
        musicaActual.Pause();
    }
    
    private void OnDestroy()
    {
        // Eliminar registro de eventos
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}