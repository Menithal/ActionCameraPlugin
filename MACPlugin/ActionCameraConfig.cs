using System;
using System.Reflection;

using MACPlugin.Utility;
using System.Diagnostics;

namespace MACPlugin
{
    public class ActionCameraConfig
    {
        public bool ready = false; // Used to just make sure if the file is ready or not.

        [SerializableFloatConfig(8, 0.1f, 30)]
        public float cameraSwapTimeLock = 8;
        [SerializableFloatConfig(0.8f, 0.1f, 30)]
        public float cameraPositionTimeLock = 0.8f;

        [SerializableBooleanConfig(false)]
        public bool reverseFBT = false;
        [SerializableBooleanConfig(false)]
        public bool reverseShoulder = false;

        [SerializableFloatConfig(0.1f, 5)]
        public float controlMovementThreshold = 2; // Meters per framea

        [SerializableFloatConfig(0.1f, 5)]
        public float controlVerticalMovementThreshold = 2; // Meters per framea
                                                           // Users tend to have headset a bit higher, so when they are looking down sights they are not 
        //[SerializableFloatConfig(0, 5)]
        public float forwardVerticalOffset = 0;
        //[SerializableFloatConfig(5, 5)]
        public float forwardHorizontalOffset = 5;
        //  [SerializableFloatConfig(15)] 
        public float forwardDistance = 10; // Maybe this value should not be adjustable...

        [SerializableBooleanConfig(true)]
        public bool removeAvatarInsteadOfHead = true;
        [SerializableBooleanConfig(true)]
        public bool disableTopCamera = true;
        [SerializableBooleanConfig(false)]
        public bool disableFBTCamera = false;
        [SerializableBooleanConfig(false)]
        public bool disableFPSCamera = false;
        [SerializableBooleanConfig(false)]
        public bool disableGunCamera = false;
        //[SerializableFloatConfig(80f, 45f, 120f)]
        public float cameraDefaultFov = 80f;
        [SerializableBooleanConfig(false)]
        public bool FPSCameraOverride = false;

        [SerializableBooleanConfig(true)]
        public bool cameraVerticalLock = true;
        [SerializableFloatConfig(2f, 0.1f, 10f)]
        public float cameraShoulderPositioningTime = 2f;
        [SerializableFloatConfig(1.8f, 0.1f, 10f)]
        public float cameraShoulderDistance = 1.8f;
        [SerializableFloatConfig(20f, 0f, 75f)]
        public float cameraShoulderAngle = 20;
        [SerializableFloatConfig(2f, 0.1f, 10f)]
        public float cameraShoulderSensitivity = 2f;

        [SerializableFloatConfig(0.5f, 2f)]
        public float cameraBodyVerticalTargetOffset = 0.5f;
        [SerializableFloatConfig(1.8f, 0.1f, 10f)]
        public float cameraBodyPositioningTime = 1.8f;
        [SerializableFloatConfig(0.1f, 0.1f, 10f)]
        public float cameraBodyLookAtForward = 0.1f;
        [SerializableFloatConfig(1.4f, 0.5f, 5f)]
        public float cameraBodyDistance = 1.4f;
        [SerializableFloatConfig(55f, 0f, 75f)]
        public float cameraBodyAngle = 55;
        [SerializableFloatConfig(2f, 0.1f, 10f)]
        public float cameraBodySensitivity = 2f;

        [SerializableBooleanConfig(false)]
        public bool averageHandsWithHead = false;
        [SerializableBooleanConfig(false)]
        public bool useDominantHand = false;
        [SerializableBooleanConfig(true)]
        public bool rightHandDominant = true;

        //[SerializableFloatConfig(80f, 45f, 120f)]
        public float cameraGunFov = 80f;
        [SerializableBooleanConfig(false)]
        public bool cameraFovLerp = false;

        [SerializableFloatConfig(15f, 0f, 75f)]
        public float cameraGunHeadAlignAngleTrigger = 15;
        [SerializableFloatConfig(0.3f, 1f)]
        public float cameraGunHeadDistanceTrigger = 0.3f;
        [SerializableFloatConfig(0.15f, 1f)]
        public float cameraGunEyeVerticalOffset = 0.15f;
        [SerializableFloatConfig(0.6f, 1f)]
        public float cameraGunMaxTwoHandedDistance = 0.6f;
        [SerializableFloatConfig(0.15f, 1f)]
        public float cameraGunMinTwoHandedDistance = 0.15f;
        [SerializableFloatConfig(0.2f, 0, 1f)]
        public float cameraGunSmoothing = 0.2f;

        [SerializableBooleanConfig(true)]
        public bool alwaysHaveAvatarInFrame = true;

        [SerializableBooleanConfig(false)]
        public bool cameraUseWaistAsHeight = false;

        [SerializableBooleanConfig(false)]
        public bool cameraBodyUseRoomOriginCenter = false;
        [SerializableBooleanConfig(false)]
        public bool cameraShoulderUseRoomOriginCenter = false;
        [SerializableBooleanConfig(true)]
        public bool cameraShoulderFollowGaze = true;
        [SerializableBooleanConfig(true)]
        public bool cameraBodyFollowGaze = true;

        [SerializableFloatConfig(0.1f, 0, 0.5f)]
        public float minimumCameraDistance = 0.5f;


        [SerializableBooleanConfig(true)]
        public bool useEyePosition = true;
        [SerializableBooleanConfig(true)]
        public bool rightEyeDominant = true;
        [SerializableBooleanConfig(false)]
        public bool linearCameraMovement = false;


        public bool useDanceGestures = false;
        public void PrintContents()
        {
#if DEBUG
            Type type = typeof(ActionCameraConfig);
            FieldInfo[] properties = type.GetFields();
            
            foreach (FieldInfo property in properties)
            {
                PluginLog.Log("ActionCameraConfig", property.Name + " = " + property.GetValue(this));
            }
#endif
        }
    }
}
