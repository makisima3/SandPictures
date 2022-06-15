using Code.InitDatas;
using DG.Tweening;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.Levels
{
    public class Grain : MonoBehaviour,IInitialized<GrainInitData>
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
        private Vector3 _endPosition;
        private Vector3 _renderPosition;
        private float _timeToMove;
        private Material _material;
        private Transform _renderParent;
        
        public Vector2Int GridPosition { get; private set; }
        public Color Color { get; private set; }
        
        public void Initialize(GrainInitData initData)
        {
            trailRenderer.enabled = false;
            transform.position = initData.SpawnPosition;
            _endPosition = initData.EndPosition;
            _renderPosition = initData.RenderPosition;
            _timeToMove = initData.TimeToMove;
            GridPosition = initData.GridPosition;
           
            _renderParent = initData.RenderParent;
        }

        public void SetMaterial(MaterialHolder.UniqueMaterial material)
        {
            meshRenderer.sharedMaterial = material.Material;
            Color = material.Material.color;
            
            trailRenderer.startColor = Color;
            trailRenderer.endColor = Color;
            
            trailRenderer.enabled = true;
        }

        public void GoToPlace()
        {
            transform.DOMove(_endPosition,_timeToMove)
                .OnComplete(() =>
                {
                    transform.SetParent(_renderParent);
                    transform.position = _renderPosition;
                    trailRenderer.enabled = false;
                });
        }
    }
}