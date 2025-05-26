using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ModoMultijugador : MonoBehaviour
{
    public int bolasRestantesJ1 = 3;

    public int bolasRestantesJ2 = 3;
    public double distanciaJ1 = -1.0;

    public double distanciaJ2 = -1.0;


    public Canvas canvas;
    private GestorUI gestorUI;

    public GUIStyle estilo;

    public bool mostrarFinJuego = false;
    public bool mostrarPausa = false;
    private bool cambioPausa = false;

    void Start()
    {
      gestorUI = GetComponent<GestorUI>();
        if (gestorUI != null)
        {
            gestorUI.Inicializar(canvas);
            gestorUI.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            canvas.enabled = false;


        }

    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 100, 180, 30), "Bolas restantes J1: " + bolasRestantesJ1, estilo);
        GUI.Label(new Rect(20, 130, 180, 30), "Bolas restantes J2: " + bolasRestantesJ2, estilo);

        if(distanciaJ1 == -1.0 && distanciaJ2 == -1.0){
            GUI.Label(new Rect(20, 70, 180, 30), "Distancia al boliche: Aún falta por lanzar ", estilo);
        }else{    
            if(distanciaJ1 == -1.0){
                GUI.Label(new Rect(20, 70, 180, 30), "Distancia mas cercana J2: " + distanciaJ2.ToString("F2") +" metros", estilo);
            }else if(distanciaJ2 == -1.0){
                GUI.Label(new Rect(20, 70, 180, 30), "Distancia mas cercana J1: " + distanciaJ1.ToString("F2") +" metros", estilo);
            }else if(distanciaJ1 < distanciaJ2){
                GUI.Label(new Rect(20, 70, 180, 30), "La bola mas cercana es de J1: " + distanciaJ1.ToString("F2") +" metros", estilo);
            }else{
                GUI.Label(new Rect(20, 70, 180, 30), "Distancia mas cercana es de J2: " + distanciaJ2.ToString("F2") +" metros", estilo);
            }
           
        }  


     
    }
    

    public void noController()
    {
        GUI.Label(new Rect(10, 120, 200, 30), "No se detectó el controlador.", estilo);
    }

    public void ActualizarDistancia(double nuevaDistancia, int jugador)
    {
        if (jugador == 1)
        {
            distanciaJ1 = nuevaDistancia;
        }
        else if (jugador == 2)
        {
            distanciaJ2 = nuevaDistancia;
        }
    }
  
    public void FinJuego(){
        Debug.Log("Fin del juego");
        mostrarFinJuego = true;
        canvas.enabled = true;
        
    }


    public void ReducirBolas(int jugador)
    {
        if (jugador == 1 && bolasRestantesJ1 > 0)
        {
            bolasRestantesJ1--;
        }
        else if (jugador == 2 && bolasRestantesJ2 > 0)
        {
            bolasRestantesJ2--;
        }
    }
 

    public void moverMenu(int movimiento)
    {
        Debug.Log("Movimiento del menú: " + movimiento);
        if (gestorUI != null)
        {
            gestorUI.MoverMenu(movimiento);
        }
    }

    public void SeleccionarBoton()
    {
        if (gestorUI != null)
        {
            gestorUI.SeleccionarBoton();
        }
    }

    public void LiberarBoton()
    {
        if (gestorUI != null)
        {
            gestorUI.LiberarBoton();
        }
    }

     public void PausarJuego()
    {
        if(!cambioPausa)
        {
        cambioPausa = true;
    
       mostrarPausa = true;
       canvas.enabled = true;
        gestorUI.MoverMenu(0);
         StartCoroutine(DesactivarCambioPausa());
        }
    }

    public void SalirMenu()
    {
        if(!cambioPausa){
        mostrarPausa = false;
        canvas.enabled = false;
        cambioPausa = true;
        StartCoroutine(DesactivarCambioPausa());
        }
    

    }

    private IEnumerator DesactivarCambioPausa()
    {
        yield return new WaitForSeconds(0.5f);
        cambioPausa = false;
    }


    public void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);

        if (botonSeleccionado == 0)
        {
            SceneManager.LoadScene("PetancaMultijugador");
        }
        else if (botonSeleccionado == 1)
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }
}

 
