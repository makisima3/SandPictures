using Code.Levels;
using UnityEngine;

namespace Code.Utils
{
    public static class ImageConverter
    {
        public static Cell[,] GetCells(Texture2D texture)
        {
            var cells = new Cell[texture.width,texture.height];

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    cells[x, y] = new Cell()
                    {
                        Position = new Vector2Int(x, y),
                        Color = texture.GetPixel(x, y)
                    };
                }
            }

            return cells;
        }

        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4()
            {
                w = color.a,
                x = color.r,
                y = color.g,
                z = color.b
            };
        }

        public static float DistanceTo(this Vector4 vectorA, Vector4 vectorB)
        {
            return Vector4.Distance(vectorA, vectorB);
        }
    }
}