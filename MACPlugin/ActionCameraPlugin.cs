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
using MACPlugin.Utility;
using BlastonCameraBehaviour;

// Keeping out of Namespace since otherwise Serialization wasnt succesfull (probably something
// to do with the Reflection done on the other end?)
public class ActionCameraSettings : IPluginSettings
{
    public string configurationName = "MACPluginDefault.config";
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
        public string version => "1.0.0a";

        public event EventHandler ApplySettings;
        private ActionCameraDirector cameraDirector;
        private TimerHelper timerHelper;
        private AvatarReferenceSignal avatarRefSignal;

        // Called when the Plugin is selected
        public void OnActivate(PluginCameraHelper helper)
        {
            PluginLog.Log("ActionCameraPlugin", "OnActivate");
            timerHelper = new TimerHelper();
            ConfigUtility utility = new ConfigUtility(_settings.configurationName);
            cameraDirector = new ActionCameraDirector(utility.Config, helper, ref timerHelper);

            AvatarManager avatarManager = Resources.FindObjectsOfTypeAll<AvatarManager>().FirstOrDefault();
            avatarRefSignal = avatarManager?.GetPrivateField<AvatarReferenceSignal>("_avatarInstantiated");
            avatarRefSignal?.OnChanged.AddListener(OnAvatarChanged);

            OnAvatarChanged(avatarRefSignal?.Value);

            ActionController.Instance.Initialize();
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
            ConfigUtility utility = new ConfigUtility(_settings.configurationName);
            cameraDirector.SetSettings(utility.Config);
        }

        public void OnAvatarChanged(LIV.Avatar.Avatar avatar)
        {
            PluginLog.Log("ActionCameraPlugin", "OnAvatarChanged ");
            cameraDirector.SetAvatar(avatar);
        }

        // Called Every Frame.
        public void OnUpdate()
        {
            timerHelper.AddTime(Time.deltaTime);
            UpdateInput();
            cameraDirector.SelectCamera();
            cameraDirector.HandleCameraView();
        }

        private void UpdateInput()
        {
            ActionController.Instance.Update();

            if (ActionController.Instance.aAction.IsStarted)
            {
                cameraDirector.ForceSelectCamera(cameraDirector.FullBodyActionCamera);
            }

            if (ActionController.Instance.bAction.IsStarted)
            {
                cameraDirector.ForceSelectCamera(cameraDirector.TacticalCamera);
            }
        }
    }

}
