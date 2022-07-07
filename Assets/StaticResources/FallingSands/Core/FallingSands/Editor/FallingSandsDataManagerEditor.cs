#if UNITY_EDITOR
namespace CorgiFallingSands
{
    using UnityEditor;
    using UnityEngine;
    

    [CustomEditor(typeof(FallingSandsDataManager))]
    public class FallingSandsDataManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var instance = (FallingSandsDataManager)target;
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(instance.gameObject);
            var isEditorInPrefabMode = prefabStage != null;

            if (!isEditorInPrefabMode)
            {
                EditorGUILayout.HelpBox("Please only edit this prefab in prefab mode.", MessageType.Warning);
            }
            else if (GUILayout.Button("Refresh"))
            {
                Undo.RecordObject(instance, "Refresh");

                var guids = AssetDatabase.FindAssets("t:FallingSandsDataObj");
                foreach (var guid in guids)
                {
                    if (string.IsNullOrEmpty(guid)) continue;

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path)) continue;

                    var asset = AssetDatabase.LoadAssetAtPath<FallingSandsDataObj>(path);
                    if (asset == null) continue;

                    if (!instance.DataObjects.Contains(asset))
                    {
                        instance.DataObjects.Add(asset);
                    }
                }

                EditorUtility.SetDirty(instance);

                // generate the enum file 
                if (instance.DataObjects.Count > 0)
                {
                    var directoryName = System.IO.Path.GetDirectoryName(prefabStage.assetPath);
                    var enumFilename = $"{Application.dataPath}/../{directoryName}/FallingDataTypeEnum.cs";

                    var sb = new System.Text.StringBuilder();

                    sb.AppendLine("namespace CorgiFallingSands");
                    sb.AppendLine("{");
                    sb.AppendLine("    [System.Serializable]");
                    sb.AppendLine("    public enum FallingDataType");
                    sb.AppendLine("    {");

                    for (var i = 0; i < instance.DataObjects.Count; ++i)
                    {
                        var data = instance.DataObjects[i];
                        data.Id = (int)i;

                        EditorUtility.SetDirty(data);

                        sb.AppendLine($"        {data.name} = {i}, ");
                    }

                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    try
                    {
                        if (System.IO.File.Exists(enumFilename))
                        {
                            System.IO.File.Delete(enumFilename);
                        }

                        System.IO.File.WriteAllText(enumFilename, sb.ToString());

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        Debug.Log($"Refreshed! Created: {enumFilename}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if(!instance.name.Contains("FallingSandsDataManager"))
            {
                EditorGUILayout.HelpBox("The 'goto' functionality on data objects will not work unless the name of this prefab contains 'FallingSandsDataManager'", MessageType.Warning);
            }
        }
    }
}
#endif