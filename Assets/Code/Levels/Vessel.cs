using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private ParticleSystem sandPS;
       

        private ResultRenderer _renderer;
        private Grid _grid;

        private float _dropGrainTime;
        private SimpleFactory _worldFactory;
        private Grain[,] _grains;
        private Vector2Int _size;
        private List<List<Cell>> _splitedZones;
        private float _step;
        
        private IOrderedEnumerable<IGrouping<Color, Cell>> _zones;
        private Dictionary<int, MeshCombiner> combiners;

        public void Initialize(VesselInitData initData)
        {
            combiners = new Dictionary<int, MeshCombiner>();

            _grid = GetComponent<Grid>();
            _grains = new Grain[initData.Size.x, initData.Size.y];
            _dropGrainTime = initData.DropGrainTime;
            _worldFactory = initData.WorldFactory;
            _renderer = initData.ResultRenderer;
            _size = initData.Size;
            _splitedZones = initData.SplitedZones;
            
            spawnPointView.transform.position = pointA.position;
            sandPS.gameObject.SetActive(false);

            initData.OnSpawnStateChange.AddListener((isSpawn) => { sandPS.gameObject.SetActive(isSpawn); });

            var distance = Vector3.Distance(pointA.position, pointB.position);
            _step = distance / initData.Size.x;

            int counter = 0;
            foreach (var zone in _splitedZones)
            {
                var combiner = CreateNewCombiner();
                combiners.Add(counter,combiner);
                foreach (var cell in zone)
                {
                    var grain = _worldFactory.Create<Grain, GrainInitData>(new GrainInitData()
                    {
                        EndPosition = _renderer.GetPosition(cell.Position),
                        //SpawnPosition = Vector3.one * 1000,
                        SpawnPosition = _renderer.GetPosition(cell.Position),
                        TimeToMove = _dropGrainTime,
                        RenderParent = _renderer.Holder,
                        RenderPosition = _renderer.GetPosition(cell.Position),
                    });

                    grain.gameObject.SetActive(false);
                    grain.transform.SetParent(combiner.transform);
                    _grains[cell.Position.x, cell.Position.y] = grain;
                }

                counter++;
            }

            /*foreach (var cell in initData.Cells)
            {
                var grain = _worldFactory.Create<Grain, GrainInitData>(new GrainInitData()
                {
                    EndPosition = _renderer.GetPosition(cell.Position),
                    //SpawnPosition = Vector3.one * 1000,
                    SpawnPosition = _renderer.GetPosition(cell.Position),
                    TimeToMove = _dropGrainTime,
                    RenderParent = _renderer.Holder,
                    RenderPosition = _renderer.GetPosition(cell.Position),
                });

                grain.gameObject.SetActive(false);
                grain.transform.SetParent(_renderer.Holder);
                _grains[cell.Position.x, cell.Position.y] = grain;
            }*/
        }

        private MeshCombiner CreateNewCombiner()
        {
            var newHodler = new GameObject();
            newHodler.transform.SetParent(_renderer.Holder);
            newHodler.transform.position = Vector3.zero;
            var combiner = newHodler.AddComponent<MeshCombiner>();
            combiner.CreateMultiMaterialMesh = true;
            combiner.DeactivateCombinedChildrenMeshRenderers = true;
            
            return combiner;
        }

        public void CombineGroup(int index)
        {
            combiners[index].CombineMeshes(false);
        }

        public void Move(Vector2Int position, float time)
        {
            var pos = spawnPointView.position;
            pos.x = (pointB.position + Vector3.right * (_step * position.x)).x;
            spawnPointView.DOMove(pos, time);

            pos = sandPS.transform.position;
            pos.x = _renderer.GetPosition(position).x;
            pos.y = _renderer.GetPosition(Vector2Int.up * _size.y * 2).y;
            sandPS.transform.DOMove(pos, time);
        }

        public void SpawnGrain(Cell cell, MaterialHolder.UniqueMaterial material, float time)
        {
            SoundManager.Instance.PlaySound();
            Move(cell.Position, time);

            sandPS.startColor = material.Color;

            var grain = _grains[cell.Position.x, cell.Position.y];
            grain.SetMaterial(material);
            grain.gameObject.SetActive(true);
            grain.GoToPlace();
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

        public float Remap(float value, float fromLower, float fromUpper, float toLower, float toUpper)
        {
            return (toUpper - toLower) * ((value - fromLower) / (fromUpper - fromLower)) + toLower;
        }
    }
}