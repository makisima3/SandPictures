using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using Code.UI;
using Code.Utils;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Levels
{
    public class Level : MonoBehaviour, IInitialized<LevelInitData>
    {
        [SerializeField] private Texture2D baseTexture;
        [SerializeField] private Vessel vessel;
        [SerializeField] private ResultRenderer renderer;
        [SerializeField] private float dropRate = 0.5f;
        [SerializeField] private float dropGrainTime;
        [SerializeField] private float newRowReload = 1f;
        [SerializeField, Range(0f, 1f)] private float threshold;
        [SerializeField] private bool _isLeftFirst;
        [SerializeField] private int oneStepSpawnGrainsCount = 2;

        private Coroutine spawnCoroutine;
        private Cell[,] _cells;
        private MaterialHolder.UniqueMaterial _currentMaterial;
        private bool _isSpawn;
        private bool _isEnded;
        private int x;
        private int y;
        private LevelCompleteView _levelCompleteView;
        private IOrderedEnumerable<IGrouping<Color, Cell>> _zones;
        private int currentZoneIndex =0;

        [field: SerializeField] public MaterialHolder MaterialHolder { get; private set; }
        public Vector2Int Size => new Vector2Int(_cells.GetLength(0), _cells.GetLength(1));

        public void Initialize(LevelInitData initData)
        {
            _cells = ImageConverter.GetCells(baseTexture);
            MaterialHolder = new MaterialHolder();
            _levelCompleteView = initData.LevelCompleteView;

            var filteredCells = FilterCells();
            var uniqueColors = GetUniqueColors(filteredCells);

            var texture = GetTexture(filteredCells, baseTexture);
            initData.TargetImage.sprite =
                Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one / 2);

            int id = 0;
            foreach (var color in uniqueColors)
            {
                var material = new Material(initData.BaseMaterial)
                {
                    color = color,
                };

                MaterialHolder.Register(new MaterialHolder.UniqueMaterial(id, material));
                id++;
            }

            x = _cells.GetLength(0) - 1;
            y = 0;

            renderer.Initialize(new ResultRendererInitData()
            {
                Size = Size,
            });

            vessel.Initialize(new VesselInitData()
            {
                WorldFactory = initData.WorldFactory,
                DropGrainTime = dropGrainTime,
                Size = new Vector2Int(_cells.GetLength(0), _cells.GetLength(1)),
                ResultRenderer = renderer,
                Cells = _cells
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

            spawnCoroutine = StartCoroutine(SpawnV3());
        }

        public void StopSpawn()
        {
            _isSpawn = false;

            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        public void SelectMaterial(MaterialHolder.UniqueMaterial material) => _currentMaterial = material;

        private HashSet<Color> GetUniqueColors(Cell[] cells) => cells.Select(c => c.Color).ToHashSet();

        private Texture2D GetTexture(Cell[] cells, Texture2D baseTex)
        {
            var texture = new Texture2D(baseTex.width, baseTex.height, TextureFormat.ARGB32, true)
            {
                filterMode = FilterMode.Point
            };

            foreach (var cell in cells)
            {
                texture.SetPixel(cell.Position.x, cell.Position.y, cell.Color);
            }

            texture.Apply();
            return texture;
        }

        private Cell[] FilterCells()
        {
            var cells = _cells.Cast<Cell>().ToArray();

            for (int i = 0; i < cells.Length; i++)
            {
                var cell1 = cells[i];

                for (int j = i; j < cells.Length; j++)
                {
                    var cell2 = cells[j];

                    if (cell1.Color.ToVector4().DistanceTo(cell2.Color.ToVector4()) < threshold)
                    {
                        cell2.Color = cell1.Color;
                    }
                }
            }

            foreach (var cell in _cells)
            {
                var ca = cells.First(c => c.Position == cell.Position);
                cell.Color = ca.Color;
            }

            return cells;
        }

        private int step = -1;

        private IEnumerator Spawn()
        {
            while (_isSpawn)
            {
                for (int i = x; i >= 0 && i < _cells.GetLength(0); i += step)
                {
                    vessel.SpawnGrain(_cells[i, y], _currentMaterial, dropRate);

                    x += step;
                    yield return new WaitForSeconds(dropRate);
                }

                step *= -1;

                x = step > 0 ? 0 : _cells.GetLength(0) - 1;
                y++;

                if (y >= _cells.GetLength(1))
                {
                    _isEnded = true;

                    var percetn = vessel.CompareResultV2(_cells);
                    _levelCompleteView.Show(percetn);

                    yield break;
                }

                yield return new WaitForSeconds(newRowReload);
            }
        }

        private IEnumerator SpawnV2()
        {
            while (_isSpawn)
            {
                var cells = _cells
                    .Cast<Cell>()
                    .Where(c => c.Color == _currentMaterial.Color && !c.IsSpawned)
                    .GroupBy(c => c.Position.y)
                    .OrderBy(c => c.Key);

                if (!cells.Any())
                {
                    _isSpawn = false;
                    break;
                }

                foreach (var cellGroup in cells)
                {
                    vessel.Move(cellGroup.First().Position, newRowReload);
                    yield return new WaitForSeconds(newRowReload);

                    foreach (var cell in cellGroup.OrderBy(c => c.Position.x))
                    {
                        if (!_isSpawn)
                            break;

                        vessel.SpawnGrain(cell, _currentMaterial, dropRate);
                        cell.IsSpawned = true;
                        yield return new WaitForSeconds(dropRate);
                    }
                }

                if (_cells.Cast<Cell>().All(c => c.IsSpawned))
                {
                    _isEnded = true;

                    var percetn = vessel.CompareResultV2(_cells);
                    _levelCompleteView.Show(percetn);

                    yield break;
                }
            }
        }


        private IEnumerator SpawnV3()
        {
            while (_isSpawn)
            {
                var t = true;
                foreach (var cellGroup in _zones.Skip(currentZoneIndex).First().GroupBy(c => c.Position.y))
                {
                    if(cellGroup.All(c => c.IsSpawned))
                        continue;

                    t = false;
                }

                if (t)
                {
                    _isSpawn = false;
                    currentZoneIndex += 1;

                    if (_cells.Cast<Cell>().All(c => c.IsSpawned))
                    {
                        _isEnded = true;

                        var percetn = vessel.CompareResultV2(_cells);
                        _levelCompleteView.Show(percetn);

                        yield break;
                    }

                    break;
                }
                
                foreach (var cellGroup in _zones.Skip(currentZoneIndex).First().Where(c => !c.IsSpawned).GroupBy(c => c.Position.y).OrderBy(c => c.Key))
                {
                    var group = cellGroup.OrderBy(c => c.Position.x);

                    if (_isLeftFirst)
                        group = group.OrderByDescending(c => c.Position.x);
                    
                    _isLeftFirst = !_isLeftFirst;
                    
                    vessel.Move(group.First().Position, newRowReload);
                    yield return new WaitForSeconds(newRowReload);


                    //var counter = oneStepSpawnGrainsCount;
                    foreach (var cell in group)
                    {
                        if (!_isSpawn)
                            break;

                       // counter--;
                        vessel.SpawnGrain(cell, _currentMaterial, dropRate);
                        cell.IsSpawned = true;
                        //yield return new WaitForSeconds(dropRate);
                        /*if (counter >= 0) continue;
                        counter = oneStepSpawnGrainsCount;
                        yield return null;*/
                    }

                    
                }

                
            }
        }
    }
}