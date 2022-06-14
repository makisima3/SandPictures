using System.Collections.Generic;
using Code.InitDatas;
using DG.Tweening;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.Levels
{
    [RequireComponent(typeof(Grid))]
    public class Vessel : MonoBehaviour, IInitialized<VesselInitData>
    {
        [SerializeField] private float validCellPoint = 1f;
        [SerializeField] private float invalidCellPoint = 1f;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform spawnPointView;
        [SerializeField] private Transform view;
        [SerializeField] private float spawnPointOffset = 2f;
        [SerializeField] private Transform grainsHolder;
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        
        private ResultRenderer _renderer;
        private Grid _grid;

        private float _dropGrainTime;
        private SimpleFactory _worldFactory;
        private Grain[,] _grains;
        private Vector2Int _size;

        private float _step;

        public void Initialize(VesselInitData initData)
        {
            _grid = GetComponent<Grid>();
            _grains = new Grain[initData.Size.x, initData.Size.y];
            _dropGrainTime = initData.DropGrainTime;
            _worldFactory = initData.WorldFactory;
            _renderer = initData.ResultRenderer;
            _size = initData.Size;
            spawnPointView.transform.position = pointA.position;
            
            var distance = Vector3.Distance(pointA.position, pointB.position);
            _step = distance / initData.Size.x;

            
        }

        public void Move(Vector2Int position, float time)
        {
            var pos = spawnPointView.position;
            pos.x = GetWorldPosition(position).x;
            spawnPointView.DOMove(pos,time);
        }
        
        public void SpawnGrain(Cell cell, MaterialHolder.UniqueMaterial material,float time)
        {
            SoundManager.Instance.PlaySound();
            Move(cell.Position, time);
            //spawnPointView.position = pointA.position + Vector3.left * ( _size.x - cell.Position.x) * _step;

            var sposition = spawnPointView.position;
            sposition.x = _renderer.GetPosition(cell.Position).x;
            sposition.y = _renderer.GetPosition(Vector2Int.up * 100).y;
            
            
            var grain = _worldFactory.Create<Grain, GrainInitData>(new GrainInitData()
            {
                UniqueMaterial = material,
                EndPosition = _renderer.GetPosition(cell.Position),
                SpawnPosition = sposition,
                TimeToMove = _dropGrainTime,
                RenderParent = _renderer.Holder,
                RenderPosition = _renderer.GetPosition(cell.Position),
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

        private Cell[,] _cells;
        
        public float CompareResultV2(Cell[,] cells)
        {
            _cells = cells;
            var correctGrainsCount = 0f;

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    if (cells[x, y].Color == _grains[x, y].Color)
                        correctGrainsCount += validCellPoint;
                    else
                        correctGrainsCount -= invalidCellPoint;
                }
            }

            return correctGrainsCount / (cells.Length * validCellPoint);
        }

        [ContextMenu("Compare")]
        private void Test()
        {
            Debug.Log(CompareResultV2(_cells));
        }
    }
}