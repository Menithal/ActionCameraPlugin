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

// Keeping out of Namespace since otherwise Serialization wasnt succesfull (probably something to do with the Reflection done on the other end?)
public class ActionCameraSettings : IPluginSettings
{
    // How many seconds until swapping another camera
    public float generalCameraSwapClamp { get; set; } = 8;
    // actionCamera can swap faster than general Camera and people tend to change look directions quite quickly.
    public float actionCameraSwapClamp { get; set; } = 0.8f;

    // How fast should we swap between 
    public float shoulderCameraPositioningTime = 0.9f;
    public float bodyCameraPositioningTime = 0.9f;
    public float actionCameraDistance = 1.4f;

    public bool reverseFBT = false;
    public bool reverseShoulder = false;
    public float controlMovementThreshold = 4; // Meters per framea
    // Users tend to have headset a bit higher, so when they are looking down sights they are not fully looking up. This offsets that
    public float forwardVerticalOffset = 0;
    public float forwardHorizontalOffset = 5;
    public float forwardDistance = 10;

    public bool removeAvatarInsteadOfHead = true;
    public bool disableTopCamera = true;
    public bool disableFBTCamera = false;
    public bool disableFPSCamera = false;

    public bool inBetweenCameraEnabled = false;

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
        public IPluginSettings settings => _settings ;
        // Matching naming schema
        readonly ActionCameraSettings _settings  = new ActionCameraSettings();

#if DEBUG
        public string name => "Menithal' Action Camera DEV BUILD";
#else
        public string name => "Menithal' Action Camera";
#endif
        public string ID => "ActionCamera";
        public string author => "MA 'Menithal' Lahtinen";
        public string version => "0.6.0a";

        public event EventHandler ApplySettings;
        private ActionCameraDirector cameraManager;
        private TimerHelper timerHelper;
        private AvatarReferenceSignal avatarRefSignal;

        // Called when the Plugin is selected
        public void OnActivate(PluginCameraHelper helper)
        {
            PluginLog.Log("ActionCameraPlugin", "OnActivate");
            timerHelper = new TimerHelper();
            cameraManager = new ActionCameraDirector(_settings , helper, ref timerHelper);

            AvatarManager avatarManager = Resources.FindObjectsOfTypeAll<AvatarManager>().FirstOrDefault();
            avatarRefSignal = avatarManager?.GetPrivateField<AvatarReferenceSignal>("_avatarInstantiated");
            avatarRefSignal?.OnChanged.AddListener(OnAvatarChanged);

            OnAvatarChanged(avatarRefSignal?.Value);
        }

        // called when the Plugin is deselected OR when Liv is being closed down.
        public void OnDeactivate()
        {
            
            PluginLog.Log("ActionCameraPlugin", "OnDeactivate ", true);
        
            ApplySettings?.Invoke(this, EventArgs.Empty);

            avatarRefSignal?.OnChanged.RemoveListener(OnAvatarChanged);
            avatarRefSignal = null;
        }

        // Called when Plugin component is destroyed.
        public void OnDestroy()
        {
            PluginLog.Log("ActionCameraPlugin", "OnDestroy", true);
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
            PluginLog.Log("ActionCameraPlugin", "OnSettingsDeserialized", true);
            PluginLog.Log("ActionCameraPlugin", "generalCameraSwapClamp " + _settings.generalCameraSwapClamp, true);
            PluginLog.Log("ActionCameraPlugin", "actionCameraSwapClamp " + _settings.actionCameraSwapClamp, true);
            PluginLog.Log("ActionCameraPlugin", "shoulderCameraPositioningTime " + _settings.shoulderCameraPositioningTime, true);
            PluginLog.Log("ActionCameraPlugin", "bodyCameraPositioningTime " + _settings.bodyCameraPositioningTime, true);
            PluginLog.Log("ActionCameraPlugin", "actionCameraDistance " + _settings.actionCameraDistance, true);
            PluginLog.Log("ActionCameraPlugin", "reverseFBT " + _settings.reverseFBT, true);
            PluginLog.Log("ActionCameraPlugin", "reverseShoulder " + _settings.reverseShoulder, true);
            PluginLog.Log("ActionCameraPlugin", "forwardVerticalOffset " + _settings.forwardVerticalOffset, true);
            PluginLog.Log("ActionCameraPlugin", "forwardHorizontalOffset " + _settings.forwardHorizontalOffset, true);
            PluginLog.Log("ActionCameraPlugin", "forwardDistance " + _settings.forwardDistance, true);
            PluginLog.Log("ActionCameraPlugin", "removeAvatarInsteadOfHead " + _settings.removeAvatarInsteadOfHead, true);
            PluginLog.Log("ActionCameraPlugin", "disableTopCamera " + _settings.disableTopCamera, true);
            PluginLog.Log("ActionCameraPlugin", "disableFBTCamera " + _settings.disableFBTCamera, true);
            PluginLog.Log("ActionCameraPlugin", "disableFPSCamera " + _settings.disableFPSCamera, true);
            PluginLog.Log("ActionCameraPlugin", "inBetweenCameraEnabled " + _settings.inBetweenCameraEnabled, true);
            PluginLog.Log("ActionCameraPlugin", "averageHandsWithHead " + _settings.averageHandsWithHead, true);
            PluginLog.Log("ActionCameraPlugin", "useDominantHand " + _settings.useDominantHand, true);
            PluginLog.Log("ActionCameraPlugin", "rightHandDominant " + _settings.rightHandDominant, true);

            cameraManager.SetSettings(_settings);
        }

        public void OnAvatarChanged(Avatar avatar)
        {
            PluginLog.Log("ActionCameraPlugin", "OnAvatarChanged ", true);
            cameraManager.SetAvatar(avatar);
        }

        // Called Every Frame.
        public void OnUpdate()
        {
            timerHelper.AddTime(Time.deltaTime);
            cameraManager.SelectCamera();
            cameraManager.HandleCameraView();
        }
    }

}
