namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct AddTemperatureFromData : IJobParallelFor
    {
        [ReadOnly] public NativeArray<FallingData> Data;
        [ReadOnly] public NativeArray<FallingDataMetadata> Metadata;

        [ReadOnly] public NativeArray<float> TemperatureIn;
        [WriteOnly] public NativeArray<float> TemperatureOut;

        public int textureWidth;
        public float deltaTime;
        public float dissipation; // include dt

        public void Execute(int index)
        {
            FallingData data = Data[index];
            var metadata = Metadata[(int)data.GetDataType()];
            TemperatureOut[index] = TemperatureIn[index] * (1f - dissipation) + metadata.temperature * deltaTime;
        }
    }
}