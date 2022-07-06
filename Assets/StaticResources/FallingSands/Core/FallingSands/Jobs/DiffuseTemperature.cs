namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct DiffuseTemperature : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> TemperatureIn;
        [WriteOnly] public NativeArray<float> TemperatureOut;

        public int textureWidth;
        public int textureHeight;

        public void Execute(int index)
        {
            var pos = MathE.Get1DTo2D(index, textureWidth, textureHeight);

            var toAdd = 0f;

            toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(+0, +1), textureWidth, textureHeight)];
            toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(+1, +0), textureWidth, textureHeight)];
            toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(+0, -1), textureWidth, textureHeight)];
            toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(-1, +0), textureWidth, textureHeight)];
            // toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(+1, +1), textureWidth, textureHeight)];
            // toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(+1, -1), textureWidth, textureHeight)];
            // toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(-1, +1), textureWidth, textureHeight)];
            // toAdd += TemperatureIn[MathE.Get2DTo1D(pos + new int2(-1, -1), textureWidth, textureHeight)];

            var value = toAdd / 4f;

            TemperatureOut[index] = value;
        }

    }
}