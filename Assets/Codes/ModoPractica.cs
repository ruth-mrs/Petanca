using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModoPractica : MonoBehaviour
{
    public int bolasRestantes = 3;
    public double distancia = -1.0;

    public Canvas canvas;

    public GUIStyle estilo;

    public bool mostrarFinJuego = false;
    public bool mostrarPausa = false;
    private bool cambioPausa = false;

    void Start()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            canvas.enabled = false;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 40, 180, 30), "Bolas restantes: " + bolasRestantes, estilo);

        if (distancia == -1.0)
        {
            GUI.Label(new Rect(20, 70, 180, 30), "Distancia al boliche: Aún falta por lanzar ", estilo);
        }
        else
        {
            GUI.Label(new Rect(20, 70, 180, 30), "Distancia mas cercana: " + distancia.ToString("F2") + " metros", estilo);
        }
    }

    public void noController()
    {
        GUI.Label(new Rect(10, 120, 200, 30), "No se detectó el controlador.", estilo);
    }

    public void ActualizarDistancia(double nuevaDistancia)
    {
        distancia = nuevaDistancia;
    }

    public void FinJuego()
    {
        Debug.Log("Fin del juego");
        mostrarFinJuego = true;
        canvas.enabled = true;
    }

    public void ReducirBolas()
    {
        if (bolasRestantes > 0)
        {
            bolasRestantes--;
        }
    }

    public void moverMenu(int movimiento)
    {
        Debug.Log("Movimiento del menú: " + movimiento);
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.MoverMenu(movimiento);
        }
    }


    public void SeleccionarBoton()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.SeleccionarBoton();
        }
    }

    public void LiberarBoton()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.LiberarBoton();
        }
    }


    public void PausarJuego()
    {
        if(!cambioPausa)
        {
        cambioPausa = true;
    
       mostrarPausa = true;
       canvas.enabled = true;
       GestorUI.Instance.MoverMenu(0);
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
            SceneManager.LoadScene("PetancaSolitario");
        }
        else if (botonSeleccionado == 1)
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }
}
