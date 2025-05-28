using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GestorPerfiles : MonoBehaviour
{
    // Singleton para acceso global
    private static GestorPerfiles _instancia;
    public static GestorPerfiles Instancia
    {
        get
        {
            if (_instancia == null)
            {
                GameObject go = new GameObject("GestorPerfiles");
                _instancia = go.AddComponent<GestorPerfiles>();
                DontDestroyOnLoad(go);
            }
            return _instancia;
        }
    }
    
    // Lista de perfiles disponibles
    public List<PerfilUsuario> perfilesUsuarios = new List<PerfilUsuario>();
    
    // Perfil actualmente seleccionado
    public PerfilUsuario perfilActual;
    
    // Nombres para generación aleatoria
    private string[] nombresJugadores = new string[]
    {
        "Philippe Quintais", "Christian Fazzino", "Henri Lacroix", 
        "Dylan Rocher", "Philippe Suchaud", "Bruno Le Boursicaud", 
        "Marco Foyot", "Claudy Weibel", "Zvonko Radnic", 
        "Christian Longo", "Michel Loy", "André Massoni",
        "Didier Choupay", "Damien Hureau", "Pascal Milei",
        "Jean-Marc Foyot", "Joseph Farré", "Stéphane Robineau"
    };
    
    private void Awake()
    {
        // Singleton pattern
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instancia = this;
        DontDestroyOnLoad(gameObject);
        
        // Cargar perfiles existentes
        CargarPerfiles();
        
        // Si no hay perfiles, crear uno por defecto
        if (perfilesUsuarios.Count == 0)
        {
            PerfilUsuario perfilPredeterminado = new PerfilUsuario("Jugador", false, false, 2.5f);
            perfilesUsuarios.Add(perfilPredeterminado);
            perfilActual = perfilPredeterminado;
            GuardarPerfiles();
        }
        else if (perfilActual == null && perfilesUsuarios.Count > 0)
        {
            // Seleccionar el primer perfil si no hay uno activo
            perfilActual = perfilesUsuarios[0];
        }
    }
    
    // Generar nombre aleatorio de jugador famoso
    public string GenerarNombreAleatorio()
    {
        int indice = Random.Range(0, nombresJugadores.Length);
        return nombresJugadores[indice];
    }
    
    // Crear un nuevo perfil
    public void CrearPerfil(string nombre, bool esZurdo, bool perfilReducido, float aceleracionMaxima)
    {
        Debug.Log($"GestorPerfiles - Creando perfil: {nombre}, Zurdo: {esZurdo}, Reducido: {perfilReducido}, Aceleración: {aceleracionMaxima}");
        
        PerfilUsuario nuevoPerfil = new PerfilUsuario(nombre, esZurdo, perfilReducido, aceleracionMaxima);
        
        perfilesUsuarios.Add(nuevoPerfil);
        perfilActual = nuevoPerfil;
        
        Debug.Log($"GestorPerfiles - Perfil añadido. Total perfiles: {perfilesUsuarios.Count}");
        
        GuardarPerfiles();
    }
    
    // Actualizar un perfil existente
    public void ActualizarPerfil(PerfilUsuario perfil, string nombre, bool esZurdo, bool perfilReducido, float aceleracionMaxima)
    {
        Debug.Log($"GestorPerfiles - Actualizando perfil: {nombre}");
        
        perfil.nombreUsuario = nombre;
        perfil.esZurdo = esZurdo;
        perfil.perfilReducido = perfilReducido;
        
        // Solo actualizar aceleración si se recalibró (valor > 0)
        if (aceleracionMaxima > 0)
        {
            perfil.aceleracionMaximaCalibrada = aceleracionMaxima;
            perfil.actualizarFactorAyuda();
        }
        
        GuardarPerfiles();
    }
    
    // Eliminar un perfil
    public void EliminarPerfil(PerfilUsuario perfil)
    {
        if (perfilesUsuarios.Count <= 1)
        {
            Debug.LogWarning("No se puede eliminar el único perfil existente");
            return;
        }
        
        // Si es el perfil actual, cambiar a otro
        if (perfil == perfilActual)
        {
            int indice = perfilesUsuarios.IndexOf(perfil);
            int nuevoIndice = (indice + 1) % perfilesUsuarios.Count;
            if (nuevoIndice == indice) nuevoIndice = (indice > 0) ? indice - 1 : 0;
            
            perfilActual = perfilesUsuarios[nuevoIndice];
        }
        
        perfilesUsuarios.Remove(perfil);
        GuardarPerfiles();
    }
    
    // Guardar todos los perfiles en archivo
    public void GuardarPerfiles()
    {
        PerfilesData data = new PerfilesData();
        data.perfiles = perfilesUsuarios;
        
        // También guardar índice del perfil actual para recuperarlo al cargar
        if (perfilActual != null)
        {
            data.indicePerfilActivo = perfilesUsuarios.IndexOf(perfilActual);
        }
        
        string json = JsonUtility.ToJson(data, true);
        string rutaArchivo = Path.Combine(Application.persistentDataPath, "perfiles.json");
        
        try
        {
            File.WriteAllText(rutaArchivo, json);
            Debug.Log("Perfiles guardados en: " + rutaArchivo);
            Debug.Log("JSON guardado: " + json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al guardar perfiles: " + e.Message);
        }
    }
    
    // Cargar perfiles desde archivo
    public void CargarPerfiles()
    {
        string rutaArchivo = Path.Combine(Application.persistentDataPath, "perfiles.json");
        
        if (File.Exists(rutaArchivo))
        {
            try
            {
                string json = File.ReadAllText(rutaArchivo);
                Debug.Log("JSON cargado: " + json);
                
                PerfilesData data = JsonUtility.FromJson<PerfilesData>(json);
                
                if (data != null && data.perfiles != null)
                {
                    perfilesUsuarios = data.perfiles;
                    
                    // Recuperar el perfil activo
                    if (data.indicePerfilActivo >= 0 && data.indicePerfilActivo < perfilesUsuarios.Count)
                    {
                        perfilActual = perfilesUsuarios[data.indicePerfilActivo];
                    }
                    else if (perfilesUsuarios.Count > 0)
                    {
                        perfilActual = perfilesUsuarios[0];
                    }
                    
                    Debug.Log($"Cargados {perfilesUsuarios.Count} perfiles");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al cargar perfiles: " + e.Message);
            }
        }
        else
        {
            Debug.Log("No se encontró archivo de perfiles. Se crearán perfiles por defecto.");
        }
    }
    
    [System.Serializable]
    private class PerfilesData
    {
        public List<PerfilUsuario> perfiles;
        public int indicePerfilActivo = -1;
    }
}