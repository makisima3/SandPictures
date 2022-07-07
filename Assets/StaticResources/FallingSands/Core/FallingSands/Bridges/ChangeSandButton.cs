using System;
using System.Collections;
using Code.InitDatas;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code
{
    public class ChangeSandButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image backImage;
        [SerializeField] private Image colorImage;
        [SerializeField] private float sizeScaler = 1f;
        private RectTransform _rectTransform;
        private Vector2 startSize;
        private Action onClickAction;
        private UnityAction<ChangeSandButton> OnSelect;

        public Button Button => _button;

        public Image BackImage => backImage;

        public Image ColorImage => colorImage;

        public void Initialize(ChangeSandButtonInitData initData)
        {
            OnSelect = initData.OnSelect;
            onClickAction = initData.OnClickAction;
            _rectTransform = GetComponent<RectTransform>();
            startSize = _rectTransform.sizeDelta;

            _button.onClick.AddListener(() =>
            {
                Select();
                OnSelect.Invoke(this);
            });
             
            StartCoroutine(UnityUIFix());
            
        }

        public void Select()
        {
            onClickAction.Invoke();
           
            _rectTransform.sizeDelta = startSize + Vector2.up * sizeScaler;
        }
        
        public void Unselect()
        {
            _rectTransform.sizeDelta = startSize;
        }
        
        private IEnumerator UnityUIFix()
        {
            yield return new WaitForEndOfFrame();
            startSize = _rectTransform.sizeDelta;
        }
    }
}