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
using UnityEngine;
namespace MACPlugin
{
    public class ActionCameraDirector
    {
        public readonly ActionCamera OverShoulderCamera;
        public readonly ActionCamera FPSCamera;
        public readonly ActionCamera FullBodyActionCamera;
        // CameraTop Could be saved for Twitch stuff. (UAV online)
        public readonly ActionCamera TacticalCamera;


        private readonly PluginCameraHelper cameraHelper;
        private readonly TimerHelper timerHelper;

        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        private Vector3 cameraLookAtTarget;

        private Vector3 cameraLookAtVelocity = Vector3.zero;
        private Vector3 cameraVelocity = Vector3.zero;
        private Vector3 cameraPositionTarget = Vector3.zero;

        private ActionCamera currentCamera;
        private ActionCameraSettings pluginSettings;
        private Avatar currentAvatar;

        private readonly LivPlayerEntity player;
        private readonly System.Random randomizer;
        private bool isCameraStatic = false;

        // 30 checks a second 
        private readonly static float CONTROLLER_THROTTLE = 1 / 30;
        public ActionCameraDirector(ActionCameraSettings pluginSettings, PluginCameraHelper helper, ref TimerHelper timerHelper)
        {
            this.player = new LivPlayerEntity(helper, ref timerHelper);
            this.timerHelper = timerHelper;
            this.cameraHelper = helper;
            this.pluginSettings = pluginSettings;

            randomizer = new System.Random();
            player.SetOffsets(pluginSettings.forwardHorizontalOffset, pluginSettings.forwardVerticalOffset, pluginSettings.forwardDistance);

            OverShoulderCamera = new ShoulderActionCamera(pluginSettings,  new Vector3(0.8f, -0f, -pluginSettings.actionCameraDistance));
            FullBodyActionCamera = new FullBodyActionCamera(pluginSettings, new Vector3(1.6f, 0.4f, pluginSettings.actionCameraDistance));
            FPSCamera = new FPSCamera(pluginSettings, 0.1f);
            TacticalCamera = new TopDownActionCamera(pluginSettings, 0.6f, 6f);

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

            OverShoulderCamera.SetPluginSettings(settings);
            FPSCamera.SetPluginSettings(settings);
            FullBodyActionCamera.SetPluginSettings(settings);
            TacticalCamera.SetPluginSettings(settings);
        }

        private void SetCamera(ActionCamera camera)
        {
            if (currentCamera != camera)
            {
                currentCamera = camera;
                isCameraStatic = false;
                timerHelper.ResetGlobalCameraTimer();
                timerHelper.ResetActionCameraTimer();
            }
        }
        public void SelectCamera()
        {

            if (timerHelper.controllerTimer >= CONTROLLER_THROTTLE)
            {
                player.CalculateInfo();


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
                bool canSwapCamera = (timerHelper.globalTimer > pluginSettings.generalCameraSwapClamp);
                if (canSwapCamera)
                {
                    if (PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headBelowDirection) < 60 &&
                         player.headRRadialDelta.y < -pluginSettings.controlMovementThreshold &&
                         !(pluginSettings.disableFPSCamera && pluginSettings.disableFBTCamera))
                    {

                        PluginLog.Log("ActionCameraDirector", "Pointing Down, Moving Head Down: FPS or FullBody", true);
                        // Hands are pointing down is while head is moving down (probably inventory)
                        if (randomizer.Next(0, 100) > 50 && !pluginSettings.disableFBTCamera || pluginSettings.disableFPSCamera)
                        {
                            SetCamera(FullBodyActionCamera);
                        }
                        else
                        {
                            SetCamera(FPSCamera);
                        }
                    }
                    else if (PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headAboveDirection) < 60 &&
                         player.headRRadialDelta.y > pluginSettings.controlMovementThreshold &&
                        !(pluginSettings.disableTopCamera && pluginSettings.disableFBTCamera))
                    {
                        // Hands Are Pointing up-ish, while head is moving up (probably checking on something, wrist, etc.)

                        PluginLog.Log("ActionCameraDirector", "Pointing Up, Moving Head Up: Tactical or FullBody", true);
                        if (randomizer.Next(0, 100) > 80 && !pluginSettings.disableTopCamera || pluginSettings.disableFBTCamera)
                        {
                            SetCamera(TacticalCamera);
                        }
                        else
                        {
                            SetCamera(FullBodyActionCamera);
                        }
                    }
                    // Looking Side to Side while pointing forwards. Action is ahead.
                    else if ((PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headForwardDirection) < 80) &&
                        Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.controlMovementThreshold)
                    {
                        PluginLog.Log("ActionCameraDirector", "Moving on Side, Shoulder", true);
                        SetCamera(OverShoulderCamera);
                    }
                }

                timerHelper.ResetControllerTimer();
            }
            HandleCameraView();
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
            }

            // BAC data calculation
            // TODO: Allow user to select MoveToward, SmoothDamp, Slerp and Lerp

            cameraPosition = Vector3.SmoothDamp(cameraPosition, cameraPositionTarget, ref cameraVelocity, currentCamera.timeBetweenChange);
            cameraLookAt = Vector3.SmoothDamp(cameraLookAt, cameraLookAtTarget, ref cameraLookAtVelocity, currentCamera.timeBetweenChange);

            Vector3 lookDirection = cameraLookAt - cameraPosition;

            Quaternion rotation = Quaternion.LookRotation(lookDirection);

            cameraHelper.UpdateCameraPose(cameraPosition, rotation);
            cameraHelper.UpdateFov(currentCamera.fov);
        }
    }
}
