using UnityEngine;

namespace Plugins.RobyyUtils
{
    public static class TransformExt
    {
        public static float DistanceTo(this Transform t1, Vector3 position)
        {
            return Vector3.Distance(t1.position, position);
        }

        public static float DistanceTo(this Transform t1, Transform t2)
        {
            return t1.DistanceTo(t2.position);
        }

        public static Vector3 DirectionTo(this Transform t1, Vector3 position, bool nonNormalized=false)
        {
            var direction = position - t1.position;

            return nonNormalized ? direction : direction.normalized;
        }

        public static Vector3 DirectionTo(this Transform t1, Transform t2, bool nonNormalized = false)
        {
            return t1.DirectionTo(t2.position, nonNormalized);
        }
    }
}