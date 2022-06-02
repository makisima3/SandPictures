using System.Collections.Generic;
using Code.InitDatas;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.Levels
{
    [RequireComponent(typeof(Grid))]
    public class Vessel : MonoBehaviour, IInitialized<VesselInitData>
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform view;
        [SerializeField] private float spawnPointOffset = 2f;
        [SerializeField] private Transform grainsHolder;
        
        private Grid _grid;

        private float _dropGrainTime;
        private SimpleFactory _worldFactory;
        private Grain[,] _grains;
        public void Initialize(VesselInitData initData)
        {
            _grid = GetComponent<Grid>();
            _grains = new Grain[initData.Size.x,initData.Size.y];
            _dropGrainTime = initData.DropGrainTime;
            _worldFactory = initData.WorldFactory;
            
            view.localScale = new Vector3(initData.Size.x, initData.Size.y, 0.2f);
            view.position = new Vector3(view.localScale.x / 2, view.localScale.y / 2, 0f);
            
            spawnPoint.position += Vector3.up * initData.Size.y + Vector3.up * spawnPointOffset;
        }

        public void SpawnGrain(Cell cell,MaterialHolder.UniqueMaterial material)
        {
            var position = spawnPoint.position;
            position.x = GetWorldPosition(cell.Position).x;
            spawnPoint.position = position;
            
            var grain = _worldFactory.Create<Grain, GrainInitData>(new GrainInitData()
            {
                UniqueMaterial = material,
                EndPosition = GetWorldPosition(cell.Position),
                SpawnPosition = spawnPoint.position,
                TimeToMove = _dropGrainTime
            });
            
            grain.transform.SetParent(grainsHolder);
            grain.GoToPlace();

            _grains[cell.Position.x, cell.Position.y] = grain;
        }

        private Vector3 GetWorldPosition(Vector2Int position)
        {
            return _grid.CellToWorld((Vector3Int) position);
        }

        public float CompareResult(Cell[,] cells)
        {
            var correctGrainsCount = 0f;
            
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    if (cells[x, y].Color == _grains[x, y].Color)
                        correctGrainsCount++;
                }
            }

            return correctGrainsCount / cells.Length;
        }
    }
}