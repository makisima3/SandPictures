#if UNITY_EDITOR
namespace CorgiFallingSands
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(FallingSandsSystem))]
    public class FallingSandsSystemEditor : Editor
    {
        // save data
        private SerializedProperty SaveOnDestroy;
        private SerializedProperty LoadOnStart;
        private SerializedProperty LoadChunksAfterGenerate;
        private SerializedProperty SaveAndUnloadFarAwayChunks;
        private SerializedProperty StorageManager;

        // events 
        private SerializedProperty OnFallingSandsEvent;
        private SerializedProperty GenerateEvents;

        // quality
        private SerializedProperty resolution;
        private SerializedProperty chunkResolution;
        private SerializedProperty Async;
        private SerializedProperty UpdateRate;
        private SerializedProperty MaxUpdateRate;
        private SerializedProperty BoundariesAreVoids;

        // internal 
        private SerializedProperty ourCamera;
        private SerializedProperty rendererScreenTop;
        private SerializedProperty ScreenMask;
        private SerializedProperty ourAudioPlayer;

        // procedural 3d mesh 
        private SerializedProperty GenerateMesh;
        private SerializedProperty meshCollider;

        // procedural 2d polygon 
        private SerializedProperty Generate2DPolygon;
        private SerializedProperty Assign2DPolygon;

#if UNITY_2021_2_OR_NEWER
        private SerializedProperty customCollider2D;
#else
        private SerializedProperty polygon2D;
#endif

        private void OnEnable()
        {
            SaveOnDestroy = serializedObject.FindProperty("SaveOnDestroy");
            LoadOnStart = serializedObject.FindProperty("LoadOnStart");
            LoadChunksAfterGenerate = serializedObject.FindProperty("LoadChunksAfterGenerate");
            SaveAndUnloadFarAwayChunks = serializedObject.FindProperty("SaveAndUnloadFarAwayChunks");
            StorageManager = serializedObject.FindProperty("StorageManager");

            OnFallingSandsEvent = serializedObject.FindProperty("OnFallingSandsEvent");
            GenerateEvents = serializedObject.FindProperty("GenerateEvents");

            resolution = serializedObject.FindProperty("resolution");
            chunkResolution = serializedObject.FindProperty("chunkResolution");
            Async = serializedObject.FindProperty("Async");
            UpdateRate = serializedObject.FindProperty("UpdateRate");
            MaxUpdateRate = serializedObject.FindProperty("MaxUpdateRate");
            BoundariesAreVoids = serializedObject.FindProperty("BoundariesAreVoids");

            ourCamera = serializedObject.FindProperty("ourCamera");
            rendererScreenTop = serializedObject.FindProperty("rendererScreenTop");
            ScreenMask = serializedObject.FindProperty("ScreenMask");
            ourAudioPlayer = serializedObject.FindProperty("ourAudioPlayer");

            GenerateMesh = serializedObject.FindProperty("GenerateMesh");
            meshCollider = serializedObject.FindProperty("meshCollider");

            Generate2DPolygon = serializedObject.FindProperty("Generate2DPolygon");
            Assign2DPolygon = serializedObject.FindProperty("Assign2DPolygon");

#if UNITY_2021_2_OR_NEWER
            customCollider2D = serializedObject.FindProperty("customCollider2D");
#else
            polygon2D = serializedObject.FindProperty("polygon2D");
#endif
        }

        static private bool _foldout_quality;
        static private bool _foldout_saveData;
        static private bool _foldout_events;
        static private bool _foldout_internal;
        static private bool _foldout_procedural3dmesh;
        static private bool _foldout_procedural2dpolygon;

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            var instance = (FallingSandsSystem) target;

            EditorGUILayout.BeginVertical("GroupBox");
            _foldout_quality = EditorGUILayout.Foldout(_foldout_quality, "Quality", true);
            if (_foldout_quality)
            {
                EditorGUILayout.PropertyField(Async);
                EditorGUILayout.PropertyField(resolution);
                EditorGUILayout.PropertyField(chunkResolution);
                EditorGUILayout.PropertyField(UpdateRate);
                EditorGUILayout.PropertyField(MaxUpdateRate);
                EditorGUILayout.PropertyField(BoundariesAreVoids);

                if(instance.resolution >= 1024)
                {
                    EditorGUILayout.HelpBox("Your resolution is very high! You'll probably have performance issues over 1024.", MessageType.Warning); 
                }

                if(instance.MaxUpdateRate < instance.UpdateRate)
                {
                    EditorGUILayout.HelpBox($"Your MaxUpdateRate must be higher than UpdateRate. " +
                        $"Try making it a multiple of your UpdateRate. For example: {instance.UpdateRate} * 2", 
                        MessageType.Error); 
                }

                if(instance.MaxUpdateRate > instance.UpdateRate * 16)
                {
                    EditorGUILayout.HelpBox($"Your MaxUpdateRate is very high! This can lead to performance problems. " +
                        $"Try making it a multiple of your UpdateRate. For example: {instance.UpdateRate} * 2",
                        MessageType.Warning);
                }

                if (instance.Async)
                {
                    EditorGUILayout.HelpBox("Because you have Async enabled, CorgiFallingSands will try to run the physics at the same time as the rest of the game. "
                        + "The more work your game has to do outside of the physics engine, the more you will benefit from this Async work. "
                        + "But, keep in mind that you cannot access the data textures directly anymore. You now must use the async API RequestSampleAtX() and GetSample().", MessageType.Info);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("GroupBox");
            _foldout_saveData = EditorGUILayout.Foldout(_foldout_saveData, "Save Data", true);
            if(_foldout_saveData)
            {
                EditorGUILayout.PropertyField(StorageManager);

                if(instance.StorageManager != null)
                {
                    EditorGUILayout.PropertyField(SaveOnDestroy);
                    EditorGUILayout.PropertyField(LoadOnStart);
                    EditorGUILayout.PropertyField(LoadChunksAfterGenerate);
                    EditorGUILayout.PropertyField(SaveAndUnloadFarAwayChunks);
                }
                else
                {
                    SaveOnDestroy.boolValue = false;
                    LoadOnStart.boolValue = false;
                    LoadChunksAfterGenerate.boolValue = false;
                    SaveAndUnloadFarAwayChunks.boolValue = false;

                    serializedObject.ApplyModifiedProperties(); 
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("GroupBox");
            _foldout_events = EditorGUILayout.Foldout(_foldout_events, "Events", true);
            if(_foldout_events)
            {
                EditorGUILayout.PropertyField(GenerateEvents);
                if(instance.GenerateEvents)
                {
                    EditorGUILayout.PropertyField(OnFallingSandsEvent);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("GroupBox");
            _foldout_procedural3dmesh = EditorGUILayout.Foldout(_foldout_procedural3dmesh, "3D procedural mesh", true);
            if(_foldout_procedural3dmesh)
            {
                EditorGUILayout.PropertyField(GenerateMesh);
                if(instance.GenerateMesh)
                {
                    EditorGUILayout.PropertyField(meshCollider);

                    if(instance.meshCollider == null)
                    {
                        EditorGUILayout.HelpBox("You are generating a 3d mesh but you are not assigning it to a meshCollider. " +
                            "If you are handling this yourself, you can ignore this message. " +
                            "If not, you are just wasting CPU cycles.", MessageType.Warning);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("GroupBox");
            _foldout_procedural2dpolygon = EditorGUILayout.Foldout(_foldout_procedural2dpolygon, "2D procedural polygon", true);
            if(_foldout_procedural2dpolygon)
            {
                EditorGUILayout.PropertyField(Generate2DPolygon);
                EditorGUILayout.PropertyField(Assign2DPolygon);

                if(instance.Generate2DPolygon)
                {
                    #if UNITY_2021_2_OR_NEWER
                        EditorGUILayout.PropertyField(customCollider2D);

                        if(instance.customCollider2D == null)
                        {
                            EditorGUILayout.HelpBox("You are generating a 2d polygon but you are not assigning it to a customCollider2D. " +
                                "If you are handling this yourself, you can ignore this message. " +
                                "If not, you are just wasting CPU cycles.", MessageType.Warning);
                        }
                    #else
                        if(instance.polygon2D == null)
                        {
                            EditorGUILayout.HelpBox("You are generating a 2d polygon but you are not assigning it to a polygon2D. " +
                                "If you are handling this yourself, you can ignore this message. " +
                                "If not, you are just wasting CPU cycles.", MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(polygon2D);
                        
                        EditorGUILayout.HelpBox("This operation is slow on this version of Unity. " +
                            "Updating to 2021.2 or later will drastically speed up this process.", MessageType.Warning);
                    #endif
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("GroupBox");
            _foldout_internal = EditorGUILayout.Foldout(_foldout_internal, "Internal", true);
            if (_foldout_internal)
            {
                EditorGUILayout.PropertyField(ourCamera);
                EditorGUILayout.PropertyField(rendererScreenTop);
                EditorGUILayout.PropertyField(ScreenMask);
                EditorGUILayout.PropertyField(ourAudioPlayer);

                if(instance.ourCamera == null || instance.rendererScreenTop == null || instance.ourAudioPlayer == null)
                {
                    EditorGUILayout.HelpBox("You are missing some reference!", MessageType.Error); 
                }
            }
            EditorGUILayout.EndVertical();

            if (Application.isPlaying)
            {
                var chunkManager = instance._fallingSandsChunkManager;
                var chunks = chunkManager.Chunks;
                var pixelCount = chunks.Count
                    * chunkManager.resolutionPerChunk.x * chunkManager.resolutionPerChunk.y;

                var dataTexMb = pixelCount * 1 / 1000 / 1000f;
                var tempTexMb = pixelCount * 1 / 1000 / 1000f;

                GUILayout.Label("stats:");
                GUILayout.Label($"current position: {instance._currentPosition}");
                GUILayout.Label($"chunks: {chunks.Count:N2} | ({pixelCount:N0} pixels (float + float) = {dataTexMb + tempTexMb:N2} megabytes)");
            }


            // save 
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif