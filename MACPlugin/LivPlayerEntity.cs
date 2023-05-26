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
    public class LivPlayerEntity
    {
        private PluginCameraHelper pluginCameraHelper;
        public Transform waist { get { return pluginCameraHelper.playerHead; } }
        public Transform rightHand { get { return pluginCameraHelper.playerRightHand; } }
        public Transform leftHand { get { return pluginCameraHelper.playerLeftHand; } }
        public Transform head { get { return pluginCameraHelper.playerHead; } }
        public Transform leftFoot { get { return pluginCameraHelper.playerLeftFoot; } }
        public Transform rightFoot { get { return pluginCameraHelper.playerRightFoot; } }

        public Vector3 handAverage { get { return ((rightHand.position + leftHand.position) / 2); } } 
        public Vector3 leftEye { get { return pluginCameraHelper.playerLeftEyePosition; } }
        public Vector3 rightEye { get { return pluginCameraHelper.playerRightEyePosition; } }
        // TODO: Probably better in the future to actually look into ANGULAR velocities, instead of cheaping it out like this.
        public Vector3 headRRadialDelta { get; private set; } = Vector3.zero;
        public Vector3 leftHandRRadialDelta { get; private set; } = Vector3.zero;
        public Vector3 rightHandRRadialDelta { get; private set; } = Vector3.zero;

        public TimerHelper timerHelper { get; private set; }

        public LivPlayerEntity(PluginCameraHelper helper, ref TimerHelper timerHelper)
        {
            this.pluginCameraHelper = helper;
            this.timerHelper = timerHelper;
        }

        private Vector3 preCalculatedAbovePlayer;
        private Vector3 preCalculatedBelowPlayer;
        private Vector3 preCalculatedLeftHeadDirection;
        private Vector3 preCalculatedRightHeadDirection;
        private Vector3 preCalculatedAboveHeadDirection;
        private Vector3 preCalculatedBelowHeadDirection;
        private Vector3 preCalculatedFrontDirection;

        // Smooth Damp Used values, which are predetermiend with the offsets.
        public Vector3 headAboveDirection { get; private set; } = Vector3.zero;
        public Vector3 headBelowDirection { get; private set; } = Vector3.zero;
        // The following will ALWAYS be forward relative. These are just to check where the player is currently pointing towards 
        // Users tend to be looking down if playinga  shooter already, thus the vertical offset up..

        public Vector3 headForwardRightDirection { get; private set; } = Vector3.zero;
        public Vector3 headForwardLeftDirection { get; private set; } = Vector3.zero;
        // Saving these for probably later, when I figure in what cases could these be useful.
        public Vector3 headForwardUpDirection { get; private set; } = Vector3.zero;
        public Vector3 headForwardDownDirection { get; private set; } = Vector3.zero;

        public Vector3 headForwardDirection { get; private set; } = Vector3.zero;
        public Vector3 leftHandForwardDirection { get; private set; } = Vector3.zero;
        public Vector3 rightHandForwardDirection { get; private set; } = Vector3.zero;

        private Vector3 dampedCameraForwardVelocity = Vector3.zero;
        private Vector3 dampedRightHandForwardVelocity = Vector3.zero;
        private Vector3 dampedLeftHandForwardVelocity = Vector3.zero;

        private Vector3 dampedForwardCamera = Vector3.zero;
        private Vector3 dampedRightHand = Vector3.zero;
        private Vector3 dampedLeftHand = Vector3.zero;

        public void SetOffsets(float horizontalOffset, float verticalOffset, float distance)
        {
            preCalculatedAbovePlayer = new Vector3(0, verticalOffset, 0);
            preCalculatedBelowPlayer = -preCalculatedAbovePlayer;
            preCalculatedLeftHeadDirection = new Vector3(horizontalOffset, verticalOffset, distance);
            preCalculatedRightHeadDirection = new Vector3(-horizontalOffset, verticalOffset, distance);
            preCalculatedAboveHeadDirection = new Vector3(0, verticalOffset, distance);
            preCalculatedBelowHeadDirection = new Vector3(0, -verticalOffset, distance);
            preCalculatedFrontDirection = new Vector3(0, 0, distance);
        }
        public void CalculateInfo()
        {
            // Relative Directions from the current position of the head, but not including rotation.
            headAboveDirection = head.position + preCalculatedAbovePlayer;
            headBelowDirection = head.position + preCalculatedBelowPlayer;

            // The following will ALWAYS be forward relative. These are just to check where the player is currently pointing towards 
            // Users tend to be looking down if playinga  shooter already, thus the vertical offset up..
            headForwardRightDirection = head.TransformPoint(preCalculatedLeftHeadDirection);
            headForwardLeftDirection = head.TransformPoint(preCalculatedRightHeadDirection);

            // Saving these for probably later, when I figure in what cases could these be useful.
            headForwardUpDirection = head.TransformPoint(preCalculatedAboveHeadDirection);
            headForwardDownDirection = head.TransformPoint(preCalculatedBelowHeadDirection);

            headForwardDirection = head.TransformPoint(preCalculatedFrontDirection);
            rightHandForwardDirection = rightHand.TransformPoint(preCalculatedFrontDirection);
            leftHandForwardDirection = leftHand.TransformPoint(preCalculatedFrontDirection);

            // Trying to make this is as simple as possible: Check the distance between the last damped location and the current one, relative to the head rotation.
            // RRadia being "Relative Radial", as thats what it... technically is. Its not TRUE radial velocity, but its the difference between the damped and the current values
            headRRadialDelta = head.InverseTransformPoint(headForwardDirection) - head.InverseTransformPoint(dampedForwardCamera);
            leftHandRRadialDelta = leftHand.InverseTransformPoint(rightHandForwardDirection) - leftHand.InverseTransformPoint(dampedLeftHand);
            rightHandRRadialDelta = rightHand.InverseTransformPoint(leftHandForwardDirection) - rightHand.InverseTransformPoint(dampedRightHand);

            dampedLeftHand = Vector3.SmoothDamp(dampedLeftHand, leftHandForwardDirection, ref dampedLeftHandForwardVelocity, 0.3f);
            dampedRightHand = Vector3.SmoothDamp(dampedRightHand, rightHandForwardDirection, ref dampedRightHandForwardVelocity, 0.3f);
            dampedForwardCamera = Vector3.SmoothDamp(dampedForwardCamera, headForwardDirection, ref dampedCameraForwardVelocity, 0.3f);
        }
    }

    public class TimerHelper
    {
        public float globalTimer { get; private set; }
        public float controllerTimer { get; private set; }
        public float cameraTimer { get; private set; }
        public float cameraActionTimer { get; private set; }
        public float removeAvatarTimer { get; private set; }
        public float cameraGunTimer { get; private set; }

        public float manualCameraSeconds { get; private set; }

        public TimerHelper()
        {
            globalTimer = 0;
            cameraTimer = 0;
            cameraActionTimer = 0;
            removeAvatarTimer = 0;
            cameraGunTimer = 0;
            manualCameraSeconds = 0;
        }

        public void AddTime(float delta)
        {
            globalTimer += delta;
            controllerTimer += delta;
            cameraActionTimer += delta;
            cameraTimer += delta;
            removeAvatarTimer += delta;
            cameraGunTimer += delta;
            manualCameraSeconds += delta;
        }
        public void ResetGlobalCameraTimer()
        {
            globalTimer = 0;
        }
        public void ResetCameraTimer()
        {
            cameraTimer = 0;
        }

        public void SetGlobalTimer(float timer)
        {
            globalTimer = timer;
        }

        public void SetManualCameraSeconds(float seconds)
        {
            manualCameraSeconds = seconds;
        }

        public void ResetCameraActionTimer()
        {
            cameraActionTimer = 0;
        }
        public void ResetControllerTimer()
        {
            controllerTimer = 0;
        }
        public void ResetRemoveAvatarTimer()
        {
            removeAvatarTimer = 0;
        }
        public void ResetCameraGunTimer()
        {
            cameraGunTimer = 0;
        }

        public void ResetManualCameraSeconds()
        {
            manualCameraSeconds = 0;
        }
    }

}
