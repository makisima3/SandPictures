namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    // copies a rectangle of an input texture into a rectangle of an output texture
    [BurstCompile]
    public struct DataBlitJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<FallingData> InputData;
        [ReadOnly] public NativeArray<float> InputTemp;
        [NativeDisableParallelForRestriction] public NativeArray<FallingData> OutputData;
        [NativeDisableParallelForRestriction] public NativeArray<float> OutputTemp;

        public int2 copyRegionInputPos;
        public int2 copyRegionOutputPos;
        public int2 copyResolution;

        public int2 outputTextureResolution;
        public int2 inputTextureResolution;

        // iterating over copyResolution
        public void Execute(int index)
        {
            int2 copyRegionPos = MathE.Get1DTo2D(index, copyResolution.x, copyResolution.y);

            var inputTexturePos = copyRegionInputPos + copyRegionPos;
            var outputTexturePos = copyRegionOutputPos + copyRegionPos;

            if (inputTexturePos.x < 0 || inputTexturePos.x >= inputTextureResolution.x) return;
            if (inputTexturePos.y < 0 || inputTexturePos.y >= inputTextureResolution.y) return;
            if (outputTexturePos.x < 0 || outputTexturePos.x >= outputTextureResolution.x) return;
            if (outputTexturePos.y < 0 || outputTexturePos.y >= outputTextureResolution.y) return;

            var inputTextureIndex = MathE.Get2DTo1D(inputTexturePos, inputTextureResolution.x, inputTextureResolution.y);
            var outputTextureIndex = MathE.Get2DTo1D(outputTexturePos, outputTextureResolution.x, outputTextureResolution.y);

            OutputData[outputTextureIndex] = InputData[inputTextureIndex];
            OutputTemp[outputTextureIndex] = InputTemp[inputTextureIndex];
        }
    }
}