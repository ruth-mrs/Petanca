using UnityEngine;
using UnityEngine.SceneManagement;

public class ModoPractica : MonoBehaviour
{
    void OnGUI()
    {
        // Crear un menú simple
        GUI.Box(new Rect(10, 10, 200, 150), "Menú Principal");

        // Botón para iniciar una partida en solitario
        if (GUI.Button(new Rect(20, 40, 180, 30), "Partida en Solitario"))
        {
            CrearPartidaSolitario();
        }

        // Botón para salir del juego
        if (GUI.Button(new Rect(20, 80, 180, 30), "Salir"))
        {
            SalirDelJuego();
        }
    }

    void CrearPartidaSolitario()
    {
        // Cargar la escena de la partida de petanca
        SceneManager.LoadScene("PetancaSolitario");
    }

    void SalirDelJuego()
    {
        // Salir del juego
        Application.Quit();
    }
}