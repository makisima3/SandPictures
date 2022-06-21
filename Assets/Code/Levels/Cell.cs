using UnityEngine;

namespace Code.Levels
{
    public class Cell
    {
        public bool IsSpawned { get; set; }
        public bool IsEmpty { get; set; }
        public Vector2Int Position { get; set; }
        public Color Color;
    }
}