using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using Code.UI;
using Code.Utils;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.Levels
{
    public class Level : MonoBehaviour, IInitialized<LevelInitData>
    {
        public class SplitedZone
        {
            public int Index { get; set; }
            public List<List<Cell>> Zones { get; set; }
        }

        [SerializeField] private Texture2D baseTexture;
        [SerializeField] private Vessel vessel;
        
        [SerializeField] private float dropRate = 0.5f;
        [SerializeField] private float dropGrainTime;
        [SerializeField] private float newRowReload = 1f;
        [SerializeField, Range(0f, 1f)] private float threshold;
        [SerializeField] private bool _isLeftFirst;
        [SerializeField] private int oneStepSpawnGrainsCount = 2;
        [SerializeField] private ToolView toolView;
        [SerializeField] private int maxZoneLength = 5000;
        [SerializeField] private MeshRenderer resultMeshRenderer;


        private Coroutine spawnCoroutine;
        private Cell[,] _cells;
        private List<Color> _uniqueColors;
        private Texture2D _resultTargetTexture;
        private Texture2D _resultColbasTexture;
        private MaterialHolder.UniqueMaterial _currentMaterial;
        private bool _isSpawn;
        private bool _isEnded;
        private LevelCompleteView _levelCompleteView;
        private IOrderedEnumerable<IGrouping<Color, Cell>> _zones;
        private int currentZoneIndex = 0;
        private Image _targetImage;
        [field: SerializeField] public MaterialHolder MaterialHolder { get; private set; }
        public UnityEvent<bool> OnSpawnStateChange;
        public Vector2Int Size => new Vector2Int(_cells.GetLength(0), _cells.GetLength(1));

        public void Initialize(LevelInitData initData)
        {
            MaterialHolder = new MaterialHolder();
            _levelCompleteView = initData.LevelCompleteView;
            _targetImage = initData.TargetImage;

            GetCells(baseTexture);
            _targetImage.sprite = Sprite
                .Create(_resultTargetTexture, new Rect(0f, 0f, _resultTargetTexture.width, _resultTargetTexture.height),
                    Vector2.one / 2);

            int id = 0;
            foreach (var color in _uniqueColors)
            {
                var material = new Material(initData.BaseMaterial)
                {
                    color = color,
                };

                MaterialHolder.Register(new MaterialHolder.UniqueMaterial(id, material));
                id++;
            }

           

            vessel.Initialize(new VesselInitData()
            {
                WorldFactory = initData.WorldFactory,
                DropGrainTime = dropGrainTime,
                Size = new Vector2Int(_cells.GetLength(0), _cells.GetLength(1)),
                Cells = _cells,
                OnSpawnStateChange = OnSpawnStateChange
            });

            toolView.Initialize(new ToolViewInitData()
            {
                Colors = _uniqueColors.ToList(),
                firstColor = _uniqueColors.First()
            });
            
            _zones = _cells
                .Cast<Cell>()
                .GroupBy(c => c.Color)
                .OrderBy(z => z.Sum(c => c.Position.y) / z.Count());
        }

        public void StartSpawn()
        {
            if (_isEnded)
                return;

            _isSpawn = true;
            OnSpawnStateChange.Invoke(_isSpawn);
            spawnCoroutine = StartCoroutine(SpawnV5());
        }

        public void StopSpawn()
        {
            _isSpawn = false;
            OnSpawnStateChange.Invoke(_isSpawn);
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        public void SelectMaterial(MaterialHolder.UniqueMaterial material)
        {
            _currentMaterial = material;
            toolView.SetToolPipe(material.Color);
        }

        private int _zoneCounter;

        private void GetCells(Texture2D baseTex)
        {
            _resultTargetTexture = new Texture2D(baseTex.width, baseTex.height, TextureFormat.ARGB32, true)
            {
                filterMode = FilterMode.Point
            };
            _resultColbasTexture = new Texture2D(baseTex.width, baseTex.height * 2, TextureFormat.ARGB32, true)
            {
                filterMode = FilterMode.Point
            };
            _resultColbasTexture.SetPixels(0,0,_resultColbasTexture.width,_resultColbasTexture.height,Enumerable.Repeat(
                new Color(0,0,0,0),_resultColbasTexture.width * _resultColbasTexture.height).ToArray());
            _resultColbasTexture.Apply();
            
            resultMeshRenderer.sharedMaterial.mainTexture = _resultColbasTexture;
            _cells = new Cell[_resultTargetTexture.width, _resultTargetTexture.height];
            _uniqueColors = new List<Color>();

            for (int x = 0; x < _resultTargetTexture.width; x++)
            {
                for (int y = 0; y < _resultTargetTexture.height; y++)
                {
                    var color = baseTex.GetPixel(x, y);

                    var uniqueColor = _uniqueColors.FirstOrDefault(c => CheckThreshold(c, color));

                    if (uniqueColor != default)
                    {
                        color = uniqueColor;
                    }
                    else
                    {
                        _uniqueColors.Add(color);
                    }

                    _cells[x, y] = new Cell()
                    {
                        Position = new Vector2Int(x, y),
                        Color = color
                    };

                    _resultTargetTexture.SetPixel(x, y, color);
                }
            }

            _resultTargetTexture.Apply();
        }

        private bool CheckThreshold(Color color1, Color color2)
        {
            return color1.ToVector4().DistanceTo(color2.ToVector4()) < threshold;
        }

        private int step = -1;

        private Cell GetCell(Vector2Int position)
        {
            if (position.x < 0 || position.x >= _cells.GetLength(0))
                return null;

            if (position.y < 0 || position.y >= _cells.GetLength(1))
                return null;

            return _cells[position.x, position.y];
        }

        private IEnumerator SpawnV5()
        {
            while (_isSpawn)
            {
                var t = true;
                foreach (var cellGroup in _zones.Skip(currentZoneIndex).First().GroupBy(c => c.Position.y))
                {
                    if (cellGroup.All(c => c.IsSpawned))
                        continue;

                    t = false;
                }

                if (t)
                {
                    currentZoneIndex += 1;

                    if (_cells.Cast<Cell>().All(c => c.IsSpawned))
                    {
                        _isEnded = true;

                        var percetn = vessel.CompareResultV2(_cells);
                        _levelCompleteView.Show(percetn);
                        _isSpawn = false;
                        yield break;
                    }
                    
                    if (_zones.Skip(currentZoneIndex).First().First().Color !=
                        _zones.Skip(currentZoneIndex - 1).First().First().Color)
                    {
                        _isSpawn = false;
                        break;
                    }
                }


                foreach (var cellGroup in _zones.Skip(currentZoneIndex).First().Where(c => !c.IsSpawned)
                    .GroupBy(c => c.Position.y).OrderBy(c => c.Key))
                {
                    if(cellGroup.All(c => c.IsSpawned))
                        continue;
                    
                    var group = cellGroup.OrderBy(c => c.Position.x);

                    if (_isLeftFirst)
                        group = group.OrderByDescending(c => c.Position.x);

                    _isLeftFirst = !_isLeftFirst;

                    vessel.Move(group.First().Position, newRowReload,_currentMaterial.Color);
                    yield return new WaitForSeconds(newRowReload);

                    var counter = oneStepSpawnGrainsCount;
                    foreach (var cell in group)
                    {
                        if (!_isSpawn)
                            break;
                        
                        counter--;

                        vessel.Move(cell.Position, dropRate,_currentMaterial.Color);
                        _resultColbasTexture.SetPixel(cell.Position.x, cell.Position.y, _currentMaterial.Color);
                        
                        cell.IsSpawned = true;
                        
                         var c = GetCell(cell.Position + Vector2Int.up);
                         if (c != null && c.Color == cell.Color)
                         {
                             _resultColbasTexture.SetPixel(c.Position.x, c.Position.y, _currentMaterial.Color);
                             c.IsSpawned = true;
                         }


                         if (counter >= 0) continue;
                         
                        _resultColbasTexture.Apply();
                        counter = oneStepSpawnGrainsCount;
                        
                        yield return null;
                    }
                }
            }
        }
    }
}