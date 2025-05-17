using UnityEngine;

public class CamaraSeguirBola : MonoBehaviour
{
    public Transform bola; // Referencia al objeto que la cámara debe seguir
    public Vector3 offset; // Desplazamiento de la cámara respecto a la bola

    public bool seguirBola = true;
    void LateUpdate()
    {
        if (seguirBola && bola != null)
        {

             transform.position = bola.position + offset;
             transform.LookAt(bola.position);}
    }

     public void ActualizarBola(Transform nuevaBola)
    {
        this.bola = nuevaBola;
        seguirBola = true;
    }

    public void DetenerSeguimiento()
    {
        seguirBola = false;
    }

    
}