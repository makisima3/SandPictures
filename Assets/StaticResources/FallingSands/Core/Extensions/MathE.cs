namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Mathematics;

    public static class MathE
    {
        public static int2 Get1DTo2D(int index, int width, int height)
        {
            var x = index % width;
            var y = index / width;

            var pos = new int2(x, y);

            if (pos.x < 0) pos.x = 0;
            if (pos.y < 0) pos.y = 0;

            if (pos.x > width - 1) pos.x = width - 1;
            if (pos.y > height - 1) pos.y = height - 1;

            return pos;
        }

        public static int Get2DTo1D(int2 pos, int width, int height)
        {
            if (pos.x < 0) pos.x = 0;
            if (pos.y < 0) pos.y = 0;
            if (pos.x > width - 1) pos.x = width - 1;
            if (pos.y > height - 1) pos.y = height - 1;

            int index = pos.x + width * pos.y;
            return math.clamp(index, 0, (width * height) - 1);
        }

    }
}