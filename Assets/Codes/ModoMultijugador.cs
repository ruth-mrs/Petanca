using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;

public class ModoMultijugador : MonoBehaviour
{
    public int bolasRestantesJ1 = 3;

    public int bolasRestantesJ2 = 3;
    public double distanciaJ1 = -1.0;

    public double distanciaJ2 = -1.0;


    public Canvas canvas;

    public GUIStyle estilo;

    public bool mostrarFinJuego = false;
    public bool mostrarPausa = false;
    private bool cambioPausa = false;

     public TMP_Text textoJ1Nombre, textoJ1Bolas, textoJ1Distancia;
    public TMP_Text textoJ2Nombre, textoJ2Bolas, textoJ2Distancia;
    public GameObject coronaJ1, coronaJ2;
    public LineRenderer lineaJ1, lineaJ2;
    public GameObject panelResultado;
    public TMP_Text textoGanador, textoDistanciaGanadora;

    private Vector3 posBolaJ1 = Vector3.zero;
    private Vector3 posBolaJ2 = Vector3.zero;
    private Vector3 posBoliche = Vector3.zero;

    void Start()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            canvas.enabled = false;

            if (textoJ1Nombre) textoJ1Nombre.text = "JUGADOR 1";
            if (textoJ2Nombre) textoJ2Nombre.text = "JUGADOR 2";
            ActualizarUI();

        }

    }    

    void Update()
    {
        ActualizarUI();
        ActualizarCoronas();
        ActualizarLineas();
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
        mostrarFinJuego = true;
        canvas.enabled = true;
        MostrarPanelResultado();
        
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

        ActualizarUI();
    }

     private void ActualizarUI()
    {
        if (textoJ1Bolas) textoJ1Bolas.text = $"Bolas: {bolasRestantesJ1}";
        if (textoJ2Bolas) textoJ2Bolas.text = $"Bolas: {bolasRestantesJ2}";

        if (textoJ1Distancia)
        {
            textoJ1Distancia.text = distanciaJ1 == -1.0 ? "Distancia: --" : $"Distancia: {distanciaJ1:F2} m";
        }
        if (textoJ2Distancia)
        {
            textoJ2Distancia.text = distanciaJ2 == -1.0 ? "Distancia: --" : $"Distancia: {distanciaJ2:F2} m";
        }
    }

    private void ActualizarCoronas()
    {
        if (coronaJ1 && coronaJ2)
        {
            if (distanciaJ1 != -1.0 && (distanciaJ1 < distanciaJ2 || distanciaJ2 == -1.0))
            {
                coronaJ1.SetActive(true);
                coronaJ2.SetActive(false);
            }
            else if (distanciaJ2 != -1.0)
            {
                coronaJ1.SetActive(false);
                coronaJ2.SetActive(true);
            }
            else
            {
                coronaJ1.SetActive(false);
                coronaJ2.SetActive(false);
            }
        }
    }

    private void ActualizarLineas()
    {
        if (lineaJ1 && posBolaJ1 != Vector3.zero && posBoliche != Vector3.zero)
        {
            lineaJ1.enabled = true;
            lineaJ1.positionCount = 2;
            lineaJ1.SetPosition(0, new Vector3(posBolaJ1.x, 0.05f, posBolaJ1.z));
            lineaJ1.SetPosition(1, new Vector3(posBoliche.x, 0.05f, posBoliche.z));
        }
        else if (lineaJ1)
        {
            lineaJ1.enabled = false;
        }

        if (lineaJ2 && posBolaJ2 != Vector3.zero && posBoliche != Vector3.zero)
        {
            lineaJ2.enabled = true;
            lineaJ2.positionCount = 2;
            lineaJ2.SetPosition(0, new Vector3(posBolaJ2.x, 0.05f, posBolaJ2.z));
            lineaJ2.SetPosition(1, new Vector3(posBoliche.x, 0.05f, posBoliche.z));
        }
        else if (lineaJ2)
        {
            lineaJ2.enabled = false;
        }
    }

    private void MostrarPanelResultado()
    {
        if (panelResultado && textoGanador && textoDistanciaGanadora)
        {
            panelResultado.SetActive(true);
            if (distanciaJ1 != -1.0 && (distanciaJ1 < distanciaJ2 || distanciaJ2 == -1.0))
            {
                textoGanador.text = "¡Ganador: JUGADOR 1!";
                textoGanador.color = new Color32(0x04, 0x6e, 0xc9, 255); // #046ec9
                textoDistanciaGanadora.text = $"Distancia más cercana: {distanciaJ1:F2} m";
            }
            else if (distanciaJ2 != -1.0)
            {
                textoGanador.text = "¡Ganador: JUGADOR 2!";
                textoGanador.color = new Color32(0xc5, 0x32, 0x2a, 255); // #c5322a
                textoDistanciaGanadora.text = $"Distancia más cercana: {distanciaJ2:F2} m";
            }
            else
            {
                textoGanador.text = "¡Empate!";
                textoDistanciaGanadora.text = "";
            }
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

 
