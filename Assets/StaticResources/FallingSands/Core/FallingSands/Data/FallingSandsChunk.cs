namespace CorgiFallingSands
{
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;

    [System.Serializable]
    public class FallingSandsChunk
    {
        public NativeArray<FallingData> DataTex;
        public NativeArray<float> TempTex;
        public int2 position;

        public FallingSandsChunk(int2 position, int2 resolution)
        {
            this.position = position;

            DataTex = new NativeArray<FallingData>(resolution.x * resolution.y, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            TempTex = new NativeArray<float>(resolution.x * resolution.y, Allocator.Persistent, NativeArrayOptions.ClearMemory); 
        }

        public void Dispose()
        {
            DataTex.Dispose();
            TempTex.Dispose();
        }
    }
}