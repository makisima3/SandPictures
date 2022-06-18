using Code.InitDatas;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.Levels
{
    public class ToolPipeView : MonoBehaviour, IInitialized<ToolPipeViewInitData>
    {
        [SerializeField] private int index;

        private MeshRenderer _meshRenderer;
        
        public int Index => index;

        public void Initialize(ToolPipeViewInitData initData)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetColor(Color color)
        {
            _meshRenderer.sharedMaterial.color = color;
        }
    }
}