using Code.InitDatas;
using DG.Tweening;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.Levels
{
    public class Grain : MonoBehaviour,IInitialized<GrainInitData>
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private Vector3 _endPosition;
        private float _timeToMove;
        private Material _material;
        
        public Vector2Int GridPosition { get; private set; }
        public Color Color { get; private set; }
        
        public void Initialize(GrainInitData initData)
        {
            meshRenderer.sharedMaterial = initData.UniqueMaterial.Material;
            transform.position = initData.SpawnPosition;
            _endPosition = initData.EndPosition;
            _timeToMove = initData.TimeToMove;
            GridPosition = initData.GridPosition;
            Color = initData.UniqueMaterial.Material.color;
        }

        public void GoToPlace()
        {
            transform.DOMove(_endPosition,_timeToMove);
        }
    }
}