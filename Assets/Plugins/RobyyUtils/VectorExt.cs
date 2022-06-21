using UnityEngine;

namespace Plugins.RobyyUtils
{
    public static class VectorExt
    {
        public static Vector2 ToV2(this Vector3 v3) => new Vector2(v3.x, v3.y);
        
        //public static Vector2 ToV2Z(this Vector3 v3) => new Vector2(v3.x, v3.z);

        public static Vector3 ToV3(this Vector2 v2, float z = 0f) => new Vector3(v2.x, v2.y, z);
        

        public static Vector2 ToV2(this Vector2Int v2INT) => new Vector2(v2INT.x, v2INT.y);
        public static Vector3 ToV3(this Vector3Int v3INT) => new Vector3(v3INT.x, v3INT.y, v3INT.z);
        
        public static Vector2Int ToV2Int(this Vector3Int v3) => new Vector2Int(v3.x, v3.y);
        public static Vector3Int ToV3Int(this Vector2Int v2, int z=0) => new Vector3Int(v2.x, v2.y, z);

        public static Vector2Int RoundToInt(this Vector2 v2) => new Vector2Int()
        {
            x = Mathf.RoundToInt(v2.x), 
            y = Mathf.RoundToInt(v2.y)
        };

        public static Vector3Int RoundToInt(this Vector3 v3) => new Vector3Int()
        {
            x = Mathf.RoundToInt(v3.x),
            y = Mathf.RoundToInt(v3.y),
            z = Mathf.RoundToInt(v3.z)
        };
    }
}