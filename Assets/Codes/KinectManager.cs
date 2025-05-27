using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Threading;
using System.Collections.Generic;

public class KinectManager : MonoBehaviour
{
    public static KinectManager Instance { get; private set; }

    public Device kinectDevice { get; private set; }
    public Tracker bodyTracker { get; private set; }
    public Calibration calibration { get; private set; }
    public bool InicializadoCorrectamente { get; private set; } = false;

    public Animator animator;
    public Vector3 wristPos;

    private Thread kinectThread;
    private bool isRunning = false;

    private Vector3 latestShoulder, latestElbow, latestWrist, latestPelvis, latestChest; 
    private Vector3 smoothedShoulder, smoothedElbow, smoothedWrist, smoothedPelvis, smoothedChest;
      private Vector3 shoulderPos, elbowPos, pelvisPos, chestPos;
    private Vector3 upperArm, forearm, torsoForward;
    private float elbowAngle;
    private Quaternion pelvisRotation, inversePelvis;
    private float smoothingFactor = 0.8f; // Ajusta este valor entre 0 (sin suavizado) y 1 (suavizado completo)
    private readonly object dataLock = new object();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            InicializarKinect();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void InicializarKinect()
    {
        try
        {
            kinectDevice = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorResolution = ColorResolution.Off,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = false,
                CameraFPS = FPS.FPS30
            };

            kinectDevice.StartCameras(config);

            calibration = kinectDevice.GetCalibration();

            bodyTracker = Tracker.Create(calibration, new TrackerConfiguration
            {
                ProcessingMode = TrackerProcessingMode.Gpu,
                SensorOrientation = SensorOrientation.Default
            });

            InicializadoCorrectamente = true;
            isRunning = true;

            kinectThread = new Thread(TrackBodyLoop);
            kinectThread.Start();

            Debug.Log("Kinect inicializada correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error inicializando Kinect: {ex.Message}");
        }
    }

 private void TrackBodyLoop()
    {
        while (isRunning)
        {
            try
            {
                using (var capture = kinectDevice.GetCapture())
                {
                    bodyTracker.EnqueueCapture(capture);
                }

                using (var frame = bodyTracker.PopResult(TimeSpan.FromMilliseconds(100)))
                {
                    if (frame == null || frame.NumberOfBodies == 0)
                        continue;

                    var body = frame.GetBody(0);
                    var shoulder = body.Skeleton.GetJoint(JointId.ShoulderRight).Position;
                    var elbow = body.Skeleton.GetJoint(JointId.ElbowRight).Position;
                    var wrist = body.Skeleton.GetJoint(JointId.WristRight).Position;
                    var pelvis = body.Skeleton.GetJoint(JointId.Pelvis);
                    var chest = body.Skeleton.GetJoint(JointId.SpineChest);

                    lock (dataLock)
                    {
                        latestShoulder = new Vector3(-shoulder.X, -shoulder.Y, -shoulder.Z) / 1000f;
                        latestElbow = new Vector3(-elbow.X, -elbow.Y, -elbow.Z) / 1000f;
                        latestWrist = new Vector3(-wrist.X, -wrist.Y, -wrist.Z) / 1000f;
                        latestPelvis = new Vector3(-pelvis.Position.X, -pelvis.Position.Y, pelvis.Position.Z) / 1000f;
                        latestChest = new Vector3(-chest.Position.X, -chest.Position.Y, chest.Position.Z) / 1000f;
                    }
                }

                Thread.Sleep(33); // ~30 FPS
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error en TrackBodyLoop: {ex.Message}");
            }
        }
    }

    void Update()
    {
        if (!InicializadoCorrectamente || animator == null) return;
    
        Vector3 shoulderPos, elbowPos, wristPosLocal, pelvisPos, chestPos;
    
        lock (dataLock)
        {
            shoulderPos = latestShoulder;
            elbowPos = latestElbow;
            wristPosLocal = latestWrist;
        }
    
        this.wristPos = latestWrist;
    
        Vector3 upperArm = (elbowPos - shoulderPos).normalized;
        Vector3 forearm = (wristPosLocal - elbowPos).normalized;
        float elbowAngle = Vector3.Angle(upperArm, forearm);
    
             // Actualizar animaciones del brazo
        animator.SetFloat("ElbowFlex", Mathf.InverseLerp(180f, 45f, elbowAngle));
        animator.SetFloat("ArmDirX", upperArm.x);
        animator.SetFloat("ArmDirZ", upperArm.z);

        // Obtener huesos del brazo
        Transform upperArmBone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Transform lowerArmBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        if (upperArmBone && lowerArmBone)
        {
              // Calcular rotación del brazo superior
    Quaternion upperArmRotation = Quaternion.LookRotation(upperArm);
    Quaternion axisCorrectionUpperArm = Quaternion.Euler(90, 0, 0); // Ajusta según la orientación del modelo
    upperArmBone.rotation = upperArmRotation * axisCorrectionUpperArm;

    // Calcular rotación del antebrazo
    Quaternion lowerArmRotation = Quaternion.LookRotation(forearm);
    Quaternion axisCorrectionLowerArm = Quaternion.Euler(90, 0, 0); // Ajusta según la orientación del modelo
    lowerArmBone.rotation = lowerArmRotation * axisCorrectionLowerArm;
        }
    }
    
    private Vector3 SmoothPosition(Vector3 previous, Vector3 current)
    {
        return Vector3.Lerp(previous, current, 1f - smoothingFactor);
    }

    void OnDestroy()
    {
        isRunning = false;
        kinectThread?.Join();      
        bodyTracker?.Dispose();
        kinectDevice?.Dispose();
    }
}