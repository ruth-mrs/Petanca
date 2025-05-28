using UnityEngine;
using System;

public class PerfilUsuario : MonoBehaviour
{
    [SerializeField] private DatosPerfilUsuario datos;

    public string nombreUsuario
    {
        get { return datos.nombreUsuario; }
        set { datos.nombreUsuario = value; }
    }

    public float fuerzaBase
    {
        get { return datos.fuerzaBase; }
        set { datos.fuerzaBase = value; }
    }

    public bool esZurdo
    {
        get { return datos.esZurdo; }
        set { datos.esZurdo = value; }
    }

    public bool perfilReducido
    {
        get { return datos.perfilReducido; }
        set { datos.perfilReducido = value; }
    }

    public float factorAyuda
    {
        get { return datos.factorAyuda; }
        set { datos.factorAyuda = value; }
    }

    public float sensibilidadMovimiento
    {
        get { return datos.sensibilidadMovimiento; }
        set { datos.sensibilidadMovimiento = value; }
    }

    public float aceleracionMaximaCalibrada
    {
        get { return datos.aceleracionMaximaCalibrada; }
        set { datos.aceleracionMaximaCalibrada = value; }
    }

    public long fechaCreacion
    {
        get { return datos.fechaCreacion; }
    }

    void Awake()
    {
        if (datos == null)
            datos = new DatosPerfilUsuario();
    }

    public void InicializarConDatos(DatosPerfilUsuario datosExistentes)
    {
        datos = datosExistentes;
    }

    public DatosPerfilUsuario ObtenerDatos()
    {
        return datos;
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
        return datos.getFuerzaBase();
    }

    public void setFuerzaBase(float fuerza)
    {
        datos.setFuerzaBase(fuerza);
    }
    
    public void actualizarFactorAyuda()
    {
        datos.actualizarFactorAyuda();
    }
}