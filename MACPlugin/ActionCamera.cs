﻿/**  Copyright 2020 Matti 'Menithal' Lahtinen

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
        // Camera FOV
        public float fov = 70f;
        // Distance from Avatar Head
        public float distance = 5f;
        public float timeBetweenChange = 1f;
        public float horizontalOffset = 1f;
        public float verticalOffset = 1f;
        public bool removeHead = false;
        public bool staticCamera = false;

        protected static readonly sbyte POSITIVE_SBYTE = 1;
        protected static readonly sbyte NEGATIVE_SBYTE = -1;

        protected ActionCameraSettings pluginSettings;

        protected sbyte currentSide;
        public Vector3 offset { get; protected set; }
        public ActionCamera(ActionCameraSettings pluginSettings,
            float timeBetweenChange, Vector3 offset,
            bool removeHead = false, bool staticCamera = false)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.removeHead = removeHead;
            this.staticCamera = staticCamera;
            this.currentSide = 1;
            this.offset = offset;
        }
        public ActionCamera(ActionCameraSettings pluginSettings,
           float timeBetweenChange)
        {
            this.pluginSettings = pluginSettings;
            this.timeBetweenChange = timeBetweenChange;
            this.currentSide = 1;
        }

        public virtual void SetPluginSettings(ActionCameraSettings settings)
        {
            pluginSettings = settings;
        }

        abstract public void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget,
            LivPlayerEntity player, bool isCameraAlreadyPlaced);
    }

    public class SimpleActionCamera : ActionCamera
    {
        private readonly Vector3 lookAtOffset = new Vector3(0f, 0, 0.1f);
        public SimpleActionCamera(ActionCameraSettings settings, float timeBetweenChange, Vector3 offset, bool removeHead = false, bool staticCamera = false) :
            base(settings, timeBetweenChange, offset, removeHead, staticCamera)
        {
        }
        // Next to FPS Camera, simplest Camera
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            cameraTarget = player.head.TransformPoint(offset);
            lookAtTarget = player.head.TransformPoint(lookAtOffset);

            // average between Head and Waist to avoid head from flipping the camera around so much.s
            lookAtTarget = (lookAtTarget + player.waist.position) / 2;

            cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);
        }
    }
    public class ShoulderActionCamera : ActionCamera
    {
        // This one really needs an intermediary camera
        private readonly Vector3 lookAtOffset = new Vector3(0f, 0, 5f);
        private readonly SimpleActionCamera betweenCamera;
        private bool swappingSides;
        // Predefining this to get around having to convert them.
        public ShoulderActionCamera(ActionCameraSettings settings, Vector3 offset) :
            base(settings, settings.shoulderCameraPositioningTime)
        {
            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y += 1f;

            betweenCamera = new SimpleActionCamera(settings, settings.shoulderCameraPositioningTime / 2, neutralOffset);
            Vector3 calculatedOffset = offset;

            // Gotta be from back not from front. 
            calculatedOffset.z = -Mathf.Sqrt(Mathf.Abs(Mathf.Pow(offset.z, 2) - Mathf.Pow(offset.x, 2)));
            this.offset = calculatedOffset;

            PluginLog.Log("ShoulderActionCamera", "Calculated Offset " + calculatedOffset + " vs " + offset, true);

        }
        public override void SetPluginSettings(ActionCameraSettings settings)
        {
            pluginSettings = settings;
            this.timeBetweenChange = settings.shoulderCameraPositioningTime;
            betweenCamera.timeBetweenChange = settings.shoulderCameraPositioningTime / 2f;
            betweenCamera.SetPluginSettings(settings);

        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            Vector3 cameraOffsetTarget = offset;

            /**
             * Logic for betweenCamera
             * When Camera is triggering swap sides (when player moves head fast enough, and the direction does not match previous)
             *  use only the betweenCamera behavior.
             *  once timeBetween Change has been fully realized, then start applying current behavior as planned.
            */

            sbyte estimatedSide = (player.headRRadialDelta.x < 0 ? NEGATIVE_SBYTE : POSITIVE_SBYTE);
            if (!swappingSides && Mathf.Abs(player.headRRadialDelta.x) > 3 &&
                player.timerHelper.actionCameraTimer > this.timeBetweenChange / 2 &&
                estimatedSide != currentSide)
            {
                PluginLog.Log("ShoulderCamera", "Swapping sides " + estimatedSide, true);
                swappingSides = true;
                player.timerHelper.ResetActionCameraTimer();
            }
            else if (swappingSides && player.timerHelper.actionCameraTimer > this.timeBetweenChange / 2)
            {
                swappingSides = false;

                currentSide = estimatedSide;
                PluginLog.Log("ShoulderCamera", "Done Swapping", true);
                player.timerHelper.ResetActionCameraTimer();
            }

            if (swappingSides && pluginSettings.inBetweenCameraEnabled)
            {
                betweenCamera.ApplyBehavior(ref cameraTarget, ref lookAtTarget, player, isCameraAlreadyPlaced);
            }
            else
            {
                sbyte settingsReverse = pluginSettings.reverseShoulder ? NEGATIVE_SBYTE : POSITIVE_SBYTE;

                cameraOffsetTarget.x = -currentSide * Mathf.Abs(cameraOffsetTarget.x) * settingsReverse;
                cameraTarget = player.head.TransformPoint(cameraOffsetTarget);

                lookAtTarget = player.head.TransformPoint(lookAtOffset);

                // Floor and Ceiling Avoidance. Camera should not be too high or too low in ratio to player head position
                cameraTarget.y = Mathf.Clamp(cameraTarget.y, player.head.position.y * 0.2f, player.head.position.y * 1.2f);
            }
        }

    }

    public class FPSCamera : SimpleActionCamera
    {
        public FPSCamera(ActionCameraSettings settings, float timeBetweenChange) : base(settings, timeBetweenChange, Vector3.zero, true, false)
        {
        }
    }

    public class FullBodyActionCamera : ActionCamera
    {
        // This one really needs an intermediary camera
        private readonly Vector3 lookAtOffset = new Vector3(0f, 0f, 0.5f);
        private readonly SimpleActionCamera betweenCamera;
        private bool swappingSides = false;
        public FullBodyActionCamera(ActionCameraSettings settings, Vector3 offset, bool staticCamera = false) :
            base(settings, settings.bodyCameraPositioningTime, Vector3.zero, false, staticCamera)
        {

            Vector3 neutralOffset = offset;
            neutralOffset.x = 0;
            neutralOffset.y += 0.5f;
            betweenCamera = new SimpleActionCamera(settings, settings.bodyCameraPositioningTime / 2, neutralOffset);

            Vector3 calculatedOffset = offset;

            calculatedOffset.z = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(offset.z, 2f) - Mathf.Pow(offset.x, 2f)));
            PluginLog.Log("FullBodyActionCamera", "Calculated Position " + calculatedOffset, true);
            PluginLog.Log("FullBodyActionCamera", "Calculated Offset " + calculatedOffset + " vs " + offset, true);
            this.offset = calculatedOffset;
        }
        public override void SetPluginSettings(ActionCameraSettings settings)
        {
            pluginSettings = settings;
            this.timeBetweenChange = settings.bodyCameraPositioningTime / 2;
            betweenCamera.timeBetweenChange = settings.bodyCameraPositioningTime / 2f;
            betweenCamera.SetPluginSettings(settings);
        }
        public override void ApplyBehavior(ref Vector3 cameraTarget, ref Vector3 lookAtTarget, LivPlayerEntity player, bool isCameraAlreadyPlaced)
        {
            Vector3 cameraPositionOffsetTarget = offset;

            sbyte estimatedSide = (player.headRRadialDelta.x < 0 ? NEGATIVE_SBYTE : POSITIVE_SBYTE);
            if (!swappingSides && Mathf.Abs(player.headRRadialDelta.x) > 3 &&
                player.timerHelper.actionCameraTimer > this.timeBetweenChange / 2 &&
                estimatedSide != currentSide)
            {
                PluginLog.Log("FullBodyActionCamera", "Swapping sides " + estimatedSide, true);
                swappingSides = true;
                player.timerHelper.ResetActionCameraTimer();
            }
            else if (swappingSides && player.timerHelper.actionCameraTimer > this.timeBetweenChange / 2)
            {
                swappingSides = false;
                currentSide = estimatedSide;

                PluginLog.Log("FullBodyActionCamera", "Done Swapping ", true);
                player.timerHelper.ResetActionCameraTimer();
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