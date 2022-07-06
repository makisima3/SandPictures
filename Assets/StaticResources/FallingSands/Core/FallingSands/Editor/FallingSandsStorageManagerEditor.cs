#if UNITY_EDITOR
namespace CorgiFallingSands
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(FallingSandsStorageManager))]
    public class FallingSandsStorageManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("FallingSandsStorageManager is an example on how to save and load data chunks. " +
                "The methods are all 'virtual', so feel free to override with whatever platform you need to ship on. ", MessageType.Info);

            EditorGUILayout.HelpBox($"Data is stored in Application.persistentDataPath! For the Editor, this is {Application.persistentDataPath}", MessageType.Info);

            if(GUILayout.Button("goto save folder"))
            {
                Application.OpenURL($"file://{Application.persistentDataPath}");
            }
        }
    }
}
#endif