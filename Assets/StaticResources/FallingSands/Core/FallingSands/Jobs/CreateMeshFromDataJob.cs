namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    // mesh stuff 
    [BurstCompile]
    public struct CreateMeshFromDataJob : IJob
    {
        [DeallocateOnJobCompletion] public NativeArray<bool> MeshedData;
        [ReadOnly] public NativeArray<FallingData> DataTex;
        [ReadOnly] public NativeArray<FallingDataMetadata> Metadata;

        public int2 resolution;

        [WriteOnly] public NativeList<Vector3> verts;
        [WriteOnly] public NativeList<Vector3> normals;
        [WriteOnly] public NativeList<int> tris;
        [WriteOnly] public NativeArray<Bounds> bounds;

        public void Execute()
        {
            var vertexIndex = 0;
            var meshBounds = new Bounds();

            var wRatio = (float)resolution.x / resolution.y;
            var scaleVertexPositions = new Vector3(wRatio / resolution.x, 1f / resolution.y, 32f / resolution.y);

            var volume = resolution.x * resolution.y;
            for (var i = 0; i < volume; ++i)
            {
                // skip over already meshed data 
                if (MeshedData[i])
                {
                    continue;
                }

                FallingData data = DataTex[i];
                var metadata = Metadata[(int)data.GetDataType()];

                MeshedData[i] = true;

                if (!metadata.hasSolidCollision)
                {
                    continue;
                }

                var rawPosition2d = MathE.Get1DTo2D(i, resolution.x, resolution.y);
                var position2d = rawPosition2d - resolution / 2;
                var vertexPosition = new Vector3(position2d.x, position2d.y, 0f);

                // start of bounds 
                var cubeBounds = new Bounds();
                cubeBounds.SetMinMax(vertexPosition, vertexPosition + new Vector3(1, 1, 1));

                var greedyMeshX = rawPosition2d.x;

                // greedy mesh along x axis 
                // from testing, setting a limit gets better results
                var remainingX = resolution.x - rawPosition2d.x;
                var greedyMeshUntilX = rawPosition2d.x + remainingX / 16;

                for (var x = rawPosition2d.x + 1; x < greedyMeshUntilX; ++x)
                {
                    var neighborIndex = MathE.Get2DTo1D(new int2(x, rawPosition2d.y), resolution.x, resolution.y);

                    if (MeshedData[neighborIndex])
                    {
                        break;
                    }

                    FallingData neighborData = DataTex[neighborIndex];
                    var neighborMetadata = Metadata[(int)neighborData.GetDataType()];

                    if (!neighborMetadata.hasSolidCollision)
                    {
                        break;
                    }

                    // extend x
                    cubeBounds.max = new Vector3(cubeBounds.max.x + 1, cubeBounds.max.y, cubeBounds.max.z);

                    MeshedData[neighborIndex] = true;

                    greedyMeshX = x;
                }

                // greedy mesh along the y axis 
                for (var y = rawPosition2d.y + 1; y < resolution.y; ++y)
                {
                    // if a whole row is not blocked, extend the greedy mesh upwards 
                    var broken = false;

                    for (var x = rawPosition2d.x; x <= greedyMeshX; ++x)
                    {
                        var neighborIndex = MathE.Get2DTo1D(new int2(x, y), resolution.x, resolution.y);

                        if (MeshedData[neighborIndex])
                        {
                            broken = true;
                            break;
                        }

                        FallingData neighborData = DataTex[neighborIndex];
                        var neighborMetadata = Metadata[(int)neighborData.GetDataType()];

                        if (!neighborMetadata.hasSolidCollision)
                        {
                            broken = true;
                            break;
                        }
                    }

                    if (broken)
                    {
                        break;
                    }

                    // extend y
                    cubeBounds.max = new Vector3(cubeBounds.max.x, cubeBounds.max.y + 1, cubeBounds.max.z);

                    // go back and mark the last y row as used
                    for (var x = rawPosition2d.x; x <= greedyMeshX; ++x)
                    {
                        var neighborIndex = MathE.Get2DTo1D(new int2(x, y), resolution.x, resolution.y);
                        MeshedData[neighborIndex] = true;
                    }
                }

                MeshBounds(cubeBounds, scaleVertexPositions, ref vertexIndex, ref meshBounds);
            }

            bounds[0] = meshBounds;
        }

        private void MeshBounds(Bounds bounds, Vector3 scaleVertexPositions, ref int vertexIndex, ref Bounds meshBounds)
        {
            var boundsStart = bounds.min;
            var boundsSize = bounds.size;

            var up = new Vector3(0, boundsSize.y, 0);
            var right = new Vector3(boundsSize.x, 0, 0);
            var forward = new Vector3(0, 0, boundsSize.z);

            var vert000 = boundsStart;
            var vert100 = boundsStart + right;
            var vert110 = boundsStart + right + up;
            var vert010 = boundsStart + up;

            var vert001 = vert000 + forward;
            var vert101 = vert100 + forward;
            var vert111 = vert110 + forward;
            var vert011 = vert010 + forward;

            vert000 = Vector3.Scale(vert000, scaleVertexPositions);
            vert100 = Vector3.Scale(vert100, scaleVertexPositions);
            vert110 = Vector3.Scale(vert110, scaleVertexPositions);
            vert010 = Vector3.Scale(vert010, scaleVertexPositions);

            vert001 = Vector3.Scale(vert001, scaleVertexPositions);
            vert101 = Vector3.Scale(vert101, scaleVertexPositions);
            vert111 = Vector3.Scale(vert111, scaleVertexPositions);
            vert011 = Vector3.Scale(vert011, scaleVertexPositions);

            verts.Add(vert000);
            verts.Add(vert100);
            verts.Add(vert110);
            verts.Add(vert010);
            verts.Add(vert001);
            verts.Add(vert101);
            verts.Add(vert111);
            verts.Add(vert011);

            // generate normals
            var normal000 = new Vector3(0, -1, 0);
            var normal100 = new Vector3(0, -1, 0);
            var normal001 = new Vector3(0, -1, 0);
            var normal110 = new Vector3(0, 1, 0);
            var normal010 = new Vector3(0, 1, 0);
            var normal101 = new Vector3(0, 1, 0);
            var normal111 = new Vector3(0, 1, 0);
            var normal011 = new Vector3(0, 1, 0);

            normals.Add(normal000);
            normals.Add(normal100);
            normals.Add(normal110);
            normals.Add(normal010);
            normals.Add(normal001);
            normals.Add(normal101);
            normals.Add(normal111);
            normals.Add(normal011);

            // generate triangles 
            var vi000 = vertexIndex + 0;
            var vi100 = vertexIndex + 1;
            var vi110 = vertexIndex + 2;
            var vi010 = vertexIndex + 3;
            var vi001 = vertexIndex + 4;
            var vi101 = vertexIndex + 5;
            var vi111 = vertexIndex + 6;
            var vi011 = vertexIndex + 7;

            // front 
            tris.Add(vi110);
            tris.Add(vi100);
            tris.Add(vi000);

            tris.Add(vi010);
            tris.Add(vi110);
            tris.Add(vi000);

            // back 
            tris.Add(vi001);
            tris.Add(vi101);
            tris.Add(vi111);

            tris.Add(vi001);
            tris.Add(vi111);
            tris.Add(vi011);

            // bottom 
            tris.Add(vi101);
            tris.Add(vi001);
            tris.Add(vi000);

            tris.Add(vi100);
            tris.Add(vi101);
            tris.Add(vi000);

            // top
            tris.Add(vi010);
            tris.Add(vi011);
            tris.Add(vi111);

            tris.Add(vi010);
            tris.Add(vi111);
            tris.Add(vi110);

            // left 
            tris.Add(vi000);
            tris.Add(vi001);
            tris.Add(vi011);

            tris.Add(vi000);
            tris.Add(vi011);
            tris.Add(vi010);

            // right 
            tris.Add(vi111);
            tris.Add(vi101);
            tris.Add(vi100);

            tris.Add(vi110);
            tris.Add(vi111);
            tris.Add(vi100);

            vertexIndex += 8;

            meshBounds.min = Vector3.Min(meshBounds.min, vert000);
            meshBounds.max = Vector3.Max(meshBounds.max, vert000);
        }
    }
}