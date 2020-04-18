/**  
* Copyright 2020 Matti 'Menithal' Lahtinen
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
**/
using System;
using System.Linq;
using UnityEngine;
using LIV.Avatar;

// Keeping out of Namespace since otherwise Serialization wasnt succesfull (probably something
// to do with the Reflection done on the other end?)
public class ActionCameraSettings : IPluginSettings
{
    // TODO: SEparate settings to serializable classes intead.
    // How many seconds until swapping another camera

#if SIMPLIFIED
    public float cameraSwapTimeLock = 3f;
#else
    public float cameraSwapTimeLock = 8f;
#endif

    // actionCamera can swap faster than general Camera and people tend to change look directions quite quickly.
    public float cameraPositionTimeLock = 0.2f;

    // How fast should we swap between 
    public bool reverseShoulder = false;

    public float controlMovementThreshold = 2f; // Meters per framea
    public float controlVerticalMovementThreshold = 2f; // Meters per framea
    // Users tend to have headset a bit higher, so when they are looking down sights they are not 
    // fully looking up. This offsets that
    public float forwardVerticalOffset = 0f;
    public float forwardHorizontalOffset = 5f;
    public float forwardDistance = 10f;

#if SIMPLIFIED
    // No Full Body stuff
    public bool disableFBTCamera { get; private set; }  = true ;
    public bool disableTopCamera { get; private set; } = true;
    public bool disableFPSCamera { get; private set; } = false;
    public bool disableGunCamera { get; private set; } = false;
    public bool FPSCameraOverride { get; private set; } = false;
#else
    
    public bool reverseFBT = false;
    public bool disableFBTCamera = false;
    public bool disableTopCamera = true;
    public bool disableFPSCamera = false;
    public bool disableGunCamera = false;
    public bool FPSCameraOverride = false;
#endif

    public bool removeAvatarInsteadOfHead = true;
    public bool inBetweenCameraEnabled = true;
    public float cameraDefaultFov = 80f;

    public bool cameraVerticalLock = true;
    public float cameraShoulderPositioningTime = 2f;
    public float cameraShoulderDistance = 1.8f;

#if SIMPLIFIED
    public float cameraShoulderAngle = 45;
#else
    public float cameraShoulderAngle = 20;
#endif

    public float cameraShoulderSensitivity = 2f;
    public float cameraBodyVerticalTargetOffset = 0.5f;

#if !SIMPLIFIED
    public float cameraBodyPositioningTime = 1.8f;
    public float cameraBodyLookAtForward = 0.1f;
    public float cameraBodyDistance = 1.4f;
    public float cameraBodyAngle = 55;
    public float cameraBodySensitivity = 2f;
#endif
    // TODO: if Enabled, average head forward with Hand Forwards (dominance)
    public bool averageHandsWithHead = false;
    public bool useDominantHand = false;
    public bool rightHandDominant = true;
    // Uses right hand info to determine additional pointt

    public float cameraGunFov = 80f;
    public bool cameraFovLerp = false;

    public float cameraGunHeadAlignAngleTrigger = 12f;
    public float cameraGunHeadDistanceTrigger = 0.3f;
    public float cameraGunEyeVerticalOffset = 0.15f;
    public float cameraGunMaxTwoHandedDistance = 0.6f;
    public float cameraGunMinTwoHandedDistance = 0.15f;
    public float cameraGunSmoothing = 0.2f;
}

#if !SIMPLIFIED
namespace MACPlugin
#else
namespace SimpleMacPlugin
#endif
{

#if !SIMPLIFIED
    public class ActionCameraPlugin : IPluginCameraBehaviour
#else
    public class SimplifiedActionCameraPlugin : IPluginCameraBehaviour
#endif
    {
        public IPluginSettings settings => _settings;
        // Matching naming schema
        ActionCameraSettings _settings = new ActionCameraSettings();

#if SIMPLIFIED
        public string name => "Menithal' Simplified Action Camera";
#else
#if DEBUG
        public string name => "Menithal' Action Camera DEV BUILD";
#else
        public string name => "Menithal' Action Camera";
#endif
#endif
#if SIMPLIFIED
        public string ID => "SimplifiedActionCamera";
#else
        public string ID => "ActionCamera";
#endif
        public string author => "MA 'Menithal' Lahtinen";
        public string version => "0.9.3a";

        public event EventHandler ApplySettings;
        private ActionCameraDirector cameraDirector;
        private TimerHelper timerHelper;
        private AvatarReferenceSignal avatarRefSignal;

        // Called when the Plugin is selected
        public void OnActivate(PluginCameraHelper helper)
        {
            PluginLog.Log(ID, "OnActivate");
            timerHelper = new TimerHelper();
            cameraDirector = new ActionCameraDirector(_settings, helper, ref timerHelper);

            AvatarManager avatarManager = Resources.FindObjectsOfTypeAll<AvatarManager>().FirstOrDefault();
            avatarRefSignal = avatarManager?.GetPrivateField<AvatarReferenceSignal>("_avatarInstantiated");
            avatarRefSignal?.OnChanged.AddListener(OnAvatarChanged);

            OnAvatarChanged(avatarRefSignal?.Value);
        }

        // called when the Plugin is deselected OR when Liv is being closed down.
        public void OnDeactivate()
        {

            PluginLog.Log(ID, "OnDeactivate ");

            ApplySettings?.Invoke(this, EventArgs.Empty);

            avatarRefSignal?.OnChanged.RemoveListener(OnAvatarChanged);
            avatarRefSignal = null;
        }

        // Called when Plugin component is destroyed.
        public void OnDestroy()
        {
            PluginLog.Log(ID, "OnDestroy");
        }

        //Equal to Unity FixedUpdate
        public void OnFixedUpdate()
        {
        }

        //Equal to Unity LateUpdate
        public void OnLateUpdate()
        {
        }

        // Called when Settings are deserialized (read) from file.
        public void OnSettingsDeserialized()
        {
            PluginLog.Log(ID, "OnSettingsDeserialized");
            PluginLog.Log(ID, "generalCameraSwapClamp " + _settings.cameraSwapTimeLock);
            PluginLog.Log(ID, "actionCameraSwapClamp " + _settings.cameraPositionTimeLock);
            PluginLog.Log(ID, "controlMovementThreshold " + _settings.controlMovementThreshold);
            PluginLog.Log(ID, "controlVerticalMovementThreshold " + _settings.controlVerticalMovementThreshold);

            PluginLog.Log(ID, "cameraVerticalLock " + _settings.cameraVerticalLock);
            PluginLog.Log(ID, "cameraShoulderDistance " + _settings.cameraShoulderDistance);
            PluginLog.Log(ID, "cameraShoulderAngle " + _settings.cameraShoulderAngle);
            PluginLog.Log(ID, "cameraShoulderPositioningTime " + _settings.cameraShoulderPositioningTime);

#if !SIMPLIFIED
            PluginLog.Log(ID, "cameraBodyVerticalTargetOffset " + _settings.cameraBodyPositioningTime);
            PluginLog.Log(ID, "cameraBodyPositioningTime " + _settings.cameraBodyPositioningTime);
            PluginLog.Log(ID, "cameraBodyAngle " + _settings.cameraBodyAngle);
            PluginLog.Log(ID, "cameraBodyDistance " + _settings.cameraBodyDistance);
            PluginLog.Log(ID, "cameraBodyLookAtForward " + _settings.cameraBodyLookAtForward);
            PluginLog.Log(ID, "reverseFBT " + _settings.reverseFBT);

#endif
            PluginLog.Log(ID, "reverseShoulder " + _settings.reverseShoulder);
            PluginLog.Log(ID, "forwardVerticalOffset " + _settings.forwardVerticalOffset);
            PluginLog.Log(ID, "forwardHorizontalOffset " + _settings.forwardHorizontalOffset);
            PluginLog.Log(ID, "forwardDistance " + _settings.forwardDistance);
            PluginLog.Log(ID, "removeAvatarInsteadOfHead " + _settings.removeAvatarInsteadOfHead);

#if !SIMPLIFIED
            PluginLog.Log(ID, "disableTopCamera " + _settings.disableTopCamera);
            PluginLog.Log(ID, "disableFBTCamera " + _settings.disableFBTCamera);
            PluginLog.Log(ID, "disableFPSCamera " + _settings.disableFPSCamera);
            PluginLog.Log(ID, "disableGunCamera " + _settings.disableGunCamera);
            
#endif
            PluginLog.Log(ID, "inBetweenCameraEnabled " + _settings.inBetweenCameraEnabled);
            PluginLog.Log(ID, "averageHandsWithHead " + _settings.averageHandsWithHead);
            PluginLog.Log(ID, "useDominantHand " + _settings.useDominantHand);
            PluginLog.Log(ID, "rightHandDominant " + _settings.rightHandDominant);
            // Need to make sure everything else is updated

      
            PluginLog.Log(ID, "cameraGunFov " + _settings.cameraGunFov);
            PluginLog.Log(ID, "cameraGunHeadAlignAngleTrigger " + _settings.cameraGunHeadAlignAngleTrigger);
            PluginLog.Log(ID, "cameraGunHeadDistanceTrigger " + _settings.cameraGunHeadDistanceTrigger);
            PluginLog.Log(ID, "cameraGunEyeVerticalOffset " + _settings.cameraGunEyeVerticalOffset);
            PluginLog.Log(ID, "cameraGunMaxTwoHandedDistance " + _settings.cameraGunMaxTwoHandedDistance);
            PluginLog.Log(ID, "cameraGunMinTwoHandedDistance " + _settings.cameraGunMinTwoHandedDistance);
            PluginLog.Log(ID, "cameraGunSmoothing " + _settings.cameraGunSmoothing);

            cameraDirector.SetSettings(_settings);
        }

        public void OnAvatarChanged(Avatar avatar)
        {
            PluginLog.Log(ID, "OnAvatarChanged ");
            cameraDirector.SetAvatar(avatar);

        }

        // Called Every Frame.
        public void OnUpdate()
        {
            timerHelper.AddTime(Time.deltaTime);
            cameraDirector.SelectCamera();
            cameraDirector.HandleCameraView();
        }
    }

}
