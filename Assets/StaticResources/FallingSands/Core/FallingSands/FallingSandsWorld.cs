namespace CorgiFallingSands
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Jobs;
    using Unity.Collections;
    using Unity.Burst;

    /// <summary>
    /// The physics simulation world. 
    /// </summary>
    public class FallingSandsWorld
    {
        // internal data 
        [System.NonSerialized] public int width;
        [System.NonSerialized] public int height;
        [System.NonSerialized] private NativeArrayPair<FallingData> DataTex;
        [System.NonSerialized] private NativeArrayPair<float> TemperatureTex;
        [System.NonSerialized] private NativeArray<bool> _trackChanges;
        [System.NonSerialized] private int _directionIndex;
        [System.NonSerialized] private bool _generate_events;
        [System.NonSerialized] private bool _boundaries_are_voids;
        [System.NonSerialized] private CreateMeshFromDataJob _prevCreateMeshJob;
        [System.NonSerialized] private bool _hasPrevCreateMeshJob;

        // shared data 
        [System.NonSerialized] private NativeQueue<FallingSandsEventData> _sharedEventsQueue;
        [System.NonSerialized] private NativeArray<FallingDataMetadata> _sharedMetadata;

        /// <summary>
        /// Creates the physics world. Automatically allocates NativeArrays and Textures. Be sure to Dispose when finished. 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="sharedEventsQueue"></param>
        /// <param name="sharedMetadata"></param>
        public FallingSandsWorld(int width, int height, NativeQueue<FallingSandsEventData> sharedEventsQueue, NativeArray<FallingDataMetadata> sharedMetadata, bool generate_events, bool boundaries_are_voids)
        {
            this.width = width;
            this.height = height;
            this._sharedEventsQueue = sharedEventsQueue;
            this._sharedMetadata = sharedMetadata;
            this._generate_events = generate_events;
            this._boundaries_are_voids = boundaries_are_voids;

            DataTex = new NativeArrayPair<FallingData>(width * height, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            TemperatureTex = new NativeArrayPair<float>(width * height, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _trackChanges = new NativeArray<bool>(GetTextureVolume(), Allocator.Persistent, NativeArrayOptions.ClearMemory);

            InitData();
        }

        /// <summary>
        /// Simply returns width * height, the volume of the data arrays. 
        /// </summary>
        /// <returns></returns>
        public int GetTextureVolume()
        {
            return width * height;
        }

        /// <summary>
        /// Returns the internal data texture. 
        /// </summary>
        /// <returns></returns>
        public NativeArrayPair<FallingData> GetDataTex()
        {
            return DataTex;
        }

        /// <summary>
        /// Returns the internal temperature texture pair. 
        /// </summary>
        /// <returns></returns>
        public NativeArrayPair<float> GetTemperatureTex()
        {
            return TemperatureTex;
        }

        /// <summary>
        /// Releases textures and disposes native arrays used only by this world. Once this is done, the physics world is no longer usable. 
        /// </summary>
        public void Dispose()
        {
            if (_trackChanges.IsCreated)
            {
                _trackChanges.Dispose();
            }

            TemperatureTex.Release();
            DataTex.Release();
        }

        /// <summary>
        /// Helper function to create and image from the data. 
        /// </summary>
        public byte[] SerializeData()
        {
            var bytes = DataTex.GetRead().Reinterpret<byte>().ToArray();
            return bytes; 
        }

        /// <summary>
        /// Helper functions to load data from an image. 
        /// </summary>
        public void DeserializeData(byte[] bytes)
        {
            DataTex.GetRead().Reinterpret<byte>().CopyFrom(bytes);
        }

        /// <summary>
        /// Initializes the already allocated arrays with valid empty data. 
        /// </summary>
        public void InitData()
        {
            var job = new InitJob()
            {
                TemperatureOut = TemperatureTex.GetWrite(),
                DataOut = DataTex.GetRead(),
                textureWidth = width,
                textureHeight = height,
            };

            var handle = job.Schedule(GetTextureVolume(), width);
                handle.Complete();

            TemperatureTex.Swap();
        }

        /// <summary>
        /// Schedules the jobs for this physics world to run. Pass in a dependency if necessary. Returns the combined dependencies. 
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public JobHandle ScheduleSimTickJobs(JobHandle dependency)
        {
            dependency = ScheduleClearTracked(dependency);
            dependency = ScheduleFallingSandsJob(dependency);
            dependency = ScheduleAddTemperatureFromData(dependency);
            dependency = ScheduleTemperatureDiffuse(dependency);
            return dependency;
        }

        /// <summary>
        /// When running a physics simulation, you can use this to inject data into the physics world. Schedule it alongside the ScheduleSimTickJobs() function. 
        /// </summary>
        /// <param name="stampTemperature"></param>
        /// <param name="mousePosUv"></param>
        /// <param name="stampSize"></param>
        /// <param name="stampData"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public JobHandle ScheduleStampJob(float stampTemperature, float2 mousePosUv, float stampSize, FallingData stampData, JobHandle dependency)
        {
            var job = new StampJob()
            {
                DataOut = DataTex.GetRead(),
                TemperatureIn = TemperatureTex.GetRead(),
                TemperatureOut = TemperatureTex.GetWrite(),
                StampTemperature = stampTemperature,
                mousePosUv = mousePosUv,
                mouseRadius = stampSize / width,
                StampData = stampData,
                textureWidth = width,
                textureHeight = height,
                inverseTextureRes = new float2(1f / width, 1f / height),
            };

            TemperatureTex.Swap();

            return job.Schedule(GetTextureVolume(), width, dependency);
        }

        /// <summary>
        /// When running a physics simulation, you can use this to inject data into the physics world. Schedule it alongside the ScheduleSimTickJobs() function. 
        /// </summary>
        /// <param name="stampTemperature"></param>
        /// <param name="mousePosUv"></param>
        /// <param name="stampSize"></param>
        /// <param name="stampData"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public JobHandle ScheduleBatchedStampJob(NativeArray<StampData> batchedStampData, JobHandle dependency)
        {
            var job = new StampBatchJob()
            {
                DataIn = DataTex.GetRead(),
                DataOut = DataTex.GetWrite(),
                TemperatureIn = TemperatureTex.GetRead(),
                TemperatureOut = TemperatureTex.GetWrite(),
                BatchedStampData = batchedStampData,
                BatchedStampDataCount = batchedStampData.Length,
                textureWidth = width,
                textureHeight = height,
                inverseTextureRes = new float2(1f / width, 1f / height),
            };

            TemperatureTex.Swap();
            DataTex.Swap();

            return job.Schedule(GetTextureVolume(), width, dependency);
        }

        /// <summary>
        /// Helper job for translating existing data within the world. 
        /// Keep in mind that this only moves data, so anything moved offscreen will be lost. 
        /// </summary>
        /// <param name="translate"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public JobHandle ScheduleTranslateJob(int2 translate, JobHandle dependency)
        {
            var clearJob = new ClearDataJob()
            {
                Data = DataTex.GetWrite(),
                width = width,
                height = height,
            };

            dependency = clearJob.Schedule(GetTextureVolume(), width, dependency);

            var job = new TranslateJob()
            {
                DataIn = DataTex.GetRead(),
                DataOut = DataTex.GetWrite(),
                TemperatureIn = TemperatureTex.GetRead(),
                TemperatureOut = TemperatureTex.GetWrite(),
                resolution = new int2(width, height),
                translate = translate,
            };

            TemperatureTex.Swap();
            DataTex.Swap(); 

            return job.Schedule(GetTextureVolume(), width, dependency); 
        }

        /// <summary>
        /// Completes the previously scheduled meshing job.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="dependency"></param>
        public void CompleteCreateMesh(Mesh mesh, JobHandle dependency)
        {
            if (!_hasPrevCreateMeshJob)
            {
                return;
            }

            dependency.Complete();

            mesh.Clear();

            mesh.indexFormat = _prevCreateMeshJob.verts.Length > ushort.MaxValue
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.SetVertices(_prevCreateMeshJob.verts.AsArray());
            mesh.SetNormals(_prevCreateMeshJob.normals.AsArray());
            mesh.SetIndices(_prevCreateMeshJob.tris.AsArray(), MeshTopology.Triangles, 0);
            mesh.bounds = _prevCreateMeshJob.bounds[0];

            // cleanup.. 
            _prevCreateMeshJob.verts.Dispose();
            _prevCreateMeshJob.normals.Dispose();
            _prevCreateMeshJob.tris.Dispose();
            _prevCreateMeshJob.bounds.Dispose();
            _hasPrevCreateMeshJob = false;
        }

        /// <summary>
        /// Schedules a meshing job. Useful for creating a unity physics mesh collider.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public JobHandle ScheduleCreateMeshFromData(JobHandle dependency)
        {
            _hasPrevCreateMeshJob = true;

            var verts = new NativeList<Vector3>(Allocator.TempJob);
            var normals = new NativeList<Vector3>(Allocator.TempJob);
            var tris = new NativeList<int>(Allocator.TempJob);
            var bounds = new NativeArray<Bounds>(1, Allocator.TempJob);
            var used = new NativeArray<bool>(width * height, Allocator.TempJob);

            _prevCreateMeshJob = new CreateMeshFromDataJob()
            {
                MeshedData = used,
                Metadata = _sharedMetadata,
                DataTex = DataTex.GetRead(),
                resolution = new int2(width, height),
                verts = verts,
                normals = normals,
                tris = tris,
                bounds = bounds,
            };

            return _prevCreateMeshJob.Schedule(dependency);
        }

#if UNITY_2021_2_OR_NEWER
        public JobHandle ScheduleCreatePolygon2D(NativeList<Vector2> _nativePolygonList, NativeList<PhysicsShape2D> _nativeShapeList, JobHandle dependency)
#else
        public JobHandle ScheduleCreatePolygon2D(NativeList<Vector2> _nativePolygonList, JobHandle dependency)
#endif
        {
            var generatePointsJob = new GeneratePolygon2DPoints()
            {
                DataTex = DataTex.GetRead(),
                MeshedData = new NativeArray<bool>(width * height, Allocator.TempJob),
                Metadata = _sharedMetadata,
                resolution = new int2(width, height),
                Points = _nativePolygonList,

#if UNITY_2021_2_OR_NEWER
                PhysicsShapes = _nativeShapeList,
#endif
            };

            return generatePointsJob.Schedule(dependency);
        }

        // internal jobs 
        private JobHandle ScheduleFallingSandsJob(JobHandle dependency)
        {
            _directionIndex++;
            _directionIndex = _directionIndex % 8;

            for(var p = 0; p < 4; ++p)
            {
                var job = new FallingSands()
                {
                    Data = DataTex.GetRead(),
                    textureWidth = width,
                    textureHeight = height,
                    Metadata = _sharedMetadata,
                    TrackedChanges = _trackChanges,
                    TemperatureIn = TemperatureTex.GetRead(),
                    direction_index = _directionIndex,
                    EventsOutput = _sharedEventsQueue.AsParallelWriter(),
                    pass_index = p,
                    generate_events = _generate_events,
                    boundaries_are_voids = _boundaries_are_voids,
                };

                dependency = job.Schedule(GetTextureVolume(), width, dependency);
            }

            return dependency;
        }

        private JobHandle ScheduleClearTracked(JobHandle dependency)
        {
            var job = new ClearTrackedJob()
            {
                Tracked = _trackChanges,
            };

            return job.Schedule(GetTextureVolume(), width, dependency);
        }

        private JobHandle ScheduleTemperatureDiffuse(JobHandle dependency)
        {
            var job = new DiffuseTemperature()
            {
                TemperatureIn = TemperatureTex.GetRead(),
                TemperatureOut = TemperatureTex.GetWrite(),
                textureWidth = width,
                textureHeight = height,
            };

            TemperatureTex.Swap();

            return job.Schedule(GetTextureVolume(), width, dependency);
        }

        private JobHandle ScheduleAddTemperatureFromData(JobHandle dependency)
        {
            var deltaTime = Time.deltaTime;

            var job = new AddTemperatureFromData()
            {
                Data = DataTex.GetRead(),
                deltaTime = deltaTime,
                Metadata = _sharedMetadata,
                TemperatureIn = TemperatureTex.GetRead(),
                TemperatureOut = TemperatureTex.GetWrite(),
                textureWidth = width,
                dissipation = 0.5f * deltaTime,
            };

            TemperatureTex.Swap();

            return job.Schedule(GetTextureVolume(), width, dependency);
        }
    }
}