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
using UnityEngine;
namespace MACPlugin
{

    // Predefining this to get around having to convert them.
    public abstract class ActionCamera
    {
        // Do something with this in the future
        protected float fov = 90;
        private float timeBetweenChange = 1f;
        public bool removeHead = false;
        public bool staticCamera = false;

        protected static readonly sbyte POSITIVE_SBYTE = 1;
        protected static readonly sbyte NEGATIVE_SBYTE = -1;

        protected ActionCameraSettings pluginSettings;

        protected sbyte currentSide;
        protected sbyte destinationSide;
        public Vector3 offset;
        public ActionCamera(ActionCameraSettings pluginSettings,
            float timeBetweenChange, Vector3 offset = new Vector3(),
            bool removeHead = false, bool staticCamera = false)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.removeHead = removeHead;
            this.staticCamera = staticCamera;
            this.currentSide = 1;
            this.destinationSide = 1;
            this.offset = offset;
        }
        public ActionCamera(ActionCameraSettings pluginSettings,
           float timeBetweenChange)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.currentSide = 1;
        }
        public virtual float GetFOV()
        {
            return fov;
        }
        public void SetBetweenTime(float time)
        {
            timeBetweenChange = time;
        }
        public virtual float GetBetweenTime()
        {
            return timeBetweenChange;
        }
        public virtual void SetPluginSettings(ActionCameraSettings settings)
        {
            pluginSettings = settings;
            fov = settings.cameraDefaultFov;
        }

        abstract public void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget,
            LivPlayerEntity player, bool isCameraAlreadyPlaced);

        public virtual Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.LookRotation(lookDirection);
        }
    }

    public class SimpleActionCamera : ActionCamera
    {
        public Vector3 lookAtOffset = new Vector3(0f, 0, 0.25f);
        public SimpleActionCamera(ActionCameraSettings settings, float timeBetweenChange, Vector3 offset, bool removeHead = false, bool staticCamera = false) :
            base(settings, timeBetweenChange, offset, removeHead, staticCamera)
        {
        }

        // Next to FPS Camera, simplest Camerda
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            cameraTarget = player.head.TransformPoint(offset);
            lookAtTarget = player.head.TransformPoint(lookAtOffset);
            PluginLog.Log("SIMPLE CAMERA", ""+lookAtOffset);
            // average between Head and Waist to avoid head from flipping the camera around so much.s
            lookAtTarget = (lookAtTarget + player.waist.TransformPoint(lookAtOffset)) / 2;

            if (pluginSettings.cameraVerticalLock)
            {
                lookAtTarget.y = (player.waist.position.y + player.head.position.y) / 2;
            }

            cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);
        }
    }
    public class ScopeActionCamera : ActionCamera
    {
        public Vector3 lookAtOffset = new Vector3(0f, 0f, 0f);

        Transform dominantHand;
        Transform nonDominantHand;
        Vector3 dominantEye;
        Vector3 lookAtDirection;
        public ScopeActionCamera(ActionCameraSettings settings) :
            base(settings, 0.2f, Vector3.zero, true)
        {
            SetPluginSettings(settings);
        }
        public override void SetPluginSettings(ActionCameraSettings settings)
        {

            fov = settings.cameraGunFov;
            pluginSettings = settings;
            offset = new Vector3(0, -settings.cameraGunEyeVerticalOffset, 0);
            lookAtOffset.y = offset.y;
            SetBetweenTime(settings.cameraGunSmoothing);
        }

        public override Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.LookRotation(lookAtDirection, player.head.up);
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            // Automatic determination which is closest?
            if (pluginSettings.rightHandDominant)
            {
                dominantHand = player.rightHand;
                nonDominantHand = player.leftHand;
                dominantEye = player.rightEye;
            }
            else
            {
                dominantHand = player.leftHand;
                nonDominantHand = player.rightHand;
                dominantEye = player.leftEye;
            }

            lookAtDirection = (nonDominantHand.position - new Vector3(0, pluginSettings.cameraGunEyeVerticalOffset, 0) - dominantHand.position);
            // We will override the lookAtTarget and use GetRotation to define the actual rotation.
            lookAtTarget = Vector3.zero;
            cameraTarget = dominantEye;
        }
    }

    public class ShoulderActionCamera : ActionCamera
    {
        // This one really needs an intermediary camera
        private readonly Vector3 lookAtOffset = new Vector3(0f, 0, 5f);
        private readonly SimpleActionCamera betweenCamera;
        private bool swappingSides;
        // Predefining this to get around having to convert them.
        public ShoulderActionCamera(ActionCameraSettings settings) :
            base(settings, 0)
        {
            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y = -settings.cameraBodyVerticalTargetOffset;
            neutralOffset.z = -settings.cameraShoulderDistance;
            betweenCamera = new SimpleActionCamera(settings, settings.cameraShoulderPositioningTime / 2, neutralOffset);

            SetPluginSettings(settings);
        }
        public override void SetPluginSettings(ActionCameraSettings settings)
        {
            base.SetPluginSettings(settings);

            SetBetweenTime(settings.cameraShoulderPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            betweenCamera.SetBetweenTime(settings.cameraShoulderPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            betweenCamera.SetPluginSettings(settings);
            betweenCamera.offset = new Vector3(0, -settings.cameraBodyVerticalTargetOffset, -settings.cameraShoulderDistance);
            CalculateOffset();

        }
        public void CalculateOffset()
        {

            float radianAngle = Mathf.Deg2Rad * pluginSettings.cameraShoulderAngle;

            float y = pluginSettings.cameraShoulderDistance * Mathf.Cos(radianAngle);
            float x = pluginSettings.cameraShoulderDistance * Mathf.Sin(radianAngle);

            Vector3 calculatedOffset = new Vector3(x, 0.5f, -y);

            // PluginLog.Log("ShoulderActionCamera", "Calculated Offset " + calculatedOffset + " vs " + offset);
            // Gotta be from back not from front. 
            //  calculatedOffset.z = -Mathf.Sqrt(Mathf.Abs(Mathf.Pow(offset.z, 2) - Mathf.Pow(offset.x, 2)));
            this.offset = calculatedOffset;
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {

            /**
             * Logic for betweenCamera
             * When Camera is triggering swap sides (when player moves head fast enough, and the direction does not match previous)
             *  use only the betweenCamera behavior.
             *  once timeBetween Change has been fully realized, then start applying current behavior as planned.
            */

            sbyte estimatedSide = (player.headRRadialDelta.x < 0 ? NEGATIVE_SBYTE : POSITIVE_SBYTE);
            if (!swappingSides && Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.cameraShoulderSensitivity &&
                player.timerHelper.cameraActionTimer > GetBetweenTime() &&
                estimatedSide != currentSide)
            {
                PluginLog.Log("ShoulderCamera", "Swapping sides " + estimatedSide);
                swappingSides = true;
                destinationSide = estimatedSide;
                player.timerHelper.ResetCameraActionTimer();
            }
            else if (swappingSides && player.timerHelper.cameraActionTimer > GetBetweenTime())
            {
                swappingSides = false;
                currentSide = destinationSide;
                PluginLog.Log("ShoulderCamera", "Done Swapping");
                player.timerHelper.ResetCameraActionTimer();
            }

            if (swappingSides && pluginSettings.inBetweenCameraEnabled)
            {
                betweenCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player, isCameraAlreadyPlaced);
            }
            else
            {
                Vector3 cameraOffsetTarget = offset;
                sbyte settingsReverse = pluginSettings.reverseShoulder ? NEGATIVE_SBYTE : POSITIVE_SBYTE;

                cameraOffsetTarget.x = -currentSide * Mathf.Abs(cameraOffsetTarget.x) * settingsReverse;
                cameraTarget = player.head.TransformPoint(cameraOffsetTarget);

                // Floor and Ceiling Avoidance. Camera should not be too high or too low in ratio to player head position
                cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);

                lookAtTarget = player.head.TransformPoint(lookAtOffset);

                if (pluginSettings.cameraVerticalLock)
                {
                    lookAtTarget.y = (player.waist.position.y + player.head.position.y) / 2;
                }
            }
        }

    }

    public class FPSCamera : SimpleActionCamera
    {
        readonly ActionCamera sightsCamera;
        bool ironSightsEnabled;
        float blend = 0;
        public FPSCamera(ActionCameraSettings settings, float timeBetweenChange) : base(settings, timeBetweenChange, Vector3.zero, true, false)
        {
            sightsCamera = new ScopeActionCamera(settings);
            ironSightsEnabled = false;
        }

        public override float GetFOV()
        {
            if (pluginSettings.cameraFovLerp) return Mathf.Lerp(base.GetFOV(), sightsCamera.GetFOV(), blend);

            return ironSightsEnabled ? sightsCamera.GetFOV() : base.GetFOV();
        }

        public override float GetBetweenTime()
        {
            return Mathf.Lerp(base.GetBetweenTime(), sightsCamera.GetBetweenTime(), blend);
        }
        public ActionCamera GetScope()
        {
            return sightsCamera;
        }
        public override void SetPluginSettings(ActionCameraSettings settings)
        {
            base.SetPluginSettings(settings);
            sightsCamera.SetPluginSettings(settings);
        }
        /*
         * 
         * Logic for Scope should only occur when 
         */
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
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

            // If Hands close enough, and aligned, then do the thing, if not then
            float handDistance = Vector3.Distance(player.rightHand.position, player.leftHand.position);
            bool isWithinTwoHandedUse = handDistance > pluginSettings.cameraGunMinTwoHandedDistance && handDistance < pluginSettings.cameraGunMaxTwoHandedDistance;
            bool isHeadWithinAimingDistance = Vector3.Distance(averageHandPosition, player.head.position) < pluginSettings.cameraGunHeadDistanceTrigger;
            bool isAimingTwoHandedForward = Mathf.Rad2Deg * PluginUtility.GetConeAngle(player.head.position, averageHandPosition + handDirection * 4f, player.head.right) <
                    pluginSettings.cameraGunHeadAlignAngleTrigger;

            if (!pluginSettings.disableGunCamera && Mathf.Abs(player.headRRadialDelta.x) < pluginSettings.controlMovementThreshold / 2 &&
                   isWithinTwoHandedUse && isHeadWithinAimingDistance && isAimingTwoHandedForward)
            {
                ironSightsEnabled = true;
                // Should have a smooth transition between Iron Sights and non iron sights.
                sightsCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player, isCameraAlreadyPlaced);
                blend += 1/pluginSettings.cameraGunSmoothing * Time.deltaTime;
            }
            else
            {
                ironSightsEnabled = false;
                cameraTarget = player.head.TransformPoint(offset);
                lookAtTarget = player.head.TransformPoint(lookAtOffset);


                blend -= 1 / pluginSettings.cameraGunSmoothing * Time.deltaTime;
            }

            blend = Mathf.Clamp(blend, 0, 1.0f);
        }
    
        public override Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.Slerp(Quaternion.LookRotation(player.head.forward, player.head.up), sightsCamera.GetRotation(lookDirection, player), blend);
        }
    }

    public class FullBodyActionCamera : ActionCamera
    {
        // This one really needs an intermediary camera
        private Vector3 lookAtOffset = Vector3.zero;
        private readonly SimpleActionCamera betweenCamera;
        private bool swappingSides = false;

        public FullBodyActionCamera(ActionCameraSettings settings) :
            base(settings, 0, Vector3.zero, false, false)
        {
            SetBetweenTime(settings.cameraBodyPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y += 0.5f;
            neutralOffset.z = settings.cameraBodyDistance;
            betweenCamera = new SimpleActionCamera(settings, GetBetweenTime(), neutralOffset);

            CalculateOffset();
        }
        public void CalculateOffset()
        {

            float radianAngle = Mathf.Deg2Rad * pluginSettings.cameraBodyAngle;

            float y = pluginSettings.cameraBodyDistance * Mathf.Cos(radianAngle);
            float x = pluginSettings.cameraBodyDistance * Mathf.Sin(radianAngle);

            Vector3 calculatedOffset = new Vector3(x, 0.4f, y);

            //  calculatedOffset.z = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(offset.z, 2f) - Mathf.Pow(offset.x, 2f)));
            PluginLog.Log("FullBodyActionCamera", "Calculated Position " + calculatedOffset);
            offset = calculatedOffset;
            lookAtOffset.y = pluginSettings.cameraBodyVerticalTargetOffset;
            lookAtOffset.z = pluginSettings.cameraBodyLookAtForward;
        }
        public override void SetPluginSettings(ActionCameraSettings settings)
        {
            pluginSettings = settings;
            SetBetweenTime(settings.cameraBodyPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            betweenCamera.SetBetweenTime(settings.cameraBodyPositioningTime / (settings.inBetweenCameraEnabled ? 2 : 1));
            betweenCamera.SetPluginSettings(settings);

            betweenCamera.offset = new Vector3(0, 1f, settings.cameraBodyDistance);

            CalculateOffset();
        }
        public override float GetBetweenTime()
        {
            if (swappingSides)
            {
                return betweenCamera.GetBetweenTime();
            }
            return base.GetBetweenTime();
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            Vector3 cameraPositionOffsetTarget = offset;

            sbyte estimatedSide = (player.headRRadialDelta.x < 0 ? NEGATIVE_SBYTE : POSITIVE_SBYTE);
            if (!swappingSides && Mathf.Abs(player.headRRadialDelta.x) > pluginSettings.cameraBodySensitivity &&
                player.timerHelper.cameraActionTimer > GetBetweenTime() &&
                estimatedSide != currentSide)
            {
                PluginLog.Log("FullBodyActionCamera", "Swapping sides " + estimatedSide);
                swappingSides = true;
                destinationSide = estimatedSide;
                player.timerHelper.ResetCameraActionTimer();
            }
            else if (swappingSides && player.timerHelper.cameraActionTimer > GetBetweenTime())
            {
                swappingSides = false;
                currentSide = destinationSide;

                PluginLog.Log("FullBodyActionCamera", "Done Swapping ");
                player.timerHelper.ResetCameraActionTimer();
            }

            if (swappingSides && pluginSettings.inBetweenCameraEnabled)
            {
                betweenCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player, isCameraAlreadyPlaced);
            }
            else
            {
                sbyte settingsReverse = pluginSettings.reverseFBT ? NEGATIVE_SBYTE : POSITIVE_SBYTE;
                cameraPositionOffsetTarget.x = -currentSide * Mathf.Abs(cameraPositionOffsetTarget.x) * settingsReverse;

                cameraTarget = player.head.TransformPoint(cameraPositionOffsetTarget);

                // Floor and Ceiling Avoidance. Camera should not be too high or too low in ratio to player head position
                cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);


                lookAtTarget = (player.waist.position + player.head.TransformPoint(lookAtOffset)) / 2;

                if (pluginSettings.cameraVerticalLock)
                {
                    lookAtTarget.y = (player.waist.position.y + player.head.position.y) / 2;
                }
            }
        }
    }

    public class TopDownActionCamera : ActionCamera
    {
        public TopDownActionCamera(ActionCameraSettings settings, float timeBetweenChange, float distance) :
            base(settings, timeBetweenChange, new Vector3(0, distance, 0), false, false)
        {
        }

        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            cameraTarget = player.head.position + offset;
            lookAtTarget = player.head.position;
        }
    }
}
