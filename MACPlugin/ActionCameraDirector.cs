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
using MACPlugin.Utility;

namespace MACPlugin
{
    public class ActionCameraDirector
    {
        public readonly ActionCamera OverShoulderCamera;
        public readonly ActionCamera FullBodyActionCamera;
        public readonly FPSCamera FPSCamera;
        // CameraTop Could be saved for Twitch stuff. (UAV online)
        public readonly ActionCamera TacticalCamera;
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
        private ActionCameraConfig pluginSettings;
        private Avatar currentAvatar;

        private readonly LivPlayerEntity player;
        private readonly System.Random randomizer;
        private bool inGunMode = false;

        // 30 checks a second 
        private static float CONTROLLER_THROTTLE = 1f;
        private float distanceFromTargetSwap = 0;
        public ActionCameraDirector(ActionCameraConfig pluginSettings, PluginCameraHelper helper, ref TimerHelper timerHelper)
        {
            this.player = new LivPlayerEntity(helper, ref timerHelper);
            this.timerHelper = timerHelper;
            this.cameraHelper = helper;
            this.pluginSettings = pluginSettings;

            randomizer = new System.Random();
            player.SetOffsets(pluginSettings.forwardHorizontalOffset, pluginSettings.forwardVerticalOffset, pluginSettings.forwardDistance);

            OverShoulderCamera = new ShoulderActionCamera(pluginSettings);
            FullBodyActionCamera = new FullBodyActionCamera(pluginSettings);
            FPSCamera = new FPSCamera(pluginSettings, 0.2f);
            TacticalCamera = new TopDownActionCamera(pluginSettings, 0.6f, 6f);

            SetCamera(OverShoulderCamera);
        }
        public void SetAvatar(Avatar avatar)
        {
            currentAvatar = avatar;
        }
        public void SetSettings(ActionCameraConfig settings)
        {
            pluginSettings = settings;
            player.SetOffsets(settings.forwardHorizontalOffset, settings.forwardVerticalOffset, settings.forwardDistance);

            OverShoulderCamera.SetPluginSettings(settings);
            FPSCamera.SetPluginSettings(settings);
            FullBodyActionCamera.SetPluginSettings(settings);
            TacticalCamera.SetPluginSettings(settings);

            settings.PrintContents();
        }

        private void SetCamera(ActionCamera camera, float timerOverride = 0)
        {
            if (currentCamera != camera)
            {
                lastCamera = currentCamera;

                currentCamera = camera;
                timerHelper.ResetGlobalCameraTimer();
                timerHelper.ResetCameraActionTimer();
                if (timerOverride > 0)
                {
                    timerHelper.SetGlobalTimer(timerOverride);
                }
                inGunMode = false;

                prevCameraTarget = cameraPositionTarget;
                prevCameraLookAtTarget = cameraLookAtTarget;
            }
        }


        public void SelectCameraLegacy()
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
                if (pluginSettings.rightHandDominant)
                {
                    handDirection = (player.leftHand.position - player.rightHand.position).normalized;
                }
                else
                {
                    handDirection = (player.rightHand.position - player.leftHand.position).normalized;
                }

                Vector3 headForwardPosition = player.head.TransformPoint(Vector3.up * 0.05f);
                Vector3 headBackPosition = player.head.TransformPoint(Vector3.forward * -0.1f);
                bool areHandsAboveThreshold = (headForwardPosition.y) > player.leftHand.position.y || (headForwardPosition.y) > player.rightHand.position.y;
                bool isAimingTwoHandedForward = Mathf.Rad2Deg *
               PluginUtility.GetConeAngle(headBackPosition, averageHandPosition + handDirection * 2f, player.head.right) <
                   pluginSettings.cameraGunHeadAlignAngleTrigger;

                // player is looking down sights.
                if (!pluginSettings.disableGunCamera && areHandsAboveThreshold
                    && Mathf.Abs(player.headRRadialDelta.x) < pluginSettings.controlMovementThreshold
                    && Mathf.Abs(player.headRRadialDelta.y) < pluginSettings.controlVerticalMovementThreshold
                    /*&& isAimingTwoHandedForward */&& (canSwapCamera || pluginSettings.FPSCameraOverride) && !inGunMode)
                {
                    SetCamera(FPSCamera);
                    inGunMode = true;
                    SnapCamera(currentCamera);
                    timerHelper.ResetCameraGunTimer();
                    PluginLog.Log("ActionCameraDirector", "In FPS Override (two handed forward)");
                }
                else if (inGunMode && canSwapCamera)
                {
                    if (!(isAimingTwoHandedForward))
                    {
                        SnapCamera(lastCamera);
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

                    SetCamera(FullBodyActionCamera);
                }
                else if (PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headAboveDirection) < 45 &&
                     player.headRRadialDelta.y > pluginSettings.controlVerticalMovementThreshold &&
                    !(pluginSettings.disableTopCamera && pluginSettings.disableFBTCamera) && canSwapCamera)
                {
                    // Hands Are Pointing up-ish, while head is moving up (probably checking on something, wrist, etc.)

                    PluginLog.Log("ActionCameraDirector", "Pointing Up, Moving Head Up: Tactical or FullBody");
                    if (randomizer.Next(0, 100) > 80 && !pluginSettings.disableTopCamera || pluginSettings.disableFBTCamera)
                    {
                        SetCamera(TacticalCamera);
                    }
                    else
                    {
                        if (randomizer.Next(0, 100) > 50)
                        {
                            SetCamera(FullBodyActionCamera);

                        }
                        else
                        {
                            SetCamera(OverShoulderCamera);

                        }
                    }

                }
                // Looking Side to Side while pointing forwards. Action is ahead.
                else if ((PluginUtility.AverageCosAngleOfControllers(player.rightHand, player.leftHand, player.headForwardDirection) < 80) &&
                    Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.controlMovementThreshold && canSwapCamera)
                {
                    PluginLog.Log("ActionCameraDirector", "Moving on Side, Shoulder");
                    SetCamera(OverShoulderCamera);
                }

                timerHelper.ResetControllerTimer();
            }
        }

        public void SnapCamera(ActionCamera camera, bool revert = false)
        {

            PluginLog.Log("ActionCameraDirector", "SNAP ");
            camera.ApplyBehavior(ref cameraPositionTarget, ref cameraLookAtTarget, player);

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

        private Vector3 prevCameraTarget;
        private Vector3 prevCameraLookAtTarget;


        public void HandleCameraView()
        {
            if (pluginSettings.ready)
            {

                TimerHelper timerHelper = player.timerHelper;
                player.timerHelper.AddTime(Time.deltaTime);

                sbyte estimatedSide = ((player.headRRadialDelta.x < 0f) ? (sbyte)(-1) : (sbyte)1);


                // Call the camera's behavior.

                currentCamera.ApplyBehavior(ref cameraPositionTarget, ref cameraLookAtTarget, player);


                // Do Direction Check. 

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



                //PluginLog.Log("ActionCameraDirector", "SidedActionCamera, checking side. " + player.headRRadialDelta.x + " " + estimatedSide);
                if (!player.swappingSides
                    && Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.cameraShoulderSensitivity &&
                    player.timerHelper.cameraActionTimer > currentCamera.GetBetweenTime() &&
                    estimatedSide != player.currentSide)
                {
                    Debug.Log("ActionCameraDirector: Swapping " + estimatedSide);
                    // FullBodyActionCamera.SwapSides(estimatedSide);


                    player.currentSide = estimatedSide;
                    player.swappingSides = true;
                    player.timerHelper.ResetCameraActionTimer();
                    distanceFromTargetSwap = (cameraPosition - currentCamera.relativeTo).magnitude;
                }
                else if (player.swappingSides && player.timerHelper.cameraActionTimer > currentCamera.GetBetweenTime())
                {

                    Debug.Log("ActionCameraDirector: End Swap");
                    player.swappingSides = false;
                    player.timerHelper.ResetCameraActionTimer();
                }

                Vector3 newPosition;
                if (!pluginSettings.linearCameraMovement // LinearMovement is not preferred.
                    && player.swappingSides // Is Currently Swapping sides
                    && (currentCamera.Equals(OverShoulderCamera) || currentCamera.Equals(FullBodyActionCamera))
                    && player.timerHelper.cameraTimer >= currentCamera.GetBetweenTime() // Is also currently not swapping cameras, just sides
                    && ((lastCamera != null && !lastCamera.inAvatar) || lastCamera == null))  // is Not transitioning from an fps camera outwards.
                {
                    Vector3 linearCameraPosition = Vector3.SmoothDamp(cameraPosition, cameraPositionTarget, ref cameraVelocity, currentCamera.GetBetweenTime());
                    // Do interpolation between current point and next point

                    Vector3 relativePosition = linearCameraPosition - currentCamera.relativeTo;

                    newPosition = currentCamera.relativeTo;
                    newPosition += CameraUtility.ClampToCircle(relativePosition, distanceFromTargetSwap);
                    newPosition.y = Mathf.SmoothDamp(cameraPosition.y, cameraPositionTarget.y, ref cameraVelocity.y, currentCamera.GetBetweenTime());
                    cameraPosition = newPosition;

                }
                else
                {
                    cameraPosition = Vector3.SmoothDamp(cameraPosition, cameraPositionTarget, ref cameraVelocity, currentCamera.GetBetweenTime());
                }

                if (this.pluginSettings.alwaysHaveAvatarInFrame && lastCamera != null)
                {
                    float distance = Mathf.Clamp(Vector3.Distance(cameraPositionTarget, cameraPosition) / (pluginSettings.cameraBodyDistance), 0f, 1f);

                    if (!lastCamera.facingAvatar && currentCamera.facingAvatar)
                    {
                        // Look at the player as soon as possible.
                        cameraLookAt = Vector3.SmoothDamp(cameraLookAt, cameraLookAtTarget, ref cameraLookAtVelocity, currentCamera.GetBetweenTime() / 4);
                    }
                    else
                    {
                        cameraLookAt = Vector3.SmoothDamp(cameraLookAt, Vector3.Slerp(cameraLookAtTarget, prevCameraLookAtTarget, distance), ref cameraLookAtVelocity, currentCamera.GetBetweenTime());
                    }
                }
                else
                {

                    // If las
                    cameraLookAt = Vector3.SmoothDamp(cameraLookAt, cameraLookAtTarget, ref cameraLookAtVelocity, currentCamera.GetBetweenTime());
                }

                Vector3 distanceFromHead = cameraPosition - player.head.position;
                distanceFromHead.y = 0;

                if (distanceFromHead.magnitude < pluginSettings.minimumCameraDistance && !currentCamera.inAvatar && lastCamera != null && !lastCamera.inAvatar)
                {
                    Debug.Log("ActionCameraDirector: In Avatar Head. Aaah.");
                    // Clamp to a circle
                    newPosition = player.head.position + CameraUtility.ClampToCircle(distanceFromHead, pluginSettings.minimumCameraDistance);
                    newPosition.y = cameraPosition.y;
                    cameraPosition = newPosition;
                }



                Vector3 lookDirection = cameraLookAt - cameraPosition;
                Quaternion rotation = currentCamera.GetRotation(lookDirection, player);



                /*
                simulatedCamera.transform.position = cameraPosition;
                simulatedCamera.transform.rotation = rotation;
                simulatedCamera.fieldOfView = currentCamera.GetFOV();
                */

                cameraHelper.UpdateCameraPose(cameraPosition, rotation, pluginSettings.cameraDefaultFov);
            }
        }
    }
}