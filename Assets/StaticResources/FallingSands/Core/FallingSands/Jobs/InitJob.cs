namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct InitJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<FallingData> DataOut;
        [WriteOnly] public NativeArray<float> TemperatureOut;

        public int textureWidth;
        public int textureHeight;

        public void Execute(int index)
        {
            TemperatureOut[index] = 0f;
            DataOut[index] = new FallingData();

            var pos = MathE.Get1DTo2D(index, textureWidth, textureHeight);

            // floor 
            if (pos.y == 0)
            {
                DataOut[index] = new FallingData(FallingDataType.Stone);
            }

            // walls 
            if (pos.x == 0 || pos.x == textureWidth - 1)
            {
                DataOut[index] = new FallingData(FallingDataType.Stone);
            }
        }
    }
}