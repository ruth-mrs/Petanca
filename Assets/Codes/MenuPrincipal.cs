using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour
{

    private GestorUI gestorUI;
    public Wiimote wiimote2;
    public Canvas canvas;


    void Start(){
        gestorUI = gameObject.GetComponent<GestorUI>();

        gestorUI.Inicializar(canvas);
        gestorUI.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
    }
    void Update()
    {
         Wiimote wiimote = GestorWiimotes.Instance?.wiimote;

        if (wiimote != null)
        {
            int ret;
            do
            {
                ret = wiimote.ReadWiimoteData();
            } while (ret > 0);

            if (wiimote.Button.d_up)
            {
                gestorUI.MoverMenu(-1);
              
            }
            else if (wiimote.Button.d_down)
            {
                gestorUI.MoverMenu(1);
            }
        }

        if (!wiimote.Button.d_up && !wiimote.Button.d_down)
        {
                gestorUI.LiberarBoton(); // Liberar el estado de "botón presionado"
        }


        if (wiimote.Button.a)
        {
            gestorUI.SeleccionarBoton();
        }

    
    }


 void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {
        Debug.Log("Botón ejecutado: " + botonSeleccionado);

        // Lógica para cada botón
        switch (botonSeleccionado)
        {
            case 0:
                Debug.Log("Opción 1 seleccionada: Jugar");
                IrAlModoPractica();
              
                break;
            case 1:

                Debug.Log("Opción 3 seleccionada: Salir");
                Application.Quit();
                break;
        }
    }




    public void IrAlModoPractica(){
        SceneManager.LoadScene("PetancaSolitario");
    }

    public void SalirDelJuego(){
        Application.Quit();
    }
}

