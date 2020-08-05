using UnityEngine;

namespace MACPlugin.Utility
{

    static class CameraUtil
    {
        public static Vector3 ClampToCircle(Vector3 position, float distance)
        {
            position.y = 0;
            Vector3 temp = position.normalized * distance;
            return temp;
        }
        public static Vector3 LookAtTarget(Quaternion rotation, Vector3 position, Vector3 currentTarget) => (rotation * (position - currentTarget));
    }
}
