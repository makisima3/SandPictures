namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct StampJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<FallingData> DataOut;
        [WriteOnly] public NativeArray<float> TemperatureOut;
        [ReadOnly] public NativeArray<float> TemperatureIn;

        public FallingData StampData;
        public float StampTemperature;

        public float2 mousePosUv;
        public float mouseRadius;
        public int textureWidth;
        public int textureHeight;
        public float2 inverseTextureRes;

        public void Execute(int index)
        {
            int2 id = MathE.Get1DTo2D(index, textureWidth, textureHeight);
            float2 uv = id * inverseTextureRes;

            var fromMouse = (uv - mousePosUv);
            fromMouse.x *= (float)textureWidth / textureHeight;

            if (math.length(fromMouse) < mouseRadius)
            {
                DataOut[index] = StampData;
                TemperatureOut[index] = StampTemperature;
            }
            else
            {
                TemperatureOut[index] = TemperatureIn[index];
            }
        }
    }
}