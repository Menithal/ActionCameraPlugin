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
using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;



#if !SIMPLIFIED
namespace MACPlugin
#else
namespace SimpleMacPlugin
#endif
{
    public class ActionCameraDirector
    {
        public readonly FPSCamera FPSCamera;
        public readonly ActionCamera OverShoulderCamera;
#if !SIMPLIFIED
        public readonly ActionCamera FullBodyActionCamera;
        // CameraTop Could be saved for Twitch stuff. (UAV online)
        public readonly ActionCamera TacticalCamera;
     
// private SteamVR_Action_Boolean triggerAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "ResetAction");
  //      private SteamVR_Action_Vector1 triggerSensitivityActivity = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "GunTriggerAction");
      
#endif
        private readonly PluginCameraHelper cameraHelper;
        private readonly TimerHelper timerHelper;

        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        private Vector3 cameraLookAtTarget;


        private Vector3 cameraLastLookAtVelocity = Vector3.zero;
        private Vector3 cameraLastVelocity = Vector3.zero;

        private Vector3 cameraLookAtVelocity = Vector3.zero;
        private Vector3 cameraVelocity = Vector3.zero;
        private Vector3 cameraPositionTarget = Vector3.zero;

        private ActionCamera currentCamera;
        private ActionCamera lastCamera;
        private ActionCameraSettings pluginSettings;
        private Avatar currentAvatar;

        private readonly LivPlayerEntity player;
        private readonly System.Random randomizer;
        private bool isCameraStatic = false;

        // 30 checks a second 
        private bool inGunMode = false;
#if SIMPLIFIED

        private static float CONTROLLER_THROTTLE = 0.5f;
#else
        private static float CONTROLLER_THROTTLE = 1f;
#endif

        private ulong TriggerHandle = 0;
        public ActionCameraDirector(ActionCameraSettings pluginSettings, PluginCameraHelper helper, ref TimerHelper timerHelper)
        {
            this.player = new LivPlayerEntity(helper, ref timerHelper);
            this.timerHelper = timerHelper;
            this.cameraHelper = helper;
            this.pluginSettings = pluginSettings;

            randomizer = new System.Random();
            player.SetOffsets(pluginSettings.forwardHorizontalOffset, pluginSettings.forwardVerticalOffset, pluginSettings.forwardDistance);
            
            FPSCamera = new FPSCamera(pluginSettings, 0.2f);
            OverShoulderCamera = new ShoulderActionCamera(pluginSettings);


            

            EVRInputError err = OpenVR.Input.GetActionHandle("/actions/default/in/use", ref TriggerHandle);
            if (err != EVRInputError.None)
            {
                TriggerHandle = 0;
                PluginLog.Error("ActionCameraDirector", "Could not setup handle listener");
            }

#if !SIMPLIFIED
            FullBodyActionCamera = new FullBodyActionCamera(pluginSettings);
            TacticalCamera = new TopDownActionCamera(pluginSettings, 0.6f, 6f);
#endif

            SetCamera(OverShoulderCamera);
        }
        public void SetAvatar(Avatar avatar)
        {
            currentAvatar = avatar;
        }
        public void SetSettings(ActionCameraSettings settings)
        {
            pluginSettings = settings;
            player.SetOffsets(settings.forwardHorizontalOffset, settings.forwardVerticalOffset, settings.forwardDistance);

            FPSCamera.SetPluginSettings(settings);

            OverShoulderCamera.SetPluginSettings(settings);
#if !SIMPLIFIED
            FullBodyActionCamera.SetPluginSettings(settings);
            TacticalCamera.SetPluginSettings(settings);
#endif

        }

        private void SetCamera(ActionCamera camera, bool saveLast = true, float timerOverride = 0)
        {
            if (currentCamera != camera)
            {
                if (saveLast)
                {
                    lastCamera = currentCamera;
                }
                currentCamera = camera;
                isCameraStatic = false;
                timerHelper.ResetGlobalCameraTimer();
                timerHelper.ResetCameraActionTimer();
                if (timerOverride > 0)
                {
                    timerHelper.SetGlobalTimer(timerOverride);
                }
#if !SIMPLIFIED
                inGunMode = false;
#endif
            }
        }
        public void SelectCamera()
        {

            player.CalculateInfo();
            if (timerHelper.controllerTimer >= CONTROLLER_THROTTLE)
            {
                // TODO: Take account movements of hands as well
                /* If user swining alot, an sample from x amount of time could tell if user is swinginig their hands in multiple directions 
                * (indicating meelee) or if they are steady
                * Or if they have a rapid back and forth motion.

                * Current Logic:

                * While Aiming Forwards:
                * If user turns their head to Right,  the direction left of the camera is true.
                * the Left Angle, the controllers should be reverse of where the user is looking at.
                * If looking completely down, user is most likely interacting with their inventory so show fps or full body
                * If Looking up they are about to do something, so
                */
                bool canSwapCamera = (timerHelper.globalTimer > pluginSettings.cameraSwapTimeLock);

                Vector3 averageHandPosition = player.handAverage;
                Vector3 handDirection;

#if SIMPLIFIED
                ulong leftHandle = 0;
                OpenVR.Input.GetInputSourceHandle("/user/hand/left", ref leftHandle);
                ulong rightHandle = 0;
                OpenVR.Input.GetInputSourceHandle("/user/hand/right", ref rightHandle);

                var size = (uint)Marshal.SizeOf((typeof(InputDigitalActionData_t)));
                InputDigitalActionData_t data= new InputDigitalActionData_t();

                EVRInputError error;

                bool trigger = false;
                if (pluginSettings.rightHandDominant) 
                {
                    handDirection = (player.leftHand.position - player.rightHand.position).normalized;
                    error = OpenVR.Input.GetDigitalActionData(TriggerHandle, ref data, size, leftHandle);
                }
                else
                {
                    handDirection = (player.rightHand.position - player.leftHand.position).normalized;
                    error = OpenVR.Input.GetDigitalActionData(TriggerHandle, ref data, size, rightHandle);
                }

                if (error == EVRInputError.None)
                {
                    trigger = data.bState; 
                }
#endif

                Vector3 headForwardPosition = player.head.TransformPoint(Vector3.up * 0.05f);
                Vector3 headBackPosition = player.head.TransformPoint(Vector3.back * 0.1f);
                bool areHandsAboveThreshold = (headForwardPosition.y) > player.leftHand.position.y || (headForwardPosition.y) > player.rightHand.position.y;

                bool isAimingTwoHandedForward = Mathf.Rad2Deg *
                    PluginUtility.GetConeAngle(headBackPosition, averageHandPosition + handDirection * 2f, player.head.right) <
                        pluginSettings.cameraGunHeadAlignAngleTrigger * 1.4f;

                // player is looking down sights.

                if (!pluginSettings.disableGunCamera && areHandsAboveThreshold
                    && Mathf.Abs(player.headRRadialDelta.x) < pluginSettings.controlMovementThreshold
                    && Mathf.Abs(player.headRRadialDelta.y) < pluginSettings.controlVerticalMovementThreshold 
                    && isAimingTwoHandedForward && (canSwapCamera || pluginSettings.FPSCameraOverride) && !inGunMode)

                {
                    inGunMode = true;

                    SetCamera(FPSCamera, true);
                    timerHelper.ResetCameraGunTimer();
                    PluginLog.Log("ActionCameraDirector", "In FPS Override (two handed forward)");
                }
                else if (inGunMode && canSwapCamera)
                {
                    if (!(isAimingTwoHandedForward))
                    {
                        SnapCamera(lastCamera, true);
                        SetCamera(lastCamera);
                        PluginLog.Log("ActionCameraDirector", "Returning Back to earlier");
                        // Should actually just snap to the new positions instead, so
                    }
                    timerHelper.ResetCameraGunTimer();
                }

                else if (PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headBelowDirection) < 45 &&
                    player.headRRadialDelta.y < -pluginSettings.controlVerticalMovementThreshold &&
                    !pluginSettings.disableFBTCamera && canSwapCamera)
                {
                    PluginLog.Log("ActionCameraDirector", "Pointing Down FullBody");
#if !SIMPLIFIED
                    SetCamera(FullBodyActionCamera);
#else
                    SetCamera(FPSCamera);
#endif
                }
                else if (PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headAboveDirection) < 45 &&
                     player.headRRadialDelta.y > pluginSettings.controlVerticalMovementThreshold &&
                    !(pluginSettings.disableTopCamera && pluginSettings.disableFBTCamera) && canSwapCamera)
                {
                    // Hands Are Pointing up-ish, while head is moving up (probably checking on something, wrist, etc.)

                    PluginLog.Log("ActionCameraDirector", "Pointing Up, Moving Head Up: Tactical or FullBody");
                    if (randomizer.Next(0, 100) > 80 && !pluginSettings.disableTopCamera || pluginSettings.disableFBTCamera)
                    {
#if !SIMPLIFIED
                        SetCamera(TacticalCamera);
#else
                        SetCamera(FPSCamera);
#endif
                    }
                    else
                    {
#if !SIMPLIFIED
                         SetCamera(FullBodyActionCamera);
#else
                        SetCamera(OverShoulderCamera);
#endif


                    }
                }

                // Looking Side to Side while pointing forwards. Action is ahead.
                else if ((PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headForwardDirection) < 80) &&
                    Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.controlMovementThreshold && canSwapCamera)
                {
                    SetCamera(OverShoulderCamera);
                }


                timerHelper.ResetControllerTimer();
            }
            HandleCameraView();
        }

        public void SnapCamera(ActionCamera camera, bool revert = false)
        {

            PluginLog.Log("ActionCameraDirector", "SNAP ");
            camera.ApplyBehavior(ref cameraPositionTarget, ref cameraLookAtTarget, player, isCameraStatic);

            if (revert)
            {
                cameraLookAtVelocity = cameraLastLookAtVelocity;
                cameraVelocity = cameraLastVelocity;
            }
            else
            {
                cameraLastLookAtVelocity = cameraLookAt;
                cameraLastVelocity = cameraVelocity;
                cameraLookAtVelocity = Vector3.zero;
                cameraVelocity = Vector3.zero;
            }

            cameraPosition = cameraPositionTarget;
            cameraLookAt = cameraLookAtTarget;
        }
        public void HandleCameraView()
        {
            // Call the camera's behavior.
            currentCamera.ApplyBehavior(ref cameraPositionTarget, ref cameraLookAtTarget, player, isCameraStatic);
            isCameraStatic = currentCamera.staticCamera;


            if (currentAvatar != null)
            {
                if (pluginSettings.removeAvatarInsteadOfHead)
                {
                    currentAvatar.avatarSettings.showAvatar.Value = !currentCamera.removeHead;
                }
                else
                {
                    currentAvatar.avatarSettings.showAvatarHead.Value = !currentCamera.removeHead;
                }
                timerHelper.ResetRemoveAvatarTimer();
            }

            cameraPosition = Vector3.SmoothDamp(cameraPosition, cameraPositionTarget, ref cameraVelocity, currentCamera.GetBetweenTime());
            cameraLookAt = Vector3.SmoothDamp(cameraLookAt, cameraLookAtTarget, ref cameraLookAtVelocity, currentCamera.GetBetweenTime());

            Vector3 lookDirection = cameraLookAt - cameraPosition;

            Quaternion rotation = currentCamera.GetRotation(lookDirection, player);

            cameraHelper.UpdateCameraPose(cameraPosition, rotation);
            cameraHelper.UpdateFov(currentCamera.GetFOV());
        }
    }
}
