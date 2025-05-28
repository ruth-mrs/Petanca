using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GestorPerfiles : MonoBehaviour
{
    // Singleton para acceso global
    private static GestorPerfiles _instancia;

    private List<DatosPerfilUsuario> perfilesGuardados = new List<DatosPerfilUsuario>();
    
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
    
    // Perfil actualmente seleccionado (índice)
    public int indicePerfilActual = 0;
    
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
        if (perfilesGuardados.Count == 0)
        {
            DatosPerfilUsuario perfilPredeterminado = new DatosPerfilUsuario("Jugador", false, false, 2.5f);
            perfilesGuardados.Add(perfilPredeterminado);
            indicePerfilActual = 0;
            GuardarPerfiles();
        }
        else if (indicePerfilActual >= perfilesGuardados.Count)
        {
            // Ajustar índice si está fuera de rango
            indicePerfilActual = 0;
        }
    }
    
    // Generar nombre aleatorio de jugador famoso
    public string GenerarNombreAleatorio()
    {
        int indice = Random.Range(0, nombresJugadores.Length);
        return nombresJugadores[indice];
    }
    
    // Crear un nuevo perfil
    public void CrearPerfil(string nombre, bool esZurdo, bool perfilReducido, float aceleracionMaxima = 0f)
    {
        Debug.Log($"GestorPerfiles - Creando perfil: {nombre}, Zurdo: {esZurdo}, Reducido: {perfilReducido}, Aceleración: {aceleracionMaxima}");
        
        DatosPerfilUsuario nuevoPerfil = new DatosPerfilUsuario(nombre, esZurdo, perfilReducido, aceleracionMaxima);
        
        perfilesGuardados.Add(nuevoPerfil);
        indicePerfilActual = perfilesGuardados.Count - 1; // Seleccionar el nuevo perfil
        
        Debug.Log($"GestorPerfiles - Perfil añadido. Total perfiles: {perfilesGuardados.Count}");        
        GuardarPerfiles();
    }
    
    // Actualizar un perfil existente por índice
    public void ActualizarPerfil(int indice, string nombre, bool esZurdo, bool perfilReducido, float aceleracionMaxima = -1f)
    {
        if (indice < 0 || indice >= perfilesGuardados.Count)
        {
            Debug.LogError($"Índice de perfil inválido: {indice}");
            return;
        }
        
        Debug.Log($"GestorPerfiles - Actualizando perfil en índice {indice}: {nombre}");
        
        DatosPerfilUsuario perfil = perfilesGuardados[indice];

        
        perfil.nombreUsuario = nombre;
        perfil.esZurdo = esZurdo;
        perfil.perfilReducido = perfilReducido;
        
        // Solo actualizar aceleración si se recalibró (valor >= 0)
        if (aceleracionMaxima >= 0)
        {
            perfil.aceleracionMaximaCalibrada = aceleracionMaxima;
            perfil.actualizarFactorAyuda();
        }
        
        GuardarPerfiles();
    }
    
    // Actualizar perfil actual
    public void ActualizarPerfilActual(string nombre, bool esZurdo, bool perfilReducido, float aceleracionMaxima = -1f)
    {
        ActualizarPerfil(indicePerfilActual, nombre, esZurdo, perfilReducido, aceleracionMaxima);
    }
    
    // Eliminar un perfil por índice
    public void EliminarPerfil(int indice)
    {
        if (indice < 0 || indice >= perfilesGuardados.Count)
        {
            Debug.LogError($"Índice de perfil inválido: {indice}");
            return;
        }
        
        if (perfilesGuardados.Count <= 1)
        {
            Debug.LogWarning("No se puede eliminar el único perfil existente");
            return;
        }
        
        Debug.Log($"GestorPerfiles - Eliminando perfil en índice {indice}: {perfilesGuardados[indice].nombreUsuario}");
        
        perfilesGuardados.RemoveAt(indice);
        
        // Ajustar índice del perfil actual
        if (indicePerfilActual >= perfilesGuardados.Count)
        {
            indicePerfilActual = perfilesGuardados.Count - 1;
        }
        else if (indicePerfilActual > indice)
        {
            indicePerfilActual--;
        }
        
        GuardarPerfiles();
    }
    
    // Eliminar perfil actual
    public void EliminarPerfilActual()
    {
        EliminarPerfil(indicePerfilActual);
    }
    
    // Obtener perfil por índice
    public DatosPerfilUsuario ObtenerPerfil(int indice)
    {
        if (indice < 0 || indice >= perfilesGuardados.Count)
        {
            Debug.LogError($"Índice de perfil inválido: {indice}");
            return null;
        }
        
        return perfilesGuardados[indice];
    }
    
    // Obtener perfil actual
    public DatosPerfilUsuario ObtenerPerfilActual()
    {
        return ObtenerPerfil(indicePerfilActual);
    }
    
    // Obtener todos los perfiles
    public List<DatosPerfilUsuario> ObtenerPerfiles()
    {
        return new List<DatosPerfilUsuario>(perfilesGuardados); // Devolver copia para evitar modificaciones externas
    }
    
    // Cambiar perfil actual
    public void CambiarPerfilActual(int nuevoIndice)
    {
        if (nuevoIndice < 0 || nuevoIndice >= perfilesGuardados.Count)
        {
            Debug.LogError($"Índice de perfil inválido: {nuevoIndice}");
            return;
        }
        
        indicePerfilActual = nuevoIndice;
        Debug.Log($"Perfil actual cambiado a: {perfilesGuardados[indicePerfilActual].nombreUsuario}");
    }
    
    // Obtener cantidad de perfiles
    public int CantidadPerfiles()
    {
        return perfilesGuardados.Count;
    }
    
    // Crear PerfilUsuario (MonoBehaviour) desde DatosPerfilUsuario
    public PerfilUsuario CrearPerfilUsuarioMonoBehaviour(DatosPerfilUsuario datos)
    {
        GameObject go = new GameObject($"PerfilUsuario_{datos.nombreUsuario}");
        PerfilUsuario perfilMB = go.AddComponent<PerfilUsuario>();
        perfilMB.InicializarConDatos(datos);
        return perfilMB;
    }
    
    // Guardar todos los perfiles en archivo
    public void GuardarPerfiles()
    {
        PerfilesData data = new PerfilesData();
        data.perfiles = perfilesGuardados;
        data.indicePerfilActivo = indicePerfilActual;
        
        string json = JsonUtility.ToJson(data, true);
        string rutaArchivo = Path.Combine(Application.persistentDataPath, "perfiles.json");
        
        try
        {
            File.WriteAllText(rutaArchivo, json);
            Debug.Log("Perfiles guardados en: " + rutaArchivo);
            Debug.Log($"Guardados {perfilesGuardados.Count} perfiles, perfil actual: {indicePerfilActual}");
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
                Debug.Log("Cargando perfiles desde: " + rutaArchivo);
                
                PerfilesData data = JsonUtility.FromJson<PerfilesData>(json);
                
                if (data != null && data.perfiles != null && data.perfiles.Count > 0)
                {
                    perfilesGuardados = data.perfiles;
                    
                    // Recuperar el índice del perfil activo
                    if (data.indicePerfilActivo >= 0 && data.indicePerfilActivo < perfilesGuardados.Count)
                    {
                        indicePerfilActual = data.indicePerfilActivo;
                    }
                    else
                    {
                        indicePerfilActual = 0;
                    }
                    
                    Debug.Log($"Cargados {perfilesGuardados.Count} perfiles, perfil actual: {indicePerfilActual}");
                }
                else
                {
                    Debug.LogWarning("Datos de perfiles inválidos o vacíos");
                    perfilesGuardados.Clear();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al cargar perfiles: " + e.Message);
                perfilesGuardados.Clear();
            }
        }
        else
        {
            Debug.Log("No se encontró archivo de perfiles. Se crearán perfiles por defecto.");
            perfilesGuardados.Clear();
        }
    }
    
    // Verificar si existe un perfil con el nombre dado
    public bool ExistePerfilConNombre(string nombre)
    {
        foreach (var perfil in perfilesGuardados)
        {
            if (perfil.nombreUsuario.Equals(nombre, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    
    // Obtener nombres de todos los perfiles
    public List<string> ObtenerNombresPerfiles()
    {
        List<string> nombres = new List<string>();
        foreach (var perfil in perfilesGuardados)
        {
            nombres.Add(perfil.nombreUsuario);
        }
        return nombres;
    }
    
    [System.Serializable]
    private class PerfilesData
    {
        public List<DatosPerfilUsuario> perfiles = new List<DatosPerfilUsuario>();
        public int indicePerfilActivo = 0;
    }
}