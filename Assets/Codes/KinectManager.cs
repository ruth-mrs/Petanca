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
    private readonly object dataLock = new object();

    void Awake()
    {
        Instance = this;
        InicializarKinect();
    }

    public void InicializarKinect()
    {
        try
        {
            kinectDevice = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorMJPG,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS30
            };

            kinectDevice.StartCameras(config);

            calibration = kinectDevice.GetCalibration();

            bodyTracker = Tracker.Create(calibration, new TrackerConfiguration
            {
                ProcessingMode = TrackerProcessingMode.Cpu,
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

    void TrackBodyLoop()
    {
        while (isRunning)
        {
            try
            {
                using (Capture capture = kinectDevice.GetCapture())
                {
                    bodyTracker.EnqueueCapture(capture);
                }

                using (var frame = bodyTracker.PopResult(TimeSpan.FromMilliseconds(100)))
                {
                    if (frame == null || frame.NumberOfBodies == 0)
                        continue;

                    var body = frame.GetBody(0);

                    lock (dataLock)
                    {
                        latestShoulder = new Vector3(-body.Skeleton.GetJoint(JointId.ShoulderRight).Position.X,
                                                     -body.Skeleton.GetJoint(JointId.ShoulderRight).Position.Y,
                                                     -body.Skeleton.GetJoint(JointId.ShoulderRight).Position.Z) / 1000f;

                        latestElbow = new Vector3(-body.Skeleton.GetJoint(JointId.ElbowRight).Position.X,
                                                  -body.Skeleton.GetJoint(JointId.ElbowRight).Position.Y,
                                                  -body.Skeleton.GetJoint(JointId.ElbowRight).Position.Z) / 1000f;

                        latestWrist = new Vector3(body.Skeleton.GetJoint(JointId.WristRight).Position.X,
                                                  body.Skeleton.GetJoint(JointId.WristRight).Position.Y,
                                                  body.Skeleton.GetJoint(JointId.WristRight).Position.Z) / 1000f;

                        latestPelvis = new Vector3(-body.Skeleton.GetJoint(JointId.Pelvis).Position.X,
                                                   -body.Skeleton.GetJoint(JointId.Pelvis).Position.Y,
                                                    body.Skeleton.GetJoint(JointId.Pelvis).Position.Z) / 1000f;

                        latestChest = new Vector3(-body.Skeleton.GetJoint(JointId.SpineChest).Position.X,
                                                  -body.Skeleton.GetJoint(JointId.SpineChest).Position.Y,
                                                   body.Skeleton.GetJoint(JointId.SpineChest).Position.Z) / 1000f;
                    }

                    Thread.Sleep(33); // ~30 FPS
                }
            }
            catch { /* Silencio controlado */ }
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
            pelvisPos = latestPelvis;
            chestPos = latestChest;
        }

        this.wristPos = latestWrist;

        Vector3 upperArm = (elbowPos - shoulderPos).normalized;
        Vector3 forearm = (wristPosLocal - elbowPos).normalized;
        Vector3 torsoForward = (chestPos - pelvisPos).normalized;
        float elbowAngle = Vector3.Angle(upperArm, forearm);
        Vector3 localUpperArm = upperArm;

        animator.SetFloat("ElbowFlex", Mathf.InverseLerp(180f, 45f, elbowAngle));
        animator.SetFloat("ArmDirX", localUpperArm.x);
        animator.SetFloat("ArmDirZ", localUpperArm.z);

        Transform upperArmBone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Transform lowerArmBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        if (upperArmBone && lowerArmBone)
        {
            Quaternion upperArmRotation = Quaternion.LookRotation(upperArm);
            Quaternion lowerArmRotation = Quaternion.LookRotation(forearm);
            Quaternion axisCorrection = Quaternion.Euler(90, 0, 0);

            upperArmBone.rotation = upperArmRotation * axisCorrection;
            lowerArmBone.rotation = lowerArmRotation * axisCorrection;
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