using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour
{

    public Wiimote wiimote2;
    public Canvas canvas;


    void Start()
    {
        Application.targetFrameRate = 30;


        if(GestorUI.Instance == null)
        {
            GameObject go = new GameObject("GestorUI");
            go.AddComponent<GestorUI>();

            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }else{
            GestorUI.Instance.Inicializar(canvas);
            GestorUI.Instance.OnBotonSeleccionado += EjecutarOpcionSeleccionada;
        }


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
                GestorUI.Instance.MoverMenu(-1);
            }
            else if (wiimote.Button.d_down)
            {
                GestorUI.Instance.MoverMenu(1);
            }
            if (wiimote.Button.a)
            {
                GestorUI.Instance.SeleccionarBoton();
            }
            }

            if (!wiimote.Button.d_up && !wiimote.Button.d_down && !wiimote.Button.a)
            {
                GestorUI.Instance.LiberarBoton();
            }

        
    }


    void EjecutarOpcionSeleccionada(int botonSeleccionado)
    {

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


    private void OnDestroy()
    {   
    if (GestorUI.Instance != null)
    {
        GestorUI.Instance.OnBotonSeleccionado -= EjecutarOpcionSeleccionada;
    }
    }
}

