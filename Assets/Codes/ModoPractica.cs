using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ModoPractica : MonoBehaviour
{
    public int bolasRestantes = 3;
    public double distancia = -1.0;

    public Canvas canvas;

    public bool mostrarFinJuego = false;
    public bool mostrarPausa = false;
    private bool cambioPausa = false;

    // UI Elements (asigna en el inspector)
    public TMP_Text textoTitulo;
    public TMP_Text textoBolas;
    public TMP_Text textoDistancia;
    public GameObject panelResultado;
    public TMP_Text textoFinJuego;
    public TMP_Text textoDistanciaFinal;

    // Materiales para resaltar la bola más cercana
    public Material materialNormal;
    public Material materialResaltado;

    // Referencia a la bola más cercana actual
    private GameObject bolaMasCercanaActual;

    void Start()
    {
        if (GestorUI.Instance != null)
        {
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
            canvas.enabled = false;
        }
        ActualizarUI();
    }

    void Update()
    {
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoTitulo) textoTitulo.text = "MODO PRÁCTICA";
        if (textoBolas) textoBolas.text = $"Bolas restantes: {bolasRestantes}";
        if (textoDistancia)
        {
            textoDistancia.text = distancia == -1.0
                ? "Aún falta por lanzar"
                : $"Distancia más cercana: {distancia:F2} m";
        }
    }

    public void ActualizarDistancia(double nuevaDistancia)
    {
        distancia = nuevaDistancia;
        ActualizarUI();
    }

    /// <summary>
    /// Llama a este método cada vez que determines cuál es la bola más cercana.
    /// </summary>
    /// <param name="nuevaBola">GameObject de la nueva bola más cercana</param>
    /// <param name="nuevaDistancia">Distancia de la nueva bola más cercana</param>
    public void ActualizarBolaMasCercana(GameObject nuevaBola, double nuevaDistancia)
    {
        // Si hay una bola anterior resaltada y es diferente a la nueva, restaurar su material
        if (bolaMasCercanaActual != null && bolaMasCercanaActual != nuevaBola)
        {
            var rendererAnterior = bolaMasCercanaActual.GetComponent<Renderer>();
            if (rendererAnterior != null && materialNormal != null)
            {
                rendererAnterior.material = materialNormal;
            }
        }

        // Resalta la nueva bola más cercana
        if (nuevaBola != null)
        {
            var rendererNuevo = nuevaBola.GetComponent<Renderer>();
            if (rendererNuevo != null && materialResaltado != null)
            {
                rendererNuevo.material = materialResaltado;
            }
            bolaMasCercanaActual = nuevaBola;
            distancia = nuevaDistancia;
            ActualizarUI();
        }
    }

    public void FinJuego()
    {
        mostrarFinJuego = true;
        canvas.enabled = true;
        MostrarPanelResultado();
    }

    private void MostrarPanelResultado()
    {
        if (panelResultado && textoFinJuego && textoDistanciaFinal)
        {
            panelResultado.SetActive(true);
            textoFinJuego.text = "¡Fin del juego!";
            textoDistanciaFinal.text = distancia == -1.0
                ? "No se registró distancia."
                : $"Distancia más cercana: {distancia:F2} m";
        }
    }

    public void ReducirBolas()
    {
        if (bolasRestantes > 0)
        {
            bolasRestantes--;
            ActualizarUI();
        }
    }

    public void moverMenu(int movimiento)
    {
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
        if (!cambioPausa)
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
        if (!cambioPausa)
        {
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
            SceneManager.LoadScene("PetancaSolitario");
        }
        else if (botonSeleccionado == 1)
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }
}