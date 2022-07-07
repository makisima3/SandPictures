
namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Jobs;
    using Unity.Burst;
    using Unity.Collections;
    using UnityEngine.Events;
    using Unity.Collections.LowLevel.Unsafe;

    public class FallingSandsSystem : MonoBehaviour
    {
        
        public bool SaveOnDestroy;
        public bool LoadOnStart;
        public bool LoadChunksAfterGenerate;
        public bool SaveAndUnloadFarAwayChunks;
        public FallingSandsStorageManager StorageManager;

        public UnityEvent<FallingSandsEventData, float> OnFallingSandsEvent = new UnityEvent<FallingSandsEventData, float>();
        public bool GenerateEvents;

        [Tooltip("note: width will be adjusted based on the renderer associated. smaller values are faster to calculate.")] public int resolution = 256;
        [Tooltip("note: always square. usually the same or smaller than resolution.")] public int chunkResolution = 256;
        [Tooltip("When enabled, the system will be ran asynconously. However, this means you cannot access the data textures directly while it's running. Do not change while running.")] public bool Async;
        [Tooltip("How frequently the system will update. Try using values like '1/60', for 60 physics ticks per second.")] public float UpdateRate = 1f / 60f;
        [Tooltip("Prevents the system from slowing down the game if the framerate is too low. Use a multiple of UpdateRate.")] public float MaxUpdateRate = 2f / 60f;
        [Tooltip("When true, boundaries are treated as voids, so data can be lost. Does not update at runtime.")] public bool BoundariesAreVoids = true;

        public Camera ourCamera;
        public Renderer rendererScreenTop;
        public LayerMask ScreenMask;
        public SimpleAudioPlayer ourAudioPlayer;

        [Tooltip("When true, a mesh will be generated after every physics step. Useful for collision.")] public bool GenerateMesh;
        public MeshCollider meshCollider;

        [Tooltip("When true, a 2d polygon will be generated after every physics step.")] public bool Generate2DPolygon;
        [Tooltip("When true, the 2d polygon will be dfined to the referenced PolygonCollider2D component")] public bool Assign2DPolygon;

#if UNITY_2021_2_OR_NEWER
        public CustomCollider2D customCollider2D;
        private NativeList<PhysicsShape2D> _nativeCustomShapes;
#else
        public PolygonCollider2D polygon2D;
#endif

        public static FallingSandsSystem Instance;

        // more internal stuff
        [System.NonSerialized] public FallingSandsWorld _fallingSandsWorld;
        [System.NonSerialized] public FallingSandsChunkManager _fallingSandsChunkManager;
        [System.NonSerialized] public int2 _currentPosition;

        [System.NonSerialized] private NativeQueue<FallingSandsEventData> _nativeEventsQueue;
        [System.NonSerialized] private NativeArray<FallingDataMetadata> _nativeMetadata;
        [System.NonSerialized] private Material rendererMaterial;
        [System.NonSerialized] private bool _pausePhysics;
        [System.NonSerialized] private Dictionary<int, float> _eventHandleCache = new Dictionary<int, float>();

        [System.NonSerialized] private NativeList<int2> _sampleRequests;
        [System.NonSerialized] private NativeList<SampleData> _sampleResults;
        [System.NonSerialized] private NativeList<StampData> _stampRequests;
        [System.NonSerialized] private NativeList<Vector2> _nativePolygonList;

        [System.NonSerialized] private float _updateTimeCounter;
        [System.NonSerialized] private JobHandle _lastUpdateHandle;
        [System.NonSerialized] private Mesh[] _sharedMeshPair = new Mesh[2] { null, null };
        [System.NonSerialized] private bool _swapSharedMesh;


        [System.NonSerialized] private Texture2D DataTex;
        [System.NonSerialized] private Texture2D TempTex;

        // unity events 
        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Async)
            {
                CompleteSystemUpdate();
            }
        }

        private void Start()
        {
            InitColorStructuredBuffer();
            InitMetadata();

            // resize width to match the screen's aspect ratio.. 
            var rendererScale = rendererScreenTop.transform.localScale;
            var aspectRatio = rendererScale.x / rendererScale.y;
            var width = Mathf.FloorToInt(resolution * aspectRatio);

            _nativeEventsQueue = new NativeQueue<FallingSandsEventData>(Allocator.Persistent);
            _fallingSandsWorld = new FallingSandsWorld(width, resolution, _nativeEventsQueue, _nativeMetadata, GenerateEvents, BoundariesAreVoids);
            _fallingSandsChunkManager = new FallingSandsChunkManager(new int2(chunkResolution, chunkResolution));

            _sampleRequests = new NativeList<int2>(16, Allocator.Persistent);
            _sampleResults = new NativeList<SampleData>(16, Allocator.Persistent);
            _stampRequests = new NativeList<StampData>(16, Allocator.Persistent);
            _nativePolygonList = new NativeList<Vector2>(Allocator.Persistent);

#if UNITY_2021_2_OR_NEWER
            _nativeCustomShapes = new NativeList<PhysicsShape2D>(Allocator.Persistent);
#endif

            DataTex = new Texture2D(width, resolution, TextureFormat.RFloat, false, true);
            TempTex = new Texture2D(width, resolution, TextureFormat.RFloat, false, true);

            DataTex.filterMode = FilterMode.Point;
            TempTex.filterMode = FilterMode.Bilinear;

            DataTex.Apply();
            TempTex.Apply();

            rendererMaterial = rendererScreenTop.material;
            rendererMaterial.mainTexture = DataTex;

            // initialize by loading old data 
            if(LoadOnStart)
            {
                var loadHandle = ScheduleLoadAllChunks(default);
                var blitHandle = ScheduleBlitIntoSystem(loadHandle);
                    blitHandle.Complete();
            }
        }

        private void OnDestroy()
        {
            if(Async) 
            {
                CompleteSystemUpdate();
            }

            if(SaveOnDestroy)
            {
                var blitHandle = ScheduleBlitOutOfSystem(default);
                var saveHandle = ScheduleSaveAllChunks(blitHandle);
                    saveHandle.Complete();
            }

            if (_nativeMetadata.IsCreated)
            {
                _nativeMetadata.Dispose();
            }

            if (_nativeEventsQueue.IsCreated)
            {
                _nativeEventsQueue.Dispose();
            }

            if (_sampleRequests.IsCreated)
            {
                _sampleRequests.Dispose();
            }

            if (_sampleResults.IsCreated)
            {
                _sampleResults.Dispose();
            }

            if (_stampRequests.IsCreated)
            {
                _stampRequests.Dispose();
            }

            if (_nativePolygonList.IsCreated)
            {
                _nativePolygonList.Dispose();
            }

#if UNITY_2021_2_OR_NEWER
            if (_nativeCustomShapes.IsCreated)
            {
                _nativeCustomShapes.Dispose();
            }
#endif

            if(DataTex != null)
            {
                Texture2D.Destroy(DataTex);
                DataTex = null;
            }

            if (TempTex != null)
            {
                Texture2D.Destroy(TempTex);
                TempTex = null;
            }

            _fallingSandsWorld.Dispose();
        }

        private void Update()
        {
            if (Async)
            {
                CompleteSystemUpdate();
            }

            UpdateRequests();
            UpdateVisuals();

            _updateTimeCounter += Time.deltaTime;
            _updateTimeCounter = Mathf.Min(_updateTimeCounter, MaxUpdateRate);

            while (_updateTimeCounter >= UpdateRate)
            {
                _updateTimeCounter -= UpdateRate;
                UpdateFallingSandsSim();
            }

            if(Async)
            {
                _lastUpdateHandle = ScheduleAnyMeshGeneration(_lastUpdateHandle);
                _unloadHandles = ScheduleUnloadFarAwayChunks(_lastUpdateHandle);
            }
        }

        [System.NonSerialized] private List<FallingSandsCollisionRigidbody> _trackedBodies = new List<FallingSandsCollisionRigidbody>();
        [System.NonSerialized] private List<FallingSandsCollisionRigidbody2D> _trackedBodies2D = new List<FallingSandsCollisionRigidbody2D>();

        public void RegisterRigidbody(FallingSandsCollisionRigidbody body)
        {
            _trackedBodies.Add(body);
        }

        public void UnRegisterRigidbody(FallingSandsCollisionRigidbody body)
        {
            _trackedBodies.Remove(body);
        }

        public void RegisterRigidbody(FallingSandsCollisionRigidbody2D body)
        {
            _trackedBodies2D.Add(body);
        }

        public void UnRegisterRigidbody(FallingSandsCollisionRigidbody2D body)
        {
            _trackedBodies2D.Remove(body);
        }

        private void ShiftAllTrackedBodies(Vector3 shiftDelta)
        {
            var count = _trackedBodies.Count;
            for (var i = 0; i < count; ++i)
            {
                var tracked = _trackedBodies[i];
                tracked.ourBody.position = tracked.ourBody.position + shiftDelta;
            }

            var shiftDelta2D = new Vector2(shiftDelta.x, shiftDelta.y);
            count = _trackedBodies2D.Count;
            for (var i = 0; i < count; ++i)
            {
                var tracked = _trackedBodies2D[i];
                tracked.ourBody.position = tracked.ourBody.position + shiftDelta2D;
            }
        }

        /// <summary>
        /// Transforms a world position to a screen position and then into a texture position for accessing the internal data textures. 
        /// Requires both the camera and screen renderers to be defined. 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public int2 GetWorldPositionToTexturePosition(Vector3 worldPosition)
        {
            var screenPosition = ourCamera.WorldToScreenPoint(worldPosition);
            return GetScreenPointToTexturePosition(screenPosition);
        }

        /// <summary>
        /// Transforms a screen position (ex. Input.mousePosition) into a texture position, for accessing the internal data textures.
        /// Requires both the camera and screen renderers to be defined. 
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public int2 GetScreenPointToTexturePosition(Vector3 screenPoint)
        {
            var ray = ourCamera.ScreenPointToRay(screenPoint);
            var hit = Physics.Raycast(ray, out RaycastHit info, 128f, ScreenMask, QueryTriggerInteraction.Ignore);
            if (!hit)
            {
                return -1;
            }

            float2 texCoord = new float2(info.textureCoord.x, info.textureCoord.y);
            float2 resolution = new float2(_fallingSandsWorld.width, _fallingSandsWorld.height);

            return (int2)(texCoord * resolution);
        }

        /// <summary>
        /// Use this to sample the pixel physics world. This returns a ticket. Use it one frame later to get the result via GetSample(ticket); 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public int RequestSampleAtWorldPosition(Vector3 worldPosition)
        {
            var samplePosition = GetWorldPositionToTexturePosition(worldPosition);

            var ticket = _sampleRequests.Length;
            _sampleRequests.Add(samplePosition);
            return ticket;
        }

        /// <summary>
        /// Use this to sample the pixel physics world. This returns a ticket. Use it one frame later to get the result via GetSample(ticket); 
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public int RequestSampleAtScreenPosition(Vector3 screenPosition)
        {
            var samplePosition = GetScreenPointToTexturePosition(screenPosition);

            var ticket = _sampleRequests.Length;
            _sampleRequests.Add(samplePosition);
            return ticket;
        }

        /// <summary>
        /// One frame after requesting a ticket via RequestSampleAt(), you can use GetSample(ticket) to view the sample.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public SampleData GetSample(int ticket)
        {
            if (ticket < 0 || ticket > _sampleResults.Length)
            {
                return default;
            }

            return _sampleResults[ticket];
        }

        /// <summary>
        /// Inserts an async stamp request. Will place pixels of type id of size radius into the requested position during the next physics pass.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="screenPosition"></param>
        /// <param name="radius"></param>
        public void RequestStampAtScreenPosition(FallingData data, float temperature, Vector3 screenPosition, int radius)
        {
            var samplePosition = GetScreenPointToTexturePosition(screenPosition);

            var stampData = new StampData()
            {
                id = data,
                temperature = temperature,
                position = samplePosition,
                radius = radius,
            };

            _stampRequests.Add(stampData);
        }

        public void Clear()
        {
            _fallingSandsWorld.InitData();
        }

        public void TogglePausePhysics()
        {
            _pausePhysics = !_pausePhysics;
            Debug.Log($"_pausePhysics: {_pausePhysics}");
        }

        // internal stuff 
        private void InitMetadata()
        {
            var count = FallingSandsDataManager.Instance.DataObjects.Count;

            _nativeMetadata = new NativeArray<FallingDataMetadata>(count, Allocator.Persistent);
            for (var i = 0; i < count; ++i)
            {
                _nativeMetadata[i] = FallingSandsDataManager.Instance.DataObjects[i].Metadata;
            }
        }

        private void InitColorStructuredBuffer()
        {
            FallingSandsDataManager.Instance.DataObjects.Sort((a, b) => a.Id.CompareTo(b.Id));

            var count = FallingSandsDataManager.Instance.DataObjects.Count;

            var max = 32; // must match shader
            var colorData = new Vector4[max];

            for (var i = 0; i < count; ++i)
            {
                colorData[i] = FallingSandsDataManager.Instance.DataObjects[i].Metadata.Color;
            }

            // var computeBuffer = new ComputeBuffer(count, sizeof(float) * 4, ComputeBufferType.Structured);
            //     computeBuffer.SetData(colorData);

            Shader.SetGlobalVectorArray("FallingSandsDataColors", colorData);
            Shader.SetGlobalInt("FallingSandsDataColorsLength", count);
        }

        private void UpdateVisuals()
        {
            DataTex.LoadRawTextureData<float>(_fallingSandsWorld.GetDataTex().GetRead().Reinterpret<float>());
            TempTex.LoadRawTextureData<float>(_fallingSandsWorld.GetTemperatureTex().GetRead().Reinterpret<float>());

            DataTex.Apply();
            TempTex.Apply();

            rendererMaterial = rendererScreenTop.material;
            rendererMaterial.mainTexture = DataTex;
            rendererMaterial.SetTexture("_TemperatureTex", TempTex);
        }

        private void UpdateFallingSandsSim()
        {
            _lastUpdateHandle = UpdatePosition(_lastUpdateHandle);
            _lastUpdateHandle = ScheduleBatchedStampJob(_lastUpdateHandle);

            if (_pausePhysics)
            {
                _lastUpdateHandle.Complete();
                return;
            }

            _lastUpdateHandle = _fallingSandsWorld.ScheduleSimTickJobs(_lastUpdateHandle);

            if (!Async)
            {
                _lastUpdateHandle = ScheduleAnyMeshGeneration(_lastUpdateHandle);
                _unloadHandles = ScheduleUnloadFarAwayChunks(_lastUpdateHandle);

                CompleteSystemUpdate();
            }
        }

        private JobHandle ScheduleAnyMeshGeneration(JobHandle chain)
        {
            if (GenerateMesh)
            {
                chain = _fallingSandsWorld.ScheduleCreateMeshFromData(chain);
            }

            if (Generate2DPolygon)
            {
#if UNITY_2021_2_OR_NEWER
                chain = _fallingSandsWorld.ScheduleCreatePolygon2D(_nativePolygonList, _nativeCustomShapes, chain);
#else
                chain = _fallingSandsWorld.ScheduleCreatePolygon2D(_nativePolygonList, chain); 
#endif
            }

            return chain;
        }

        private void CompleteSystemUpdate()
        {
            _lastUpdateHandle.Complete();
            _lastUpdateHandle = default;

            if (GenerateMesh)
            {
                _swapSharedMesh = !_swapSharedMesh;

                var readMesh = _sharedMeshPair[0];
                var writeMesh = _sharedMeshPair[1];

                if (readMesh == null || writeMesh == null)
                {
                    writeMesh = new Mesh();
                    readMesh = new Mesh();

                    _sharedMeshPair[0] = writeMesh;
                    _sharedMeshPair[1] = readMesh;
                }

                if (_swapSharedMesh)
                {
                    readMesh = _sharedMeshPair[1];
                    writeMesh = _sharedMeshPair[0];
                }

                meshCollider.sharedMesh = readMesh;

                _fallingSandsWorld.CompleteCreateMesh(writeMesh, _lastUpdateHandle);

                if (Async)
                {
                    // update the mesh collider off the main thread 
                    var bakeMeshJob = new BakeMeshJob()
                    {
                        meshId = writeMesh.GetInstanceID(),
                        convex = false,
                    };

                    _lastUpdateHandle = bakeMeshJob.Schedule(_lastUpdateHandle);
                }
                else
                {
                    meshCollider.sharedMesh = writeMesh;
                }
            }

            if (Generate2DPolygon)
            {
                // slow af. damn unity 🔪
                if (Assign2DPolygon)
                {
#if UNITY_2021_2_OR_NEWER
                    customCollider2D.enabled = false;

                    if (_nativeCustomShapes.Length > 0)
                    {
                        customCollider2D.SetCustomShapes(_nativeCustomShapes, _nativePolygonList);
                    }
                    else
                    {
                        customCollider2D.ClearCustomShapes();
                    }

                    customCollider2D.enabled = true;
#else
                    polygon2D.enabled = false;
                    polygon2D.pathCount = _nativePolygonList.Length / 4;

                    var cache = new Vector2[4];

                    var pointsIndexOffset = 0;
                    for (var i = 0; i < _nativePolygonList.Length / 4 - 1; ++i)
                    {
                        cache[0] = _nativePolygonList[pointsIndexOffset + 0];
                        cache[1] = _nativePolygonList[pointsIndexOffset + 1];
                        cache[2] = _nativePolygonList[pointsIndexOffset + 2];
                        cache[3] = _nativePolygonList[pointsIndexOffset + 3];

                        polygon2D.SetPath(i, cache);

                        pointsIndexOffset += 4;
                    }

                    polygon2D.enabled = true;
#endif
                }
            }

            // handle events.. 
            HandleEvents();

            // finish unloading far off chunks 
            CompleteUnloadFarAwayChunks();
        }

        private struct BakeMeshJob : IJob
        {
            public int meshId;
            public bool convex;

            public void Execute()
            {
                Physics.BakeMesh(meshId, convex);
            }
        }

        private void HandleEvents()
        {
            var time = Time.time;

            if (Application.isMobilePlatform)
            {
                _nativeEventsQueue.Clear();
            }

            while (!_nativeEventsQueue.IsEmpty())
            {
                var nativeEvent = _nativeEventsQueue.Dequeue();
                HandleEvent(nativeEvent, time);
            }
        }

        private void HandleEvent(FallingSandsEventData data, float time)
        {
            OnFallingSandsEvent.Invoke(data, time);

            var fromId = data.id_a;

            if (_eventHandleCache.TryGetValue(fromId, out float prevTime))
            {
                if (time < prevTime + 0.1f)
                {
                    return;
                }
                else
                {
                    _eventHandleCache[fromId] = time;
                }
            }
            else
            {
                _eventHandleCache.Add(fromId, time);
            }


            var fromData = FallingSandsDataManager.Instance.FindDataObjectFromId((int)fromId);

            if (data.temperature > fromData.Metadata.MaxTemperature && fromData.OnTempHigh != null)
            {
                ourAudioPlayer.PlaySoundBurst(fromData.OnTempHigh, 0.10f);
            }

            if (data.temperature < fromData.Metadata.MinTemperature && fromData.OnTempLow != null)
            {
                ourAudioPlayer.PlaySoundBurst(fromData.OnTempLow, 0.10f);
            }
        }

        [System.NonSerialized] public int2 RequestShiftPosition;

        private JobHandle UpdatePosition(JobHandle dependency)
        {
            var delta = 1;

            var moveDelta = RequestShiftPosition;

            RequestShiftPosition = new int2(0, 0);

            if (Input.GetKey(KeyCode.LeftArrow)) moveDelta.x -= delta;
            if (Input.GetKey(KeyCode.RightArrow)) moveDelta.x += delta;
            if (Input.GetKey(KeyCode.UpArrow)) moveDelta.y += delta;
            if (Input.GetKey(KeyCode.DownArrow)) moveDelta.y -= delta;

            if (math.length(moveDelta) == 0)
            {
                return dependency;
            }

            // shift tracked rigidbodies 
            var scaleVertexPositions = new Vector3(1f / _fallingSandsWorld.width, 1f / _fallingSandsWorld.height, 0f);
            scaleVertexPositions = Vector3.Scale(scaleVertexPositions, rendererScreenTop.transform.localScale);

            var shiftBodiesBy = new Vector3(moveDelta.x * scaleVertexPositions.x, moveDelta.y * scaleVertexPositions.y);

            ShiftAllTrackedBodies(-shiftBodiesBy);

            dependency = ScheduleBlitOutOfSystem(dependency);

            _currentPosition += moveDelta;

            dependency = ScheduleBlitIntoSystem(dependency);

            return dependency;
        }

        private void UpdateRequests()
        {
            _sampleResults.Clear();

            var count = _sampleRequests.Length;
            for (var i = 0; i < count; ++i)
            {
                _sampleResults.Add(default);
            }

            var res = new int2(_fallingSandsWorld.width, _fallingSandsWorld.height);

            var job = new SampleRequestsJob()
            {
                Data = _fallingSandsWorld.GetDataTex().GetRead(),
                Temperature = _fallingSandsWorld.GetTemperatureTex().GetRead(),
                SampleRequests = _sampleRequests,
                SampleResults = _sampleResults,
                sampleResolution = res,
            };

            var handle = job.Schedule(count, 4);
            handle.Complete();

            _sampleRequests.Clear();
        }

        private JobHandle ScheduleTranslateJob(JobHandle dependency)
        {
            var translate = new int2(0, 0);

            if (Input.GetKey(KeyCode.LeftArrow)) translate.x -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) translate.x += 1;
            if (Input.GetKey(KeyCode.UpArrow)) translate.y += 1;
            if (Input.GetKey(KeyCode.DownArrow)) translate.y -= 1;

            return _fallingSandsWorld.ScheduleTranslateJob(translate, dependency);
        }

        private JobHandle ScheduleBatchedStampJob(JobHandle dependency)
        {
            if (_stampRequests.Length == 0)
            {
                return dependency;
            }

            var tempBatchRequests = new NativeArray<StampData>(_stampRequests.Length, Allocator.TempJob);
            tempBatchRequests.CopyFrom(_stampRequests);

            _stampRequests.Clear();

            return _fallingSandsWorld.ScheduleBatchedStampJob(tempBatchRequests, dependency);
        }

        private JobHandle ScheduleBlitIntoSystem(JobHandle dependency)
        {
            var screenRes = new int2(_fallingSandsWorld.width, _fallingSandsWorld.height);
            var lastCopyRes = screenRes;

            for (var x = 0; x < screenRes.x;)
            {
                for (var y = 0; y < screenRes.y;)
                {
                    var pos = new int2(x, y) + _currentPosition;
                    var chunkPos = _fallingSandsChunkManager.GetChunkPosFromWorldPos(pos);

                    var posInsideChunk = pos - chunkPos;

                    var remainingInSystem = math.abs(screenRes - new int2(x, y));
                    var remainingInChunk = math.abs(new int2(chunkResolution, chunkResolution) - posInsideChunk);
                    var copyRes = math.min(remainingInSystem, remainingInChunk);

                    var blitChunk = _fallingSandsChunkManager.TryGetChunk(chunkPos);
                    if (blitChunk == null)
                    {
                        blitChunk = _fallingSandsChunkManager.CreateChunkAt(chunkPos);
                        dependency = ScheduleInitializeNewChunk(blitChunk, new int2(chunkResolution, chunkResolution), dependency);
                    }

                    // Debug.Log($"blitting {fromPos} to {toPos}, copyRes: {copyRes}, copyRegionInputPos {new int2(x, y)} chunkPos {chunkPos}");

                    var copyRegionInputPos = posInsideChunk;
                    var copyRegionOutputPos = new int2(x, y);
                    var copyRegion = copyRes;

                    var blitJob = new DataBlitJob()
                    {
                        InputData = blitChunk.DataTex,
                        OutputData = _fallingSandsWorld.GetDataTex().GetRead(),

                        InputTemp = blitChunk.TempTex,
                        OutputTemp = _fallingSandsWorld.GetTemperatureTex().GetRead(),

                        copyResolution = copyRegion,
                        inputTextureResolution = new int2(chunkResolution, chunkResolution),
                        outputTextureResolution = screenRes,
                        copyRegionInputPos = copyRegionInputPos,
                        copyRegionOutputPos = copyRegionOutputPos,
                    };

                    dependency = blitJob.Schedule(copyRes.x * copyRes.y, copyRes.x, dependency);

                    // offset our x,y to be at the start of a chunk, so when the iterator moves we'll be at the next chunk 
                    lastCopyRes = copyRes;
                    y += lastCopyRes.y;
                }

                x += lastCopyRes.x;
            }

            return dependency;
        }

        private JobHandle ScheduleBlitOutOfSystem(JobHandle dependency)
        {
            var screenRes = new int2(_fallingSandsWorld.width, _fallingSandsWorld.height);

            var lastCopyRes = screenRes;

            for (var x = 0; x < screenRes.x;)
            {
                for (var y = 0; y < screenRes.y;)
                {
                    var pos = new int2(x, y) + _currentPosition;
                    var chunkPos = _fallingSandsChunkManager.GetChunkPosFromWorldPos(pos);

                    var posInsideChunk = pos - chunkPos;

                    var remainingInSystem = math.abs(screenRes - new int2(x, y));
                    var remainingInChunk = math.abs(new int2(chunkResolution, chunkResolution) - posInsideChunk);
                    var copyRes = math.min(remainingInSystem, remainingInChunk);

                    var blitChunk = _fallingSandsChunkManager.TryGetChunk(chunkPos);
                    if (blitChunk == null)
                    {
                        blitChunk = _fallingSandsChunkManager.CreateChunkAt(chunkPos);
                        dependency = ScheduleInitializeNewChunk(blitChunk, new int2(chunkResolution, chunkResolution), dependency);
                    }

                    var copyRegionInputPos = new int2(x, y);
                    var copyRegionOutputPos = posInsideChunk;
                    var copyRegion = copyRes;

                     Debug.Log($"[{chunkPos}] {copyRegionInputPos}->{copyRegionOutputPos}, copyRes: {copyRes}, res: {screenRes }");

                    var blitJob = new DataBlitJob()
                    {
                        InputData = _fallingSandsWorld.GetDataTex().GetRead(),
                        OutputData = blitChunk.DataTex,

                        InputTemp = _fallingSandsWorld.GetTemperatureTex().GetRead(),
                        OutputTemp = blitChunk.TempTex,

                        copyResolution = copyRegion,
                        inputTextureResolution = screenRes,
                        outputTextureResolution = new int2(chunkResolution, chunkResolution),
                        copyRegionInputPos = copyRegionInputPos,
                        copyRegionOutputPos = copyRegionOutputPos,
                    };

                    dependency = blitJob.Schedule(copyRes.x * copyRes.y, copyRes.x, dependency);

                    // offset our x,y to be at the start of a chunk, so when the iterator moves we'll be at the next chunk 
                    lastCopyRes = copyRes;
                    y += lastCopyRes.y;
                }

                x += lastCopyRes.x;
            }

            return dependency;
        }

        private JobHandle ScheduleInitializeNewChunk(FallingSandsChunk chunk, int2 chunkRes, JobHandle dependency)
        {
            // generate a new chunk 
            var job = new GenerateNewChunkJob()
            {
                DataTex = chunk.DataTex,
                TempTex = chunk.TempTex,
                ChunkPosition = chunk.position,
                resolution = chunkRes,
            };

            dependency = job.Schedule(chunkRes.x * chunkRes.y, chunkRes.x, dependency);

            // try to load from disk. will overwrite the generate.
            // wasteful.
            // shameful.
            if(LoadChunksAfterGenerate)
            {
                dependency = StorageManager.ScheduleLoadChunk(chunk, dependency); 
            }

            return dependency; 
        }

        private JobHandle ScheduleSaveAllChunks(JobHandle dependency)
        {
            var chunkData = _fallingSandsChunkManager.Chunks;

            JobHandle fileJobHandles = default;
            for (int i = 0; i < 1; i++)
            {
                var chunk = chunkData[i];

                var handle = StorageManager.ScheduleSaveChunk(chunk, dependency);
                fileJobHandles = JobHandle.CombineDependencies(fileJobHandles, handle);
            }
            /*foreach(var entry in chunkData)
            {
                var chunk = entry.Value;

                var handle = StorageManager.ScheduleSaveChunk(chunk, dependency);
                fileJobHandles = JobHandle.CombineDependencies(fileJobHandles, handle);
            }*/

            return JobHandle.CombineDependencies(fileJobHandles, dependency);
        }

        private JobHandle ScheduleLoadAllChunks(JobHandle dependency)
        {
            var storedChunkPos = StorageManager.GatherStoredChunks();
            
            JobHandle fileJobHandles = default;
            foreach (var pos in storedChunkPos)
            {
                var chunk = _fallingSandsChunkManager.TryGetChunk(pos);

                if (chunk == null)
                {
                    chunk = _fallingSandsChunkManager.CreateChunkAt(pos);
                }

                var handle = StorageManager.ScheduleLoadChunk(chunk, dependency);
                fileJobHandles = JobHandle.CombineDependencies(fileJobHandles, handle);
            }

            return JobHandle.CombineDependencies(fileJobHandles, dependency);
        }

        private List<int2> _unloadCache = new List<int2>();
        private JobHandle _unloadHandles;

        private void CompleteUnloadFarAwayChunks()
        {
            if (!SaveAndUnloadFarAwayChunks)
            {
                return;
            }

            if(!_unloadHandles.IsCompleted)
            {
                return;
            }

            _unloadHandles.Complete(); 

            for (var i = 0; i < _unloadCache.Count; ++i)
            {
                var chunkKey = _unloadCache[i];
                var chunk = _fallingSandsChunkManager.TryGetChunk(chunkKey);
                if(chunk != null)
                {
                    _fallingSandsChunkManager.TryRemoveChunk(chunkKey);
                }
            }
        }

        private JobHandle ScheduleUnloadFarAwayChunks(JobHandle dependency)
        {
            if(!SaveAndUnloadFarAwayChunks)
            {
                return default;
            }

            if(!_unloadHandles.IsCompleted)
            {
                return _unloadHandles;
            }

            var unload_distance = 2048;

            _unloadCache.Clear();

            JobHandle fileHandle = default;

            var chunkEntries = _fallingSandsChunkManager.Chunks;
            foreach(var entry in chunkEntries)
            {
                var chunkPosition = entry.Key;

                if (math.length(chunkPosition - _currentPosition) > unload_distance)
                {
                    _unloadCache.Add(chunkPosition);

                    var handle = StorageManager.ScheduleSaveChunk(entry.Value, dependency);
                    fileHandle = JobHandle.CombineDependencies(handle, fileHandle);
                }
            }

            // remove from the active chunk list
            // note: this does NOT unload the chunk, must be done later 
            foreach(var key in _unloadCache)
            {
                chunkEntries.Remove(key); 
            }

            return JobHandle.CombineDependencies(fileHandle, dependency);
        }
    }
}