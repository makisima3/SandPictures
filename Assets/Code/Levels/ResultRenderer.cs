using Code.InitDatas;
using Plugins.SimpleFactory;
using TMPro;
using UnityEngine;

namespace Code.Levels
{
    [RequireComponent(typeof(Grid))]
    public class ResultRenderer : MonoBehaviour, IInitialized<ResultRendererInitData>
    {
        [SerializeField] private CameraController camera;
        [SerializeField] private Transform holder;

        private Grid _grid;

        public Transform Holder => holder;
        public float GrainSize => _grid.cellSize.x;
            
        public void Initialize(ResultRendererInitData initData)
        {
            _grid = GetComponent<Grid>();
            
            camera.Initialize(new CameraControllerInitData()
            {
                Size = initData.Size,
                GrainSize = _grid.cellSize.x,
            });
        }

        public Vector3 GetPosition(Vector2Int position)
        {
            return _grid.CellToWorld((Vector3Int) position);
        }
    }
}