using Code.InitDatas;
using Code.StoragesObjects;
using DG.Tweening;
using Plugins.SimpleFactory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.UI
{
    public class TutorialView : MonoBehaviour, IInitialized<LevelCompleteInitData>,IPointerClickHandler
    {
        [SerializeField] private Image back;
        [SerializeField] private Image hand;
        [SerializeField] private float blackoutForce;
        [SerializeField] private float blackoutTime;
        [SerializeField] private Transform selectColorPoint;
        [SerializeField] private Transform selectNextColorPoint;
        [SerializeField] private Transform tapPoint;
        [SerializeField] private float scaleForce = 0.2f;
        [SerializeField] private float animTime = 0.5f;
        [SerializeField] private TMP_Text tipText;

        private Tween _tween;
        private bool isFirstShown;
        
        public void Initialize(LevelCompleteInitData initData)
        {
            back.DOFade(0f, 0f);
            
            gameObject.SetActive(false);
            hand.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            tipText.text = "choose color as on picture";
            back.DOFade(blackoutForce, blackoutTime)
                .OnComplete(() =>
                {
                    tipText.gameObject.SetActive(true);
                   hand.gameObject.SetActive(true);
                   hand.transform.position = selectColorPoint.position;
                   _tween = hand.transform.DOScale(hand.transform.localScale + Vector3.one * scaleForce, animTime)
                       .SetLoops(-1, LoopType.Yoyo);
                });
        }

        public void ShowV2()
        {
            isFirstShown = true;
            tipText.text = "Hold to pour sand";
            hand.transform.position = tapPoint.position;
        }
        
        public void ShowV3()
        {
            back.DOFade(0f, 0f);
            gameObject.SetActive(true);
            hand.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);
            back.DOFade(blackoutForce, blackoutTime)
                .OnComplete(() => 
                { 
                    tipText.gameObject.SetActive(true);
                    hand.gameObject.SetActive(true);
                    tipText.text = "choose next color as on picture";
                    hand.transform.position = selectNextColorPoint.position;
                });
            
            
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isFirstShown)
                ShowV2();
            else
                Hide();
        }
    }
}