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
      private Vector3 shoulderPos, elbowPos;
    private Vector3 upperArm, forearm, torsoForward;
    private float elbowAngle;
    private Quaternion pelvisRotation, inversePelvis;
    private float smoothingFactor = 0.8f; // Ajusta este valor entre 0 (sin suavizado) y 1 (suavizado completo)
    private readonly object dataLock = new object();

    public bool esZurdo = false;

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
                DepthMode = DepthMode.NFOV_2x2Binned,
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
                    var shoulder = System.Numerics.Vector3.Zero;
                    var elbow = System.Numerics.Vector3.Zero;
                    var wrist = System.Numerics.Vector3.Zero;
    
                    if(esZurdo){
                        shoulder = body.Skeleton.GetJoint(JointId.ShoulderLeft).Position;
                        elbow = body.Skeleton.GetJoint(JointId.ElbowLeft).Position;
                        wrist = body.Skeleton.GetJoint(JointId.WristLeft).Position;
                       
                    }
                    else{
                        shoulder = body.Skeleton.GetJoint(JointId.ShoulderRight).Position;
                        elbow = body.Skeleton.GetJoint(JointId.ElbowRight).Position;
                        wrist = body.Skeleton.GetJoint(JointId.WristRight).Position;
                    }

                    var pelvis = body.Skeleton.GetJoint(JointId.Pelvis);
                    var chest = body.Skeleton.GetJoint(JointId.SpineChest);

                    lock (dataLock)
                    {
                        latestShoulder = new Vector3(-shoulder.X, -shoulder.Y, -shoulder.Z) / 1000f;
                        latestElbow = new Vector3(-elbow.X, -elbow.Y, -elbow.Z) / 1000f;
                        latestWrist = new Vector3(-wrist.X, -wrist.Y, -wrist.Z) / 1000f;
                        latestPelvis = new Vector3(-pelvis.Position.X, -pelvis.Position.Y, pelvis.Position.Z) / 1000f;
                        latestChest = new Vector3(chest.Position.X, -chest.Position.Y, -chest.Position.Z) / 1000f;
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
    
        Vector3 shoulderPos, elbowPos, wristPosLocal;
    
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

              Transform upperArmBone = animator.GetBoneTransform(esZurdo ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
            Transform lowerArmBone = animator.GetBoneTransform(esZurdo ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
            if (upperArmBone && lowerArmBone)
            {
            Quaternion upperArmRotation = Quaternion.LookRotation(upperArm);
            Quaternion axisCorrectionUpperArm = Quaternion.Euler(90, 0, 0);
            upperArmBone.rotation = upperArmRotation * axisCorrectionUpperArm;

            Quaternion lowerArmRotation = Quaternion.LookRotation(forearm);
            Quaternion axisCorrectionLowerArm = Quaternion.Euler(90, 0, 0);
            lowerArmBone.rotation = lowerArmRotation * axisCorrectionLowerArm;
                }
            }
            
            private Vector3 SmoothPosition(Vector3 previous, Vector3 current)
            {
                return Vector3.Lerp(previous, current, 1f - smoothingFactor);
            }


public (Vector3 hombro, Vector3 codo, Vector3 muñeca) ObtenerPosicionesBrazo()
{
    if (kinectDevice == null || bodyTracker == null)
    {
        Debug.LogError("Kinect device or body tracker is null.");
        return (Vector3.zero, Vector3.zero, Vector3.zero);
    }

    try
    {
        using (var capture = kinectDevice.GetCapture())
        {
            bodyTracker.EnqueueCapture(capture);
        }

        using (var frame = bodyTracker.PopResult(TimeSpan.FromMilliseconds(100)))
        {
            if (frame == null || frame.NumberOfBodies == 0)
            {
                Debug.LogWarning("No bodies detected.");
                return (Vector3.zero, Vector3.zero, Vector3.zero);
            }

            var body = frame.GetBody(0);
            System.Numerics.Vector3 hombro, codo, muñeca;
            if(esZurdo){
                hombro = body.Skeleton.GetJoint(JointId.ShoulderLeft).Position;
                codo = body.Skeleton.GetJoint(JointId.ElbowLeft).Position;
                muñeca = body.Skeleton.GetJoint(JointId.WristLeft).Position;
            }
            else{
                hombro = body.Skeleton.GetJoint(JointId.ShoulderRight).Position;
                codo = body.Skeleton.GetJoint(JointId.ElbowRight).Position;
                muñeca = body.Skeleton.GetJoint(JointId.WristRight).Position;
            }

        
          

            return (
                new Vector3(-hombro.X, -hombro.Y, -hombro.Z) / 1000f,
                new Vector3(-codo.X, -codo.Y, -codo.Z) / 1000f,
                new Vector3(-muñeca.X, -muñeca.Y, -muñeca.Z) / 1000f
            );
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error in ObtenerPosicionesBrazo: {ex.Message}");
        return (Vector3.zero, Vector3.zero, Vector3.zero);
    }
}


public (Vector3 pecho, Vector3 muñeca) ObtenerDireccionBrazoExtendido()
{
    lock (dataLock)
    {
        return (latestChest, latestWrist);
    }
}

public (Vector3 hombro, Vector3 codo, Vector3 muñeca) ObtenerUltimasPosicionesBrazo()
{
    lock (dataLock)
    {
        return (latestShoulder, latestElbow, latestWrist);
    }
}

    

    void OnDestroy()
    {
        isRunning = false;
        kinectThread?.Join();      
        bodyTracker?.Dispose();
        kinectDevice?.Dispose();
    }
}