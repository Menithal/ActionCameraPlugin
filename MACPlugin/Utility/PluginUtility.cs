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
#if !SIMPLIFIED
namespace MACPlugin
#else
namespace SimpleMacPlugin
#endif
{
    static class PluginUtility
    {
        public static Quaternion DeltaRotation(Quaternion from, Quaternion to)
        {
            return Quaternion.Inverse(to) * from;
        }
        public static Vector3 GetDeltaEulerRotation(Quaternion from, Quaternion to, float deltaTime)
        {
            float angle = 0f;
            Vector3 axis = Vector3.zero;
            DeltaRotation(from, to).ToAngleAxis(out angle, out axis);
            return (axis * angle) / deltaTime;
        }

        public static void TransformClone(ref Transform A, Transform B)
        {
            A.position = B.position;
            A.rotation = B.rotation;
        }
        public static float AverageCosAngleOfControllers(Transform rightHand, Transform leftHand, Vector3 direction)
        {
            float rightHandAngle = GetConeAngle(rightHand.position, direction, rightHand.forward);
            float leftHandAngle = GetConeAngle(leftHand.position, direction, -leftHand.forward);

            return Rad2Deg((leftHandAngle + rightHandAngle) / 2);
        }

        public static float GetConeAngle(Vector3 source, Vector3 target, Vector3 axis)
        {
            Vector3 direction = target - source;
            return Mathf.Abs(Vector3.Dot(direction.normalized, axis));
        }

        public static float Deg2Rad(float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }
        public static float Rad2Deg(float rads)
        {
            return rads * Mathf.Rad2Deg;
        }
    }
}
