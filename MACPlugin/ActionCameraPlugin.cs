/**  Copyright 2020 Matti 'Menithal' Lahtinen

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
using LIV.Avatar;
using System;
using System.Linq;
using UnityEngine;
public class ActionCameraSettings : IPluginSettings
{
    // How many seconds until swapping another camera
    public float generalCameraSwapClamp { get; set; } = 8;
    // actionCamera can swap faster than general Camera and people tend to change look directions quite quickly.
    public float actionCameraSwapClamp { get; set; } = 0.8f;


    // How fast should we swap between 
    public float shoulderCameraSwapTime = 1f;
    public float bodyCameraSwapTime = 1f;
    public float actionCameraDistance = 1f;

    public bool removeAvatarInsteadOfHead = true;
    public bool disableTopCamera = true;
    public bool disableFBTCamera = false;
    public bool disableFPSCamera = false;
    public bool reverseFBT = false;
    public bool reverseShoulder = false;
    public float controlMovementThreshold = 4; // Meters per framea
    // Users tend to have headset a bit higher, so when they are looking down sights they are not fully looking up. This offsets that
    public float forwardVerticalOffset = 0;
    public float forwardHorizontalOffset = 5;
    public float forwardDistance = 10;


    // TODO: if Enabled, average head forward with Hand Forwards (dominance)
    // Assist
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
        readonly ActionCameraSettings _settings  = new ActionCameraSettings();

        public string ID => "ActionCamera";
        public string name => "Menithal' Action Camera";
        public string author => "MAL 'Menithal'";
        public string version => "0.1.0a";

        public event EventHandler ApplySettings;

        private ActionCameraDirector cameraManager;
        private TimerHelper timerHelper;
        private AvatarReferenceSignal avatarRefSignal;

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

        public void OnDeactivate()
        {
            
            PluginLog.Log("ActionCameraPlugin", "OnDeactivate ", true);

        
            ApplySettings?.Invoke(this, EventArgs.Empty);

            avatarRefSignal?.OnChanged.RemoveListener(OnAvatarChanged);
            avatarRefSignal = null;
            foreach (Delegate del in ApplySettings.GetInvocationList())
            {
                PluginLog.Log("ActionCameraPlugin", "Invokator found " + del.ToString(), true);
            }
        }

        public void OnDestroy()
        {
            PluginLog.Log("ActionCameraPlugin", "OnDestroy", true);
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        // Called when Settings are deserialized from the file?
        public void OnSettingsDeserialized()
        {
        
            PluginLog.Log("ActionCameraPlugin", "OnSettingsDeserialized", true);
            PluginLog.Log("ActionCameraPlugin", "RemoveAvatarInsteadOfHead " + _settings.removeAvatarInsteadOfHead, true);
            PluginLog.Log("ActionCameraPlugin", "CameraSwapClamp " + _settings.actionCameraSwapClamp, true);
            PluginLog.Log("ActionCameraPlugin", "DisableTopCamera " + _settings.disableTopCamera, true);
            PluginLog.Log("ActionCameraPlugin", "DisableFBTCamera " + _settings.disableFBTCamera, true);
            PluginLog.Log("ActionCameraPlugin", "DisableFBTCamera " + _settings.disableFPSCamera, true);
            PluginLog.Log("ActionCameraPlugin", "ControlMovementTreshold " + _settings.controlMovementThreshold, true);
            PluginLog.Log("ActionCameraPlugin", "ForwardVerticalOffset " + _settings.forwardVerticalOffset, true);
            PluginLog.Log("ActionCameraPlugin", "ForwardHorizontalOffset " + _settings.forwardHorizontalOffset, true);
            PluginLog.Log("ActionCameraPlugin", "ForwardDistance " + _settings.forwardDistance, true);
            PluginLog.Log("ActionCameraPlugin", "AverageHandsWithHead " + _settings.averageHandsWithHead, true);
            PluginLog.Log("ActionCameraPlugin", "UseDominantHand " + _settings.useDominantHand, true);
            PluginLog.Log("ActionCameraPlugin", "RightHandDominant " + _settings.rightHandDominant, true);

            cameraManager.SetSettings(_settings);
        }

        public void OnAvatarChanged(Avatar avatar)
        {
            PluginLog.Log("ActionCameraPlugin", "OnAvatarChanged ", true);
            cameraManager.SetAvatar(avatar);

            // Investigate if it is possible to access Visemes from here.
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
