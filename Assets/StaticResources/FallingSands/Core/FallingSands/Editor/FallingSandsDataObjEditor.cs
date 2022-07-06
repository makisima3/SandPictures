#if UNITY_EDITOR
namespace CorgiFallingSands
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(FallingSandsDataObj))]
    public class FallingSandsDataObjEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label(string.Empty);

            if (GUILayout.Button("goto Manager prefab"))
            {
                var guids = AssetDatabase.FindAssets("FallingSandsDataManager");
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (asset == null) continue;

                    AssetDatabase.OpenAsset(asset);
                    return;
                }
            }

            GUILayout.Label(string.Empty);

            base.OnInspectorGUI();
        }
    }
}
#endif