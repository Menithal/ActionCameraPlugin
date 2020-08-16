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
        public string name;
        protected float fov = 90;
        public Vector3 relativeTo { get; protected set; }
        private float timeBetweenChange = 1f;
        public bool removeHead = false;
        public bool inAvatar = false;
        public bool facingAvatar = false;


        protected static readonly sbyte POSITIVE_SBYTE = 1;
        protected static readonly sbyte NEGATIVE_SBYTE = -1;

        protected ActionCameraConfig pluginSettings;
        public Vector3 offset;
        public ActionCamera(ActionCameraConfig pluginSettings,
            float timeBetweenChange, Vector3 offset = new Vector3(),
            bool removeHead = false)
        {
            this.relativeTo = Vector3.zero;
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.removeHead = removeHead;
            this.facingAvatar = false;
            this.offset = offset;
        }
        public ActionCamera(ActionCameraConfig pluginSettings,
           float timeBetweenChange)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
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
        public virtual void SetPluginSettings(ActionCameraConfig settings)
        {
            pluginSettings = settings;
            fov = settings.cameraDefaultFov;
        }

        abstract public void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget,
            LivPlayerEntity player);

        public virtual Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.LookRotation(lookDirection);
        }
    }


    public class SimpleActionCamera : ActionCamera
    {
        public new string name = "SimpleActionCamera";
        public Vector3 lookAtOffset = new Vector3(0f, 0, 0.25f);
        public SimpleActionCamera(ActionCameraConfig settings, float timeBetweenChange, Vector3 offset, bool removeHead = false) :
            base(settings, timeBetweenChange, offset, removeHead)
        {

        }

        // Next to FPS Camera, simplest Camerda
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player)
        {
            cameraTarget = player.head.TransformPoint(offset);
            lookAtTarget = player.head.TransformPoint(lookAtOffset);
            //PluginLog.Log("SIMPLE CAMERA", "" + lookAtOffset);
            // average between Head and Waist to avoid head from flipping the camera around so much.s
            lookAtTarget = (lookAtTarget + player.head.TransformPoint(lookAtOffset)) / 2;

            if (pluginSettings.cameraVerticalLock)
            {
                lookAtTarget.y = player.chestEstimate.y;
            }

            cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);
        }
    }
    public class ShoulderActionCamera : ActionCamera
    {
        public new string name = "ShoulderActionCamera";
        // This one really needs an intermediary camera
        private readonly Vector3 lookAtOffset = new Vector3(0f, 0, 5f);
        // Predefining this to get around having to convert them.
        public ShoulderActionCamera(ActionCameraConfig settings) :
            base(settings, 0)
        {
            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y = -settings.cameraBodyVerticalTargetOffset;
            neutralOffset.z = -settings.cameraShoulderDistance;

            SetPluginSettings(settings);
        }
        public override void SetPluginSettings(ActionCameraConfig settings)
        {
            base.SetPluginSettings(settings);

            SetBetweenTime(settings.cameraShoulderPositioningTime);

            fov = settings.cameraDefaultFov;
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
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player)
        {

            /**
             * Logic for betweenCamera
             * When Camera is triggering swap sides (when player moves head fast enough, and the direction does not match previous)
             *  use only the betweenCamera behavior.
             *  once timeBetween Change has been fully realized, then start applying current behavior as planned.
            */




            Vector3 cameraOffsetTarget = offset;
            sbyte settingsReverse = (sbyte)(pluginSettings.reverseShoulder ? -1 : 1);

            cameraOffsetTarget.x = -player.currentSide * Mathf.Abs(cameraOffsetTarget.x) * settingsReverse;

            if (pluginSettings.cameraShoulderUseRoomOriginCenter)
            {
                cameraTarget = Vector3.zero;

                if (pluginSettings.cameraShoulderFollowGaze)
                {

                    cameraTarget += (player.head.rotation * cameraOffsetTarget);

                }
                else
                {

                    cameraTarget = cameraOffsetTarget;
                }

                cameraTarget.y = player.chestEstimate.y;
                relativeTo = new Vector3(player.head.position.x, player.chestEstimate.y, player.head.position.z);

            }
            else
            {
                if (pluginSettings.cameraShoulderFollowGaze)
                {
                    cameraTarget = player.head.TransformPoint(cameraOffsetTarget);
                }
                else
                {
                    cameraTarget = cameraOffsetTarget;
                    cameraTarget.y = player.head.position.y;
                }

                relativeTo = player.head.position;

            }

            // Floor and Ceiling Avoidance. Camera should not be too high or too low in ratio to player head position
            cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);


            if (pluginSettings.cameraShoulderFollowGaze)
            {
                lookAtTarget = player.head.TransformPoint(lookAtOffset);

                if (pluginSettings.cameraVerticalLock)
                {
                    lookAtTarget.y = player.chestEstimate.y;
                }
            }
            else
            {
                lookAtTarget = lookAtOffset;
                lookAtTarget.y = player.chestEstimate.y;
            }
        }

    }


    public class ADSCamera : SimpleActionCamera
    {
        public new string name = "ADSCamera";

        Transform dominantHand;
        Transform nonDominantHand;
        Vector3 dominantEye;
        Vector3 lookAtDirection;
        public ADSCamera(ActionCameraConfig settings) :
            base(settings, 0.2f, Vector3.zero, true)
        {
            SetPluginSettings(settings);
        }
        public override void SetPluginSettings(ActionCameraConfig settings)
        {

            inAvatar = true;
            removeHead = true;
            facingAvatar = false;

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
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player)
        {
            // Automatic determination which is closest?
            if (pluginSettings.rightHandDominant)
            {
                dominantHand = player.rightHand;
                nonDominantHand = player.leftHand;
            }
            else
            {
                dominantHand = player.leftHand;
                nonDominantHand = player.rightHand;
            }

            if (pluginSettings.rightEyeDominant)
            {
                dominantEye = player.rightEye;
            }
            else
            {
                dominantEye = player.leftEye;
            }

            float handDistance = Vector3.Distance(player.rightHand.position, player.leftHand.position);
            Vector3 handDirection = (nonDominantHand.position - dominantHand.position).normalized;

            if (handDistance < pluginSettings.cameraGunMinTwoHandedDistance * 1.2)
            {
                handDirection.y = handDirection.y * 0.5f;
                handDirection = handDirection.normalized;
            }

            //lookAtDirection = (nonDominantHand.position - new Vector3(0, pluginSettings.cameraGunEyeVerticalOffset, 0) - dominantHand.position);
            lookAtDirection = handDirection * 4f;
            // We will override the lookAtTarget and use GetRotation to define the actual rotation.
            lookAtTarget = Vector3.zero;
            cameraTarget = dominantEye;
        }
    }

    public class FPSCamera : SimpleActionCamera
    {
        public new string name = "FPSCamera";
        readonly ADSCamera sightsCamera;
        bool ironSightsEnabled;
        float blend = 0;
        public FPSCamera(ActionCameraConfig settings, float timeBetweenChange) : base(settings, timeBetweenChange, Vector3.zero, true)
        {
            sightsCamera = new ADSCamera(settings);
            ironSightsEnabled = false;

            inAvatar = true;
            removeHead = true;
            facingAvatar = false;
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
        public ADSCamera GetScope()
        {
            return sightsCamera;
        }
        public override void SetPluginSettings(ActionCameraConfig settings)
        {
            base.SetPluginSettings(settings);
            sightsCamera.SetPluginSettings(settings);

            inAvatar = true;
            removeHead = true;
            facingAvatar = false;

        }
        /*
         * 
         * Logic for Scope should only occur when 
         */
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player)
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
            bool isAimingTwoHandedForward = Mathf.Rad2Deg * PluginUtility.GetConeAngle(player.headBackwardDirection, averageHandPosition + handDirection * 1.2f, player.head.right) <
                    pluginSettings.cameraGunHeadAlignAngleTrigger * 0.9;


            bool snapToGun = Mathf.Abs(player.headRRadialDelta.x) < pluginSettings.controlMovementThreshold;

            if (snapToGun &&
                   isWithinTwoHandedUse && isHeadWithinAimingDistance && isAimingTwoHandedForward)
            {
                ironSightsEnabled = true;
                // Should have a smooth transition between Iron Sights and non iron sights.
                sightsCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player);
                blend += pluginSettings.cameraGunSmoothing * Time.deltaTime;
            }
            else
            {
                ironSightsEnabled = false;
                if (pluginSettings.useEyePosition)
                {
                    if (pluginSettings.rightEyeDominant)
                    {
                        cameraTarget = player.rightEye;
                    }
                    else
                    {
                        cameraTarget = player.leftEye;
                    }
                }
                else
                {
                    cameraTarget = player.head.TransformPoint(offset);
                }
                lookAtTarget = player.head.TransformPoint(lookAtOffset);



                blend -= 1 / pluginSettings.cameraGunSmoothing * Time.deltaTime;
            }
            relativeTo = player.head.position;
            blend = Mathf.Clamp(blend, 0, 1.0f);
        }

        public override Quaternion GetRotation(Vector3 lookDirection, LivPlayerEntity player)
        {
            return Quaternion.Slerp(Quaternion.LookRotation(player.head.forward, player.head.up), sightsCamera.GetRotation(lookDirection, player), blend);
        }
    }

    public class FullBodyActionCamera : ActionCamera
    {
        public new string name = "FullBodyActionCamera";
        // This one really needs an intermediary camera
        private Vector3 lookAtOffset = Vector3.zero;

        public FullBodyActionCamera(ActionCameraConfig settings) :
            base(settings, 0, Vector3.zero, false)
        {
            SetBetweenTime(settings.cameraBodyPositioningTime );
            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y += 0.5f;
            neutralOffset.z = settings.cameraBodyDistance;
            facingAvatar = true;
            CalculateOffset();
            SetPluginSettings(settings);
        }
        public void CalculateOffset()
        {

            float radianAngle = Mathf.Deg2Rad * pluginSettings.cameraBodyAngle;

            float y = pluginSettings.cameraBodyDistance * Mathf.Cos(radianAngle);
            float x = pluginSettings.cameraBodyDistance * Mathf.Sin(radianAngle);

            Vector3 calculatedOffset = new Vector3(x, 0.4f, y);

            //  calculatedOffset.z = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(offset.z, 2f) - Mathf.Pow(offset.x, 2f)));
            offset = calculatedOffset;
            lookAtOffset.y = pluginSettings.cameraBodyVerticalTargetOffset;
            lookAtOffset.z = pluginSettings.cameraBodyLookAtForward;
        }
        public override void SetPluginSettings(ActionCameraConfig settings)
        {
            base.SetPluginSettings(settings);
            SetBetweenTime(settings.cameraBodyPositioningTime);


            CalculateOffset();
            fov = settings.cameraDefaultFov;
        }
        public override float GetBetweenTime()
        {

            return base.GetBetweenTime();
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player)
        {

            Vector3 cameraPositionOffsetTarget = offset;


            sbyte settingsReverse = pluginSettings.reverseFBT ? NEGATIVE_SBYTE : POSITIVE_SBYTE;
            cameraPositionOffsetTarget.x = -player.currentSide * Mathf.Abs(cameraPositionOffsetTarget.x) * settingsReverse;

            if (pluginSettings.cameraBodyUseRoomOriginCenter)
            {
                cameraTarget = Vector3.zero;
                if (pluginSettings.cameraBodyFollowGaze)
                {
                    cameraTarget += (player.head.rotation * cameraPositionOffsetTarget);
                }
                else
                {
                    cameraTarget += cameraPositionOffsetTarget;
                }
                cameraTarget.y = (player.waist.position.y + player.head.position.y) / 2;


                relativeTo = new Vector3(player.head.position.x, cameraTarget.y, player.head.position.z);
            }
            else
            {
                cameraTarget = player.head.TransformPoint(cameraPositionOffsetTarget);
                relativeTo = player.head.position;
            }


            // Floor and Ceiling Avoidance. Camera should not be too high or too low in ratio to player head position
            cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);

            if (pluginSettings.cameraBodyUseRoomOriginCenter)
            {
                lookAtTarget = lookAtOffset;
                lookAtTarget.z = lookAtOffset.z;
                lookAtTarget.y = player.chestEstimate.y;
            }
            else
            {
                lookAtTarget = (player.waist.position + player.head.TransformPoint(lookAtOffset)) / 2;

                if (pluginSettings.cameraVerticalLock)
                {
                    lookAtTarget.y = player.chestEstimate.y;
                }
            }


        }
    }

    public class TopDownActionCamera : ActionCamera
    {
        public new string name = "TopDownActionCamera";
        public TopDownActionCamera(ActionCameraConfig settings, float timeBetweenChange, float distance) :
            base(settings, timeBetweenChange, new Vector3(0, distance, 0), false)
        {
            facingAvatar = true;
            inAvatar = true;
        }

        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player)
        {
            cameraTarget = player.head.position + offset;
            lookAtTarget = player.head.position;
            relativeTo = player.head.position;
        }
    }
}
