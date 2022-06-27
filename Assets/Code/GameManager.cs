using System.Collections.Generic;
using Code.Factories;
using Code.InitDatas;
using Code.Levels;
using Code.StoragesObjects;
using Code.UI;
using UnityEngine;
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
        
        private LevelStorageObject _levelStorageObject;

        private void Awake()
        {
            Application.targetFrameRate = 150;
            
            
            _levelStorageObject =
                PersistentStorage.PersistentStorage.Load<LevelStorageObject, LevelStorageObject.LevelData>(
                    new LevelStorageObject(new LevelStorageObject.LevelData() {Level = 0}));
            
            if (_levelStorageObject.Data.Level >= levels.Count)
            {
                _levelStorageObject.Data.Level = 0;
                PersistentStorage.PersistentStorage.Save<LevelStorageObject, LevelStorageObject.LevelData>(
                    _levelStorageObject);
            }

            var level = LoadLevel();
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
                ColorsSelector = colorsSelector
            });
            return spawnedLevel;
        }
    }
}