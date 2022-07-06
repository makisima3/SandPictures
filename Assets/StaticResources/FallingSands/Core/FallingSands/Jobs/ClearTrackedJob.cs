namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;

    [BurstCompile]
    public struct ClearTrackedJob : IJobParallelFor
    {
        public NativeArray<bool> Tracked;

        public void Execute(int index)
        {
            Tracked[index] = false;
        }
    }
}