namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct ClearDataJob : IJobParallelFor
    {
        public NativeArray<FallingData> Data;
        public int width;
        public int height;

        public void Execute(int index)
        {
            Data[index] = new FallingData(FallingDataType.Air);
        }
    }

    [BurstCompile]
    public struct ClearTempJob : IJobParallelFor
    {
        public NativeArray<float> Temp;
        public int width;
        public int height;

        public void Execute(int index)
        {
            Temp[index] = 0f; 
        }
    }
}