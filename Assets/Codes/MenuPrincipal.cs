using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour
{

    private GestorUI gestorUI;
    public Wiimote wiimote2;
    public Canvas canvas;


    void Start()
    {
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

            if (!wiimote.Button.d_up && !wiimote.Button.d_down)
            {
                gestorUI.LiberarBoton();
            }

            if (wiimote.Button.a)
            {
                gestorUI.SeleccionarBoton();
            }
        }
        else
        {
            // Controles alternativos para teclado/rato
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                gestorUI.MoverMenu(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                gestorUI.MoverMenu(1);
            }

            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && 
                !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                gestorUI.LiberarBoton();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                gestorUI.SeleccionarBoton();
            }
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
                if (GestorWiimotes.Instance?.wiimote2 != null)
                {
                    IrAlModoMultijugador();
                }
                else
                {
                    Debug.LogWarning("No se detectó el segundo mando Wii.");
                }
                break;
            case 2:
                IrAConfiguracion();
                break;
            case 3:
                SalirDelJuego();
                break;
        }
    }

    public void IrAlModoMultijugador()
    {
        SceneManager.LoadScene("PetancaMultijugador");
    }

    public void IrAlModoPractica()
    {
        SceneManager.LoadScene("PetancaSolitario");
    }

    public void IrAConfiguracion()
    {
        SceneManager.LoadScene("MenuConfiguracion");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
    
}

