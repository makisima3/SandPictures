namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [BurstCompile]
    public struct GeneratePolygon2DPoints : IJob
    {
        [DeallocateOnJobCompletion] public NativeArray<bool> MeshedData;

        [ReadOnly] public NativeArray<FallingData> DataTex;
        [ReadOnly] public NativeArray<FallingDataMetadata> Metadata;

        public NativeList<Vector2> Points;

#if UNITY_2021_2_OR_NEWER
        public NativeList<PhysicsShape2D> PhysicsShapes;
#endif


        public int2 resolution;

        public void Execute()
        {
            Points.Clear();

#if UNITY_2021_2_OR_NEWER
            PhysicsShapes.Clear();
#endif

            var wRatio = (float)resolution.x / resolution.y;
            var scaleVertexPositions = new Vector2(wRatio / resolution.x, 1f / resolution.y);

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
                var vertexPosition = new Vector2(position2d.x, position2d.y);

                // start of bounds 
                var cubeBounds = new Bounds();
                cubeBounds.SetMinMax(vertexPosition, vertexPosition + new Vector2(1, 1));

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

                MeshBounds(cubeBounds, scaleVertexPositions);
            }
        }

        private void MeshBounds(Bounds bounds, Vector2 scaleVertexPositions)
        {
            Vector2 boundsStart = bounds.min;
            Vector2 boundsSize = bounds.size;

            // invalid 
            if(boundsSize.magnitude < 0.001f || boundsSize.magnitude > 1000f || boundsSize.x <= 0.001f || boundsSize.y <= 0.001f)
            {
                return; 
            }

            // no small colliders.. 
            if(boundsSize.magnitude < 4f)
            {
                return;
            }

            var up = new Vector2(0, boundsSize.y);
            var right = new Vector2(boundsSize.x, 0);

            var vert000 = boundsStart;
            var vert100 = boundsStart + right;
            var vert110 = boundsStart + right + up;
            var vert010 = boundsStart + up;

            vert000 = Vector2.Scale(vert000, scaleVertexPositions);
            vert100 = Vector2.Scale(vert100, scaleVertexPositions);
            vert110 = Vector2.Scale(vert110, scaleVertexPositions);
            vert010 = Vector2.Scale(vert010, scaleVertexPositions);

            var pointIndex = Points.Length;

            Points.Add(vert000);
            Points.Add(vert100);
            Points.Add(vert110);
            Points.Add(vert010);

#if UNITY_2021_2_OR_NEWER
            var physicsShape = new PhysicsShape2D()
            {
                shapeType = PhysicsShapeType2D.Polygon,
                vertexStartIndex = pointIndex,
                vertexCount = 4,
            };
            
            PhysicsShapes.Add(physicsShape);
#endif
        }


    }
}