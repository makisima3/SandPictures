using System;
using System.Collections;
using System.Collections.Generic;
using Code.Factories;
using Code.InitDatas;
using Code.Levels;
using Code.StoragesObjects;
using Code.UI;
using CorgiFallingSands;
using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private InputCatcher _inputCatcher;
        [SerializeField] private List<Level> levels;
        [SerializeField] private WorldFactory worldFactory;
        [SerializeField] private UIFactory uiFactory;
        [SerializeField] private ColorsSelector colorsSelector;
        [SerializeField] private Material grainBaseMaterial;
        [SerializeField] private LevelCompleteView levelCompleteView;
        [SerializeField] private TutorialView tutorialView;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button levelEndButton;
        [SerializeField] private float timea = 30f; 
        [SerializeField] private float timeb = 15f;
        private LevelStorageObject _levelStorageObject;
        private Level level;
        public List<int> StoredChunks;

        private void Awake()
        {
            Application.targetFrameRate = 150;

            StoredChunks = GatherStoredChunks();
            levelEndButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(Restart);
            
            _levelStorageObject =
                PersistentStorage.PersistentStorage.Load<LevelStorageObject, LevelStorageObject.LevelData>(
                    new LevelStorageObject(new LevelStorageObject.LevelData() {Level = 0}));
            
            
            
            if (_levelStorageObject.Data.Level >= levels.Count)
            {
                _levelStorageObject.Data.Level = 0;
                PersistentStorage.PersistentStorage.Save<LevelStorageObject, LevelStorageObject.LevelData>(
                    _levelStorageObject);
            }

            level = LoadLevel();
            level.FallingSandsStorageManager.SetLoadLevel(_levelStorageObject.Data.Level);
            colorsSelector.Initialize(new ColorsSelectorInitData()
            {
                Level = level,
                UIFactory = uiFactory
            });

            levelCompleteView.Initialize(new LevelCompleteInitData()
            {
                ColorsSelector = colorsSelector,
            });
            _inputCatcher.Initialize(new InputCatcherInitData() {Level = level});

            tutorialView.Initialize(new LevelCompleteInitData()
            {
                ColorsSelector = colorsSelector,
            });
            
            if (_levelStorageObject.Data.Level == 0)
            {
                tutorialView.Show();
            }
            if (_levelStorageObject.Data.Level == 1)
            {
                tutorialView.ShowV3();
            }
            
            levelEndButton.onClick.AddListener(() =>
            {
                level.LevelEnd();
                levelEndButton.gameObject.SetActive(false);
            });
        }

        private void Start()
        {
            StartCoroutine(ActivateBtn());
        }

        private Level LoadLevel()
        {
            var level = levels[_levelStorageObject.Data.Level];

            var spawnedLevel = Instantiate(level.gameObject).GetComponent<Level>();
            spawnedLevel.Initialize(new LevelInitData()
            {
                WorldFactory = worldFactory,
                BaseMaterial = grainBaseMaterial,
                TargetImage = targetImage,
                LevelCompleteView = levelCompleteView,
                TutorialView = tutorialView,
                ColorsSelector = colorsSelector,
                Level = _levelStorageObject.Data.Level
            });
            return spawnedLevel;
        }

        private void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        private const string LevelDirectoryName = "Levels";
        private const string LevelsRegistryFileName = "LevelsRegistry";
        public virtual List<int> GatherStoredChunks()
        {
            var list = new List<int>();

            var levelsRegistry = Resources.Load<TextAsset>($"{LevelDirectoryName}/{LevelsRegistryFileName}");

            if (levelsRegistry == null)
            {
                return new List<int>();
            }
           
           
            /* var chunkDirectory = GetChunkSaveFolder();
             if (!System.IO.Directory.Exists(chunkDirectory))
             {
                 System.IO.Directory.CreateDirectory(chunkDirectory);
                 return list;
             }*/

            //var files = System.IO.Directory.GetFiles(chunkDirectory);
            foreach (var levelIndex in levelsRegistry.text.Split(','))
            {
                var parsedIndex = int.TryParse(levelIndex, out int index);
                
                if(!parsedIndex)
                    continue;

                list.Add(index);
                
                /* var filename = System.IO.Path.GetFileNameWithoutExtension(file);
                 var positions = filename.Split('_');
 
                 if (positions.Length < 2)
                 {
                     continue;
                 }
 
                 var parsed_x = int.TryParse(positions[0], out int x_pos);
                 var parsed_y = int.TryParse(positions[1], out int y_pos);
 
                 if (!parsed_x || !parsed_y)
                 {
                     continue;
                 }
 
                 list.Add(new int2(x_pos, y_pos));*/
            }

            return list;
        }


        private IEnumerator ActivateBtn()
        {
            if (_levelStorageObject.Data.Level == 0)
            {
                yield return new WaitForSeconds(timea);
            }
            else
            {
                yield return new WaitForSeconds(timeb);
            }

            levelEndButton.gameObject.SetActive(true);
        }
    }
}