using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
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
        [SerializeField] private float dropRate = 0.5f;
        [SerializeField] private float dropGrainTime;
        [SerializeField] private float newRowReload = 1f;
        [SerializeField, Range(0f, 1f)] private float threshold;
        
        private Coroutine spawnCoroutine;
        private Cell[,] _cells;
        private MaterialHolder.UniqueMaterial _currentMaterial;
        private bool _isSpawn;
        private bool _isEnded;
        private int x;
        private int y;
        
        [field: SerializeField] public MaterialHolder MaterialHolder { get; private set; }
        public Vector2Int Size => new Vector2Int(_cells.GetLength(0), _cells.GetLength(1));
        
        public void Initialize(LevelInitData initData)
        {
            _cells = ImageConverter.GetCells(baseTexture);
            MaterialHolder = new MaterialHolder();

            var filteredCells = FilterCells();
            var uniqueColors = GetUniqueColors(filteredCells);

            var texture = GetTexture(filteredCells, baseTexture);
            initData.TargetImage.sprite = Sprite.Create(texture,new Rect(0f,0f,texture.width,texture.height),Vector2.one/2);

            int id = 0;
            foreach (var color in uniqueColors)
            {
                var material = new Material(initData.BaseMaterial)
                {
                    color = color,
                };
                
                MaterialHolder.Register(new MaterialHolder.UniqueMaterial(id,material));
                id++;
            }
            
            x = _cells.GetLength(0);
            y = 0;
            
            vessel.Initialize(new VesselInitData()
            {
                WorldFactory = initData.WorldFactory,
                DropGrainTime = dropGrainTime,
                Size = new Vector2Int(_cells.GetLength(0),_cells.GetLength(1)),
            });
        }

        public void StartSpawn()
        {
            if(_isEnded)
                return;
            
            _isSpawn = true;
            
            spawnCoroutine = StartCoroutine(Spawn());
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

        private HashSet<Color> GetUniqueColors(Cell[] cells) =>cells.Select(c => c.Color).ToHashSet();

        private Texture2D GetTexture(Cell[] cells,Texture2D baseTex)
        {
            var texture = new Texture2D(baseTex.width, baseTex.height, TextureFormat.ARGB32,true)
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
        

        private IEnumerator Spawn()
        {
            while (_isSpawn)
            {
                for (int i = x-1; i >= 0; i--)
                {
                    vessel.SpawnGrain(_cells[i,y], _currentMaterial);
                    
                    x--;
                    yield return new WaitForSeconds(dropRate);
                }

                x = _cells.GetLength(0);
                y++;

                if (y >= _cells.GetLength(1))
                {
                    _isEnded = true;

                    var percetn = vessel.CompareResult(_cells);
                    
                    Debug.Log(percetn);
                    
                    yield break;
                }
                yield return new WaitForSeconds(newRowReload);
            }
        }
    }
}