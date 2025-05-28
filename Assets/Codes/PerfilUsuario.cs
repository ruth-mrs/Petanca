using UnityEngine;
using System;

[System.Serializable]
public class PerfilUsuario // ELIMINADO: MonoBehaviour
{
    public string nombreUsuario;
    public float fuerzaBase = 7f;
    public bool esZurdo = false;
    public bool perfilReducido = false; 
    public float factorAyuda = 1.0f;
    public float sensibilidadMovimiento = 1.0f;
    public float aceleracionMaximaCalibrada = 0f;
    public long fechaCreacion;

    public PerfilUsuario()
    {
        fechaCreacion = DateTime.Now.Ticks;
    }
    
    public PerfilUsuario(string nombre, bool zurdo, bool reducido)
    {
        nombreUsuario = nombre;
        esZurdo = zurdo;
        perfilReducido = reducido;
        fechaCreacion = DateTime.Now.Ticks;
    }

    // NUEVO: Constructor completo para la creación de perfiles
    public PerfilUsuario(string nombre, bool zurdo, bool reducido, float aceleracion)
    {
        nombreUsuario = nombre;
        esZurdo = zurdo;
        perfilReducido = reducido;
        aceleracionMaximaCalibrada = aceleracion;
        fechaCreacion = DateTime.Now.Ticks;
        actualizarFactorAyuda();
    }

    public float getFuerzaBase()
    {
        return fuerzaBase * factorAyuda;
    }

    public void setFuerzaBase(float fuerza)
    {
        fuerzaBase = fuerza;
    }
    
    public void actualizarFactorAyuda()
    {
        if (aceleracionMaximaCalibrada < 2.0f)
            factorAyuda = 1.5f;
        else if (aceleracionMaximaCalibrada < 4.0f)
            factorAyuda = 1.25f;
        else if (aceleracionMaximaCalibrada < 6.0f)
            factorAyuda = 1.1f;
        else
            factorAyuda = 1.0f;
            
        if (perfilReducido)
            factorAyuda += 0.2f;
    }
}