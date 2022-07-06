namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Many of these are borrowed from the following source:
    /// https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83
    /// </summary>
    public static class NoiseE
    {

        public static float Random(float n) { return math.frac(math.sin(n) * 43758.5453123f); }

        public static float Random(float2 n)
        {
            return math.frac(math.sin(math.dot(n, new float2(12.9898f, 4.1414f))) * 43758.5453f);
        }

        public static float Noise(float p)
        {
            float fl = math.floor(p);
            float fc = math.frac(p);
            return math.lerp(Random(fl), Random(fl + 1.0f), fc);
        }

        public static float Noise(float2 n)
        {
            float2 d = new float2(0.0f, 1.0f);
            float2 b = math.floor(n), f = math.smoothstep(new float2(0.0f, 0.0f), new float2(1.0f, 1.0f), math.frac(n));
            return math.lerp(math.lerp(Random(b), Random(b + d.yx), f.x), math.lerp(Random(b + d.xy), Random(b + d.yy), f.x), f.y);
        }

        public static unsafe float4 CellularPoint(float2 position, float2 gridSize)
        {
            var gridPos0 = math.floor(position / gridSize) * gridSize;
            var gridPos1 = math.floor((position - new float2(gridSize.x, 0f)) / gridSize) * gridSize;
            var gridPos2 = math.floor((position + new float2(gridSize.x, 0f)) / gridSize) * gridSize;
            var gridPos3 = math.floor((position + new float2(0f, gridSize.y)) / gridSize) * gridSize;
            var gridPos4 = math.floor((position - new float2(0f, gridSize.y)) / gridSize) * gridSize;

            gridPos0 += Random(gridPos0) * gridSize * 2.5f;
            gridPos1 += Random(gridPos1) * gridSize * 2.5f;
            gridPos2 += Random(gridPos2) * gridSize * 2.5f;
            gridPos3 += Random(gridPos3) * gridSize * 2.5f;
            gridPos4 += Random(gridPos4) * gridSize * 2.5f;

            var points = stackalloc float4[5];
                points[0] = new float4(gridPos0, 0, Random(gridPos0));
                points[1] = new float4(gridPos1, 0, Random(gridPos1));
                points[2] = new float4(gridPos2, 0, Random(gridPos2));
                points[3] = new float4(gridPos3, 0, Random(gridPos3));
                points[4] = new float4(gridPos4, 0, Random(gridPos4));

            float min_dist = float.MaxValue;
            float4 min_point = new float4(0f, 0f, 0f, 0f);

            for(var i = 0; i < 5; ++i)
            {
                var dist = math.length(points[i].xy - position.xy);
                if(dist < min_dist)
                {
                    min_dist = dist;
                    min_point = points[i];
                }
            }

            return min_point;
        }

    }
}