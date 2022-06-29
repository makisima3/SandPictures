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
        [SerializeField] private float animTime = 1f;
        [SerializeField] private Image progressImage;
        [SerializeField] private Transform animatedTransform;
        [SerializeField] private float animatedTransformForce;
        [SerializeField] private int animatedTransformLoopsCount;
        [SerializeField] private float animatedTransformTime;

        private float value;
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
            percentText.text = $"{(percent * 100):F1}%";

            back.DOFade(blackoutForce, blackoutTime)
                .OnComplete(() =>
                {
                    animatedTransform.DOScale(animatedTransform.localScale + Vector3.one * animatedTransformForce,
                        animatedTransformTime).SetLoops(animatedTransformLoopsCount, LoopType.Yoyo);
                    DOTween.To(Getter, Setter, percent, animTime);
                    percentText.gameObject.SetActive(true);
                    nextLevelButton.gameObject.SetActive(true);
                    retryButton.gameObject.SetActive(true);
                });

            var lvl = PersistentStorage.PersistentStorage.Load<LevelStorageObject, LevelStorageObject.LevelData>(
                new LevelStorageObject(new LevelStorageObject.LevelData() {Level = 0}));
            lvl.Data.Level += 1;
            PersistentStorage.PersistentStorage.Save<LevelStorageObject, LevelStorageObject.LevelData>(lvl);
        }

        [ContextMenu("Show")]
        public void Show()
        {
            Show(1);
        }

        private void Setter(float value)
        {
            this.value = value;
            percentText.text = $"{Mathf.RoundToInt(value * 100)}%";
            progressImage.fillAmount = value;
        }

        private float Getter()
        {
            return value;
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