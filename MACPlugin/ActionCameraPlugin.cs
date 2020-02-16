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
    // How many seconds until swapping another camera
    public float cameraSwapTimeLock = 8;
    // actionCamera can swap faster than general Camera and people tend to change look directions quite quickly.
    public float cameraPositionTimeLock= 0.8f;

    // How fast should we swap between 
    public bool reverseFBT = false;
    public bool reverseShoulder = false;
    public float controlMovementThreshold = 4; // Meters per framea
    // Users tend to have headset a bit higher, so when they are looking down sights they are not 
    // fully looking up. This offsets that
    public float forwardVerticalOffset = 0;
    public float forwardHorizontalOffset = 5;
    public float forwardDistance = 10;

    public bool removeAvatarInsteadOfHead = true;
    public bool disableTopCamera = true;
    public bool disableFBTCamera = false;
    public bool disableFPSCamera = true;
    public bool inBetweenCameraEnabled = true;

    public bool cameraVerticalLock = false;
    public float cameraShoulderPositioningTime = 0.9f;
    public float cameraShoulderDistance = 1.4f;
    public float cameraShoulderAngle = 25;

    public float cameraBodyPositioningTime = 2f;
    public float cameraBodyLookAtForward = 0.1f;
    public float cameraBodyDistance = 1.4f;
    public float cameraBodyAngle = 45;

    // TODO: if Enabled, average head forward with Hand Forwards (dominance)
    public bool averageHandsWithHead = false;
    public bool useDominantHand = false;
    public bool rightHandDominant = true;
    // Uses right hand info to determine additional pointt
}

namespace MACPlugin
{
    public class ActionCameraPlugin : IPluginCameraBehaviour
    {
        public IPluginSettings settings => _settings;
        // Matching naming schema
        ActionCameraSettings _settings = new ActionCameraSettings();

#if DEBUG
        public string name => "Menithal' Action Camera DEV BUILD";
#else
        public string name => "Menithal' Action Camera";
#endif
        public string ID => "ActionCamera";
        public string author => "MA 'Menithal' Lahtinen";
        public string version => "0.7.1a";

        public event EventHandler ApplySettings;
        private ActionCameraDirector cameraDirector;
        private TimerHelper timerHelper;
        private AvatarReferenceSignal avatarRefSignal;

        // Called when the Plugin is selected
        public void OnActivate(PluginCameraHelper helper)
        {
            PluginLog.Log("ActionCameraPlugin", "OnActivate");
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

            PluginLog.Log("ActionCameraPlugin", "OnDeactivate ");

            ApplySettings?.Invoke(this, EventArgs.Empty);

            avatarRefSignal?.OnChanged.RemoveListener(OnAvatarChanged);
            avatarRefSignal = null;
        }

        // Called when Plugin component is destroyed.
        public void OnDestroy()
        {
            PluginLog.Log("ActionCameraPlugin", "OnDestroy");
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
            PluginLog.Log("ActionCameraPlugin", "OnSettingsDeserialized");
            PluginLog.Log("ActionCameraPlugin", "generalCameraSwapClamp " + _settings.cameraSwapTimeLock);
            PluginLog.Log("ActionCameraPlugin", "actionCameraSwapClamp " + _settings.cameraPositionTimeLock);

            PluginLog.Log("ActionCameraPlugin", "cameraVerticalLock " + _settings.cameraVerticalLock);
            PluginLog.Log("ActionCameraPlugin", "cameraShoulderDistance " + _settings.cameraShoulderDistance);
            PluginLog.Log("ActionCameraPlugin", "cameraShoulderAngle " + _settings.cameraShoulderAngle);
            PluginLog.Log("ActionCameraPlugin", "cameraShoulderPositioningTime " + _settings.cameraShoulderPositioningTime);

            PluginLog.Log("ActionCameraPlugin", "cameraBodyPositioningTime " + _settings.cameraBodyPositioningTime);
            PluginLog.Log("ActionCameraPlugin", "cameraBodyAngle " + _settings.cameraBodyAngle);
            PluginLog.Log("ActionCameraPlugin", "cameraBodyDistance " + _settings.cameraBodyDistance);
            PluginLog.Log("ActionCameraPlugin", "cameraBodyLookAtForward " + _settings.cameraBodyLookAtForward);

            PluginLog.Log("ActionCameraPlugin", "reverseFBT " + _settings.reverseFBT);
            PluginLog.Log("ActionCameraPlugin", "reverseShoulder " + _settings.reverseShoulder);
            PluginLog.Log("ActionCameraPlugin", "forwardVerticalOffset " + _settings.forwardVerticalOffset);
            PluginLog.Log("ActionCameraPlugin", "forwardHorizontalOffset " + _settings.forwardHorizontalOffset);
            PluginLog.Log("ActionCameraPlugin", "forwardDistance " + _settings.forwardDistance);
            PluginLog.Log("ActionCameraPlugin", "removeAvatarInsteadOfHead " + _settings.removeAvatarInsteadOfHead);

            PluginLog.Log("ActionCameraPlugin", "disableTopCamera " + _settings.disableTopCamera);
            PluginLog.Log("ActionCameraPlugin", "disableFBTCamera " + _settings.disableFBTCamera);
            PluginLog.Log("ActionCameraPlugin", "disableFPSCamera " + _settings.disableFPSCamera);
            
            PluginLog.Log("ActionCameraPlugin", "inBetweenCameraEnabled " + _settings.inBetweenCameraEnabled);
            PluginLog.Log("ActionCameraPlugin", "averageHandsWithHead " + _settings.averageHandsWithHead);
            PluginLog.Log("ActionCameraPlugin", "useDominantHand " + _settings.useDominantHand);
            PluginLog.Log("ActionCameraPlugin", "rightHandDominant " + _settings.rightHandDominant);
            // Need to make sure everything else is updated
            cameraDirector.SetSettings(_settings);
        }

        public void OnAvatarChanged(Avatar avatar)
        {
            PluginLog.Log("ActionCameraPlugin", "OnAvatarChanged ");
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
