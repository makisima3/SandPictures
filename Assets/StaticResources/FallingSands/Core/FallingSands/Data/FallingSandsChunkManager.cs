namespace CorgiFallingSands
{
    using Unity.Mathematics;
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class FallingSandsChunkManager
    {
        public Dictionary<int2, FallingSandsChunk> Chunks = new Dictionary<int2, FallingSandsChunk>();
        public int2 resolutionPerChunk;

        public FallingSandsChunkManager(int2 resolutionPerChunk)
        {
            this.resolutionPerChunk = resolutionPerChunk;
        }

        public int2 GetChunkPosFromWorldPos(int2 worldPos)
        {
            var chunkPos = worldPos;

            chunkPos.x = (int) Mathf.Floor((float)chunkPos.x / resolutionPerChunk.x) * resolutionPerChunk.x;
            chunkPos.y = (int) Mathf.Floor((float)chunkPos.y / resolutionPerChunk.y) * resolutionPerChunk.y;

            return chunkPos;
        }

        public FallingSandsChunk TryGetChunk(int2 chunkPos)
        {
            if (Chunks.TryGetValue(chunkPos, out FallingSandsChunk chunk))
            {
                return chunk;
            }
            else
            {
                return null;
            }
        }

        public FallingSandsChunk CreateChunkAt(int2 chunkPos)
        {
            var chunk = new FallingSandsChunk(chunkPos, resolutionPerChunk);
            Chunks.Add(chunkPos, chunk);
            return chunk;
        }

        public bool TryRemoveChunk(int2 chunkPos)
        {
            if(Chunks.TryGetValue(chunkPos, out FallingSandsChunk chunk))
            {
                chunk.Dispose(); 
            }

            return Chunks.Remove(chunkPos);
        }
    }
}