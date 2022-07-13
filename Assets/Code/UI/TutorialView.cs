using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using Code.StoragesObjects;
using DG.Tweening;
using Plugins.RobyyUtils;
using Plugins.SimpleFactory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.UI
{
    public class TutorialView : Singleton<TutorialView>, IInitialized<LevelCompleteInitData>,IPointerClickHandler
    {
        [SerializeField] private Image back;
        [SerializeField] private Image hand;
        [SerializeField] private float blackoutForce;
        [SerializeField] private float blackoutTime;
        [SerializeField] private Transform selectColorPoint;
        [SerializeField] private Transform selectNextColorPoint;
        [SerializeField] private List<Transform> tapPoint;
        [SerializeField] private float scaleForce = 0.2f;
        [SerializeField] private float animTime = 0.5f;
        [SerializeField] private TMP_Text tipText;
        [SerializeField] private GameObject txtPanel; 

        private Tween _tween;
        private bool isFirstShown;
        private ColorsSelector _selector;
        
        public void Initialize(LevelCompleteInitData initData)
        {
            _selector = initData.ColorsSelector;
            back.DOFade(0f, 0f);
            gameObject.SetActive(false);
            hand.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);
        }

        public void Show()
        {
           
            gameObject.SetActive(true);
            tipText.text = "Choose color as on picture";
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

        [SerializeField] private Animator _animator;
        public void ShowV2()
        {
            isFirstShown = true;
            tipText.text = "Hold to pour sand";
            _tween.Kill();
            _animator.enabled = true;
            txtPanel.SetActive(false);
            isShowV2 = true;
        }

        private bool isShowV3;
        private bool isShowV2;
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        public void ShowV3()
        {
            hand.gameObject.SetActive(true);
            gameObject.SetActive(true);
            tipText.text = "Drag to see colors";
            tipText.gameObject.SetActive(true);
            txtPanel.SetActive(true);
            _animator.enabled = false;
            _tween.Kill();

            hand.transform.position = pointA.position;

            _tween = hand.transform.DOMove(pointB.position, animTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

            isShowV3 = true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isFirstShown)
            {
                //_selector.Buttons.First().SelectColor();
                ShowV2();
            }/*
            else if(isFirstShown && isShowV2 && !isShowV3)
            {
                ShowV3();
            }*/
            else if (isFirstShown && isShowV3)
            {
                //_selector.Buttons.Skip(1).First().SelectColor();
                Hide();
            }
            else
            {
                Hide();
            }
        }
    }
}