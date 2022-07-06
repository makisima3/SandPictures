namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Jobs;
    using Unity.Burst;
    using Unity.Mathematics;
    using Unity.Collections;

    [BurstCompile]
    public struct TranslateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> TemperatureIn;
        [NativeDisableParallelForRestriction] public NativeArray<float> TemperatureOut;

        [ReadOnly] public NativeArray<FallingData> DataIn;
        [NativeDisableParallelForRestriction] public NativeArray<FallingData> DataOut;

        public int2 translate;
        public int2 resolution;

        public void Execute(int index)
        {
            int2 oldId = MathE.Get1DTo2D(index, resolution.x, resolution.y);
            int2 newId = oldId - translate;

            var newIndex = MathE.Get2DTo1D(newId, resolution.x, resolution.y);

            if(newIndex < 0 || newIndex >= resolution.x * resolution.y)
            {
                return;
            }

            DataOut[newIndex] = DataIn[index];
            TemperatureOut[newIndex] = TemperatureIn[index];
        }
    }
}