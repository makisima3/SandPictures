using Code.InitDatas;
using Plugins.SimpleFactory;
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

        public void Initialize(ResultRendererInitData initData)
        {
            _grid = GetComponent<Grid>();
            
            camera.Initialize(new CameraControllerInitData()
            {
                Size = initData.Size,
            });
        }

        public Vector3 GetPosition(Vector2Int position)
        {
            return _grid.CellToWorld((Vector3Int) position);
        }
    }
}