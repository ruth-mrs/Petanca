using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GestorUI : MonoBehaviour
{
    private List<Button> botones; // Lista de botones detectados en el Canvas
    private int botonSeleccionado = 0; // Índice del botón actualmente seleccionado

    public Action<int> OnBotonSeleccionado; // Evento para ejecutar la acción del botón seleccionado
    private bool botonProcesado = false;



    public void Inicializar(Canvas canvas)
    {
        botones = new List<Button>(canvas.GetComponentsInChildren<Button>());

        if (botones.Count == 0)
        {
            Debug.LogWarning("No se encontraron botones en el Canvas.");
            return;
        }

        ActualizarSeleccion();
    }

    public void MoverMenu(int movimiento)
    {
        if (botones.Count == 0 || botonProcesado) return;


        botonSeleccionado += movimiento;

        if (botonSeleccionado < 0) botonSeleccionado = botones.Count - 1;
        if (botonSeleccionado >= botones.Count) botonSeleccionado = 0;

        ActualizarSeleccion();

        botonProcesado = true;

    }

    public void SeleccionarBoton()
    {
        if (botones.Count == 0) return;

        botones[botonSeleccionado].onClick.Invoke();

        OnBotonSeleccionado?.Invoke(botonSeleccionado);

    }

    public void LiberarBoton()
    {
        botonProcesado = false;
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