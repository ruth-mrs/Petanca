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

        switch (botonSeleccionado)
        {
            case 0:
                IrAlModoPractica();
                break;
            case 1:
                if(GestorWiimotes.Instance?.wiimote2 != null)
                {
                    IrAlModoMultijugador();
                }
                else
                {
                    Debug.LogWarning("No se detectó el segundo mando Wii.");
                }
                break;
            case 2:
                SalirDelJuego();
                break;
        }
    }

    public void IrAlModoMultijugador(){
        SceneManager.LoadScene("PetancaMultijugador");
    }

    public void IrAlModoPractica(){
        SceneManager.LoadScene("PetancaSolitario");
    }

    public void SalirDelJuego(){
        Application.Quit();
    }
}

