using UnityEngine;

namespace Plugins.RobyyUtils
{
    public static class GizmosExt
    {
        public static void DrawRect(Rect rect)
        {
            Gizmos.DrawWireCube(new Vector3(rect.center.x, rect.center.y, 0.01f), new Vector3(rect.size.x, rect.size.y, 0.01f));
        }
    }
}
