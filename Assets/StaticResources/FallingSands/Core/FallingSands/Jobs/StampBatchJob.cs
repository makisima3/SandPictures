namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;

    [BurstCompile]
    public struct StampBatchJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<FallingData> DataIn;
        [WriteOnly] public NativeArray<FallingData> DataOut;

        [WriteOnly] public NativeArray<float> TemperatureOut;
        [ReadOnly] public NativeArray<float> TemperatureIn;

        [DeallocateOnJobCompletion, NativeDisableParallelForRestriction] public NativeArray<StampData> BatchedStampData;
        public int BatchedStampDataCount;

        public int textureWidth;
        public int textureHeight;
        public float2 inverseTextureRes;

        public void Execute(int index)
        {
            int2 id = MathE.Get1DTo2D(index, textureWidth, textureHeight);

            // write in case nothing else does.. 
            TemperatureOut[index] = TemperatureIn[index];
            DataOut[index] = DataIn[index];

            // iterate over batch and find something within range of this pixel 
            var count = BatchedStampDataCount;
            for (var i = 0; i < count; ++i)
            {
                var stampData = BatchedStampData[i];

                var vec = (id - stampData.position);
                if(math.length(vec) < stampData.radius)
                {
                    if(!stampData.overwriteAnything)
                    {
                        FallingData prevData = DataIn[index];
                        if(prevData.GetDataType() != FallingDataType.Air)
                        {
                            continue;
                        }
                    }

                    DataOut[index] = stampData.id;
                    TemperatureOut[index] = stampData.temperature;
                }
            }
        }
    }
}