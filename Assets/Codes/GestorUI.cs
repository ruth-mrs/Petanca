using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class GestorUI : MonoBehaviour
{
    private List<Button> botones; // Lista de botones detectados en el Canvas
    private int botonSeleccionado = 0; // Índice del botón actualmente seleccionado

    public Action<int> OnBotonSeleccionado; // Evento para ejecutar la acción del botón seleccionado
    private bool botonProcesado = false;
   public static GestorUI Instance { get; private set; }

    private void Awake()
    {
    
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }


    public void Inicializar(Canvas canvas)
    {    
        botonSeleccionado = 0;
        botonProcesado = true;
        botones = new List<Button>(canvas.GetComponentsInChildren<Button>());

        StartCoroutine(HabilitarEntradaTrasUnFrame());


        if (botones.Count == 0)
        {
            Debug.LogWarning("No se encontraron botones en el Canvas.");
            return;
        }

        ActualizarSeleccion();
    }

        private IEnumerator HabilitarEntradaTrasUnFrame()
    {
        yield return new WaitForSeconds(5f);
        botonProcesado = false;;
    }
        

    public void MoverMenu(int movimiento)
    {
        if( movimiento == 0) botonSeleccionado = 0;
        if (botones.Count == 0 || botonProcesado) return;


        botonSeleccionado += movimiento;

        if (botonSeleccionado < 0) botonSeleccionado = botones.Count - 1;
        if (botonSeleccionado >= botones.Count) botonSeleccionado = 0;

        ActualizarSeleccion();

        botonProcesado = true;

    }

    public void SeleccionarBoton()
    {
        if (botones.Count == 0 || botonProcesado) return;

        botones[botonSeleccionado].onClick.Invoke();

        OnBotonSeleccionado?.Invoke(botonSeleccionado);

        botonProcesado = true;

    }

    public void LiberarBoton()
    {
        botonProcesado = false;
    }

    public int ObtenerCantidadBotones()
    {
        return botones != null ? botones.Count : 0;
    }

    private void ActualizarSeleccion()
    {
        foreach (var boton in botones)
        {
            var colors = boton.colors;
            colors.normalColor = colors.disabledColor;
            boton.colors = colors;
        }

        var selectedColors = botones[botonSeleccionado].colors;
        selectedColors.normalColor = Color.yellow;
        botones[botonSeleccionado].colors = selectedColors;
    }
}