using Code.InitDatas;
using Code.StoragesObjects;
using DG.Tweening;
using Plugins.SimpleFactory;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.UI
{
    public class LevelCompleteView : MonoBehaviour, IInitialized<LevelCompleteInitData>
    {
        [SerializeField] private Image back;
        [SerializeField] private float blackoutForce;
        [SerializeField] private float blackoutTime;
        [SerializeField] private TMP_Text percentText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button retryButton;
        
        public void Initialize(LevelCompleteInitData initData)
        {
            back.DOFade(0f, 0f);
            
            gameObject.SetActive(false);
            percentText.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
            retryButton.gameObject.SetActive(false);
            
            nextLevelButton.onClick.AddListener(NextLevel);
            retryButton.onClick.AddListener(Retry);
        }

        public void Show(float percent)
        {
            gameObject.SetActive(true);
            percentText.text = $"your result\n{percent * 100}%";

            back.DOFade(blackoutForce, blackoutTime)
                .OnComplete(() =>
                {
                    percentText.gameObject.SetActive(true);
                    nextLevelButton.gameObject.SetActive(true);
                    retryButton.gameObject.SetActive(true);
                });
        }

        private void NextLevel()
        {
            var data = new LevelStorageObject.LevelData();
            data.Level = 0;
            var levelStorageObject =
                PersistentStorage.PersistentStorage.Load<LevelStorageObject, LevelStorageObject.LevelData>(
                    new LevelStorageObject(data));

            levelStorageObject.Data.Level += 1;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}