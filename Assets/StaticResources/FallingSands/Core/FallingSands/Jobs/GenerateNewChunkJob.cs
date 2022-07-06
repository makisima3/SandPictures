namespace CorgiFallingSands
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Jobs;
    using Unity.Burst;
    using Unity.Collections;

    [BurstCompile]
    public struct GenerateNewChunkJob : IJobParallelFor
    {
        public NativeArray<FallingData> DataTex;
        public NativeArray<float> TempTex;
        public int2 ChunkPosition;
        public int2 resolution;

        public void Execute(int index)
        {
            DataTex[index] = default;
            TempTex[index] = 0f;

            int2 positionWithinChunk = MathE.Get1DTo2D(index, resolution.x, resolution.y);
            int2 positionWithinWorld = ChunkPosition + positionWithinChunk;

            var targetType = FallingDataType.Air;

            var groundOffset = math.sin((positionWithinWorld.x + positionWithinWorld.y) * 0.01f) * 10;
            
            if (positionWithinWorld.y + groundOffset < 0)
            {
                targetType = FallingDataType.Grass;
            }
            
            if (positionWithinWorld.y + groundOffset < -4)
            {
                targetType = FallingDataType.Dirt;
            }

            if(positionWithinWorld.y + groundOffset < -16)
            {
                targetType = FallingDataType.Stone;
            }

            if(positionWithinWorld.y + groundOffset < -256)
            {
                targetType = FallingDataType.Bedrock;
            }

            // examples of some more complex stuff 
            // if (positionWithinWorld.y + groundOffset < -16)
            // {
            //     targetType = FallingDataType.Stone;
            // 
            //     var point = NoiseE.CellularPoint(positionWithinWorld, new float2(16, 16)); 
            // 
            //     if(math.abs(point.w - 0.90f) < 0.025f)
            //     {
            //         targetType = FallingDataType.Gold;
            //     }
            //     else if(math.abs(point.w - 0.75f) < 0.025f)
            //     {
            //         targetType = FallingDataType.DirtCompressed;
            //     }
            //     else if (math.abs(point.w - 0.75f) < 0.05f)
            //     {
            //         targetType = FallingDataType.Dirt;
            //     }
            //     else if (math.abs(point.w - 0.50f) < 0.025f)
            //     {
            //         targetType = FallingDataType.Water;
            //     }
            //     else if (math.abs(point.w - 0.25f) < 0.025f)
            //     {
            //         targetType = FallingDataType.Gravel;
            //     }
            // }

            DataTex[index] = new FallingData(targetType);
        }
    }
}