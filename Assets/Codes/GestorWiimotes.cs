using UnityEngine;
using WiimoteApi;

public class GestorWiimotes : MonoBehaviour
{
    public static GestorWiimotes Instance;
    public Wiimote wiimote; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        WiimoteManager.FindWiimotes();
        if (WiimoteManager.HasWiimote())
        {
            wiimote = WiimoteManager.Wiimotes[0];
            wiimote.SendPlayerLED(true, false, false, false);
            wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            wiimote.Accel.CalibrateAccel(AccelCalibrationStep.LEFT_SIDE_UP);
            Debug.Log("Wiimote conectado.");
        }
        else
        {
            Debug.LogWarning("No se detectó ningún mando Wii.");
        }
    }

    void Update()
    {
        if (wiimote != null)
        {
            int ret;
            do
            {
                ret = wiimote.ReadWiimoteData();
            } while (ret > 0);
        }
    }

}