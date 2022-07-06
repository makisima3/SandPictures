namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct SampleRequestsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> Temperature;
        [ReadOnly] public NativeArray<FallingData> Data;
        [ReadOnly] public NativeArray<int2> SampleRequests;
        [WriteOnly] public NativeArray<SampleData> SampleResults;

        public int2 sampleResolution;

        public void Execute(int index)
        {
            var position = SampleRequests[index];

            var sampleIndex = MathE.Get2DTo1D(position, sampleResolution.x, sampleResolution.y);
            SampleResults[index] = new SampleData()
            {
                id = Data[sampleIndex].GetDataType(),
                temp = Temperature[sampleIndex],
            };
        }
    }
}