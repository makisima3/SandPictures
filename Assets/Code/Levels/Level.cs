using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using Code.UI;
using Code.Utils;
using CorgiFallingSands;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using Plugins.SimpleFactory;
using Unity.Mathematics;
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
        [SerializeField] private MeshRenderer TipMeshRenderer;
        [SerializeField] private int RowCount = 3;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField] private Transform camera;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float cameraMoveTime = 1f;
        [SerializeField] private HapticTypes SelectColorHapticType;
        [SerializeField] private HapticTypes levelEndHapticType;
        [SerializeField] private int levelEndHapticCount = 3;
        [SerializeField] private float levelEndHapticDelay = 0.1f;
        [SerializeField] private int toolOffset;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private Transform bottomCollider;
        [SerializeField] private float offset;
        [SerializeField] private int chunk;
        [SerializeField] private FallingSandsStorageManager fallingSandsStorageManager;
        [SerializeField] private GameObject panel;

        public FallingSandsStorageManager FallingSandsStorageManager => fallingSandsStorageManager;

        private ColorsSelector _colorsSelector;
        private TutorialView tutorialView;
        private Coroutine spawnCoroutine;
        private Cell[,] _cells;
        private List<Color> _uniqueColors;
        [SerializeField]private Texture2D _resultTargetTexture;
        [SerializeField]private Texture2D _resultColbasTexture;
        [SerializeField]private Texture2D _tipColbasTexture;
        [SerializeField] private FallingSandsDataObj fallingSandsDataObjPrefabAir;
        [SerializeField] private FallingSandsDataObj fallingSandsDataObjPrefabBad;
        [SerializeField] private FallingSandsDataObj fallingSandsDataObjPrefab;
        [SerializeField] private bool isInEditor;

        [SerializeField] private Camera resultCameraRenderer;
        
        private MaterialHolder.UniqueMaterial _currentMaterial;
        private bool _isSpawn;
        private bool _isEnded;
        private LevelCompleteView _levelCompleteView;
        [SerializeField]private IOrderedEnumerable<IGrouping<Color, Cell>> _zones;
        private int currentZoneIndex = 0;
        private Image _targetImage;
        [field: SerializeField] public MaterialHolder MaterialHolder { get; private set; }
        public UnityEvent<bool> OnSpawnStateChange;
        public Vector2Int Size => new Vector2Int(_cells.GetLength(0), _cells.GetLength(1));

        private UnityEvent onMoveEnd;
        private bool isMoveEnd;
        private int _level;
        private Color[] tipColors;
        private Color[] freeColors;
        public void Initialize(LevelInitData initData)
        {
            _level = initData.Level;
            _colorsSelector = initData.ColorsSelector;
            _levelCompleteView = initData.LevelCompleteView;
            tutorialView = initData.TutorialView;
            _targetImage = initData.TargetImage;
            
            onMoveEnd = new UnityEvent();
            onMoveEnd.AddListener(() => isMoveEnd = true);
            
            //MaterialHolder = new MaterialHolder();
            GetCells(baseTexture);
            
            _targetImage.sprite = Sprite
                .Create(baseTexture, new Rect(0f, 0f, _resultTargetTexture.width, _resultTargetTexture.height),
                    Vector2.one / 2);
            
           

            var objs = new List<FallingSandsDataObj>();

            int counter = 2;
            foreach (var color in MaterialHolder._uniqueColors)
            {
                var material = new Material(initData.BaseMaterial)
                {
                    color = color.Color
                };

                var newPrefab = new FallingSandsDataObj();
                newPrefab.descKey = fallingSandsDataObjPrefab.descKey;
                newPrefab.nameKey = fallingSandsDataObjPrefab.nameKey;
                newPrefab.Icon = fallingSandsDataObjPrefab.Icon;
                newPrefab.Id = fallingSandsDataObjPrefab.Id;
                newPrefab.listBias = fallingSandsDataObjPrefab.listBias;
                newPrefab.Metadata = fallingSandsDataObjPrefab.Metadata;
                newPrefab.OnStamp = fallingSandsDataObjPrefab.OnStamp;
                newPrefab.OnTempHigh = fallingSandsDataObjPrefab.OnTempHigh;
                newPrefab.OnTempLow = fallingSandsDataObjPrefab.OnTempLow;
                newPrefab.ShowInPicker = fallingSandsDataObjPrefab.ShowInPicker;
                
                newPrefab.Metadata.Color = color.Color;
                newPrefab.Id = counter;
                newPrefab.ShowInPicker = color.ShowInButtons;
                
                counter++;
                objs.Add(newPrefab);
                MaterialHolder.Register(new MaterialHolder.UniqueMaterial(color.Id, material));
                
            }
            
            FallingSandsDataManager.Instance.DataObjects.Clear();
            fallingSandsDataObjPrefabAir.ShowInPicker = false;
            fallingSandsDataObjPrefabBad.ShowInPicker = false;
            FallingSandsDataManager.Instance.DataObjects.Add(fallingSandsDataObjPrefabAir);
            FallingSandsDataManager.Instance.DataObjects.Add(fallingSandsDataObjPrefabBad);
            FallingSandsDataManager.Instance.DataObjects.AddRange(objs);
            
            /*_zones = _cells
                .Cast<Cell>()
                .Where(c => !c.IsEmpty)
                .GroupBy(c => c.Color)
                .OrderBy(c => MaterialHolder.GetMaterialID(c.Key));*/


           /* vessel.Initialize(new VesselInitData()
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

            freeColors = _tipColbasTexture.GetPixels();
            
            TipZone();
            
            _currentMaterial = MaterialHolder.UniqueMaterials.First();
            toolView.SetToolPipe(MaterialHolder.UniqueMaterials.First().Color);*/
        }

        [ContextMenu("PreInit")]
        private void PreInit()
        {
            MaterialHolder = new MaterialHolder();
            GetCells(baseTexture);
            _zones = _cells
                .Cast<Cell>()
                .Where(c => !c.IsEmpty)
                .GroupBy(c => c.Color)
                .OrderBy(z => z.Sum(c => c.Position.y) / z.Count());
            MaterialHolder._uniqueColors = new List<MaterialHolder.UniqueColor>();
            int id = 0;
            foreach (var color in _zones)
            {
                MaterialHolder._uniqueColors.Add(new MaterialHolder.UniqueColor(id, color.Key,false));
                id++;
            }
        }

        public void StartSpawn()
        {
            if (_isEnded)
                return;
            
            StopTip();
            _isSpawn = true;
            OnSpawnStateChange.Invoke(_isSpawn);
            spawnCoroutine = StartCoroutine(SpawnV5());
            
            SoundManager.Instance.StartPlay();
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
            
            SoundManager.Instance.StopPlay();
        }

        public void SelectMaterial(MaterialHolder.UniqueMaterial material)
        {
            MMVibrationManager.Haptic(SelectColorHapticType);
            _currentMaterial = material;
            toolView.SetToolPipe(material.Color);
            StopTip();
        }

        private int _zoneCounter;

        private void GetCells(Texture2D baseTex)
        {
            _resultTargetTexture = new Texture2D(baseTex.width, baseTex.height, TextureFormat.ARGB32, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Mirror
            };
            _resultColbasTexture = new Texture2D(baseTex.width, baseTex.height, TextureFormat.ARGB32, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Mirror
            };
            _resultColbasTexture.SetPixels(0, 0, _resultColbasTexture.width, _resultColbasTexture.height, Enumerable
                .Repeat(
                    new Color(0, 0, 0, 0), _resultColbasTexture.width * _resultColbasTexture.height).ToArray());
            _resultColbasTexture.Apply();
            
            _tipColbasTexture = new Texture2D(baseTex.width, baseTex.height, TextureFormat.ARGB32, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Mirror
            };
            _tipColbasTexture.SetPixels(0, 0, _tipColbasTexture.width, _tipColbasTexture.height, Enumerable
                .Repeat(
                    new Color(0, 0, 0, 0), _tipColbasTexture.width * _tipColbasTexture.height).ToArray());
            _tipColbasTexture.Apply();
            

            resultMeshRenderer.sharedMaterial.mainTexture = _resultColbasTexture;
            TipMeshRenderer.sharedMaterial.mainTexture = _tipColbasTexture;
            _cells = new Cell[_resultTargetTexture.width, _resultTargetTexture.height];
            _uniqueColors = new List<Color>();

            for (int x = 0; x < _resultTargetTexture.width; x++)
            {
                for (int y = 0; y < _resultTargetTexture.height; y++)
                {
                    var color = baseTex.GetPixel(x, y);

                    if (color.a < float.Epsilon)
                    {
                        _cells[x, y] = new Cell()
                        {
                            Position = new Vector2Int(x, y),
                            IsEmpty = true
                        };

                        continue;
                    }

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

        [SerializeField] private float costPos = 1f;
        [SerializeField] private float costNeg = 1f;
        [SerializeField] private float thresholdCompare;
        [ContextMenu("CompareResult")]
        public float CompareResult()
        {
            var correctGrainsCount = 0f;
            var texture = RTImage();
           
            for (int x = 0; x < _resultTargetTexture.width; x++)
            {
                for (int y = 0; y < _resultTargetTexture.height; y++)
                {
                    var c = texture.GetPixel(x, y);

                    if (c.a <= 0 || c == Color.black)
                    {
                        continue;
                    }
                    
                    if (_resultTargetTexture.GetPixel(x, y).ToVector4().DistanceTo(texture.GetPixel(x, y).ToVector4()) <= thresholdCompare )
                        correctGrainsCount+= costPos;
                    else
                    {
                        correctGrainsCount-= costNeg;
                    }
                }
            }
            
            Debug.Log(correctGrainsCount / (_resultTargetTexture.width * _resultTargetTexture.height));
            return correctGrainsCount / (_resultTargetTexture.width * _resultTargetTexture.height);
        }

        private Texture2D RTImage()
        {
            resultCameraRenderer.gameObject.SetActive(true);
            Rect rect = new Rect(0, 0, _resultTargetTexture.width, _resultTargetTexture.height);
            RenderTexture renderTexture = new RenderTexture(_resultTargetTexture.width, _resultTargetTexture.height, 24);
            Texture2D screenShot = new Texture2D(_resultTargetTexture.width, _resultTargetTexture.height, TextureFormat.RGBA32, false);
 
            resultCameraRenderer.targetTexture = renderTexture;
            resultCameraRenderer.Render();
 
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(rect, 0, 0);
 
            resultCameraRenderer.targetTexture = null;
            RenderTexture.active = null;
 
            Destroy(renderTexture);
            renderTexture = null;
            
            resultCameraRenderer.gameObject.SetActive(false);
            return screenShot;
        }
        
        public void LevelEnd()
        {
            panel.gameObject.SetActive(false);
            _isEnded = true;
            confetti.Play();
            var percetn = CompareResult();
            _levelCompleteView.Show(Mathf.Clamp(percetn,0,1));
            camera.DOMove(endPoint.position, cameraMoveTime);
            camera.DORotateQuaternion(endPoint.rotation, cameraMoveTime);
            _isSpawn = false;
            _colorsSelector.gameObject.SetActive(false);
            toolView.gameObject.SetActive(false);
            SoundManager.Instance.StopPlay();
            StartCoroutine(LevelEndVibration());
            TipMeshRenderer.gameObject.SetActive(false);
        }
        
        private IEnumerator SpawnV5()
        {
            while (_isSpawn)
            {
                _resultColbasTexture.Apply();
                //Проверка закрашена ли зона
                var isZoneCompleted = _zones.Skip(currentZoneIndex).First().All(c => c.IsSpawned);

                if (isZoneCompleted)
                {
                    currentZoneIndex += 1;
                    
                    if (currentZoneIndex >= _zones.Count())
                    {
                        _isEnded = true;
                        confetti.Play();
                        var percetn = CompareResult();
                        _levelCompleteView.Show(percetn);
                        camera.DOMove(endPoint.position, cameraMoveTime);
                        camera.DORotateQuaternion(endPoint.rotation, cameraMoveTime);
                        _isSpawn = false;
                        _colorsSelector.gameObject.SetActive(false);
                        toolView.gameObject.SetActive(false);
                        SoundManager.Instance.StopPlay();
                        StartCoroutine(LevelEndVibration());
                        TipMeshRenderer.gameObject.SetActive(false);
                        yield break;
                    }
                    TipZone();
                    if (_zones.Skip(currentZoneIndex).First().First().Color !=
                        _zones.Skip(currentZoneIndex - 1).First().First().Color)
                    {
                        _isSpawn = false;
                        OnSpawnStateChange.Invoke(_isSpawn);
                        confetti.Play();
                        SoundManager.Instance.StopPlay();
                        if (_level == 0 && currentZoneIndex == 1)
                            tutorialView.ShowV3();
                        break;
                    }
                }

                var rows = _zones
                    .Skip(currentZoneIndex)
                    .First()
                    .Where(c => !c.IsSpawned)
                    .GroupBy(c => c.Position.y)
                    .OrderBy(c => c.Key)
                    .ToArray();

                
                
                Vector2Int lastCell = Vector2Int.zero;
                for (int i = 0; i < rows.Length; i += RowCount)
                {
                    if (!_isSpawn)
                    {
                        break;
                    }

                    List<Cell> rowsGroup;
                    if (_isLeftFirst)
                        rowsGroup = rows.Skip(i).Take(RowCount).SelectMany(c => c).OrderByDescending(c => c.Position.x)
                            .ToList();
                    else
                        rowsGroup = rows.Skip(i).Take(RowCount).SelectMany(c => c).OrderBy(c => c.Position.x).ToList();

                    var pos = bottomCollider.transform.position;
                    pos.y = rowsGroup.First().Position.y * offset;
                    bottomCollider.transform.position = pos;
                    
                    var counter = oneStepSpawnGrainsCount;
                    var xMax = rowsGroup.Max(r => r.Position.x);
                    var xMin = rowsGroup.Min(r => r.Position.x);
                    for (int j = 0; j < rowsGroup.Count(); j++)
                    {
                        if (!_isSpawn)
                        {
                            break;
                        }

                        counter--;

                        _resultColbasTexture.SetPixel(rowsGroup[j].Position.x, rowsGroup[j].Position.y,
                            _currentMaterial.Color);
                        rowsGroup[j].IsSpawned = true;

                        if (counter >= 0)
                            continue;

                        
                        isMoveEnd = false;
                        var offset = _isLeftFirst ? -toolOffset : toolOffset;
                        vessel.Move(rowsGroup[j].Position + Vector2Int.right * offset, dropRate, _currentMaterial.Color, xMax,xMin)
                            .OnComplete(onMoveEnd.Invoke);

                        _resultColbasTexture.Apply();
                        counter = oneStepSpawnGrainsCount;

                        yield return new WaitUntil(() => isMoveEnd);
                    }

                    _isLeftFirst = !_isLeftFirst;
                }
            }
        }

        private int lastTippedZone = -1;
        private Tween _tween;
        [SerializeField] private float fadeForce;
        [SerializeField] private float fadeTime;
        private void TipZone()
        {
            if (lastTippedZone == currentZoneIndex)
                return;
            TipMeshRenderer.sharedMaterial.DOFade(0, 0);
            TipMeshRenderer.gameObject.SetActive(true);
            lastTippedZone = currentZoneIndex;
            tipColors = freeColors;
            var zone = _zones.Skip(currentZoneIndex).First();
            var color = new Color(1, 1, 1, 1);
            foreach (var cell in zone)
            {
                tipColors[_resultColbasTexture.width * cell.Position.y + cell.Position.x] = color;
            }

            _tipColbasTexture.SetPixels(tipColors);
            _tipColbasTexture.Apply();

            _tween = TipMeshRenderer.sharedMaterial.DOFade(fadeForce, fadeTime).SetLoops(-1, LoopType.Yoyo);
        }

        private void StopTip()
        {
            if(_tween != null)
                _tween.Kill();
            
            TipMeshRenderer.gameObject.SetActive(false);
        }
        
        private IEnumerator LevelEndVibration()
        {
            for (int i = 0; i < levelEndHapticCount; i++)
            {
                MMVibrationManager.Haptic(levelEndHapticType);

                yield return new WaitForSeconds(levelEndHapticDelay);
            }
        }
        
    }
}