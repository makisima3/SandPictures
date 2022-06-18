using System.Collections;
using Code.InitDatas;
using Code.Levels;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.UI
{
    public class SelectColorButton : MonoBehaviour, IInitialized<SelectColorButtonInitData>
    {
        [SerializeField] private Button button;
        [SerializeField] private Image colorImage;
        [SerializeField] private Image selectedImage;
        [SerializeField] private float attachmentSelectedSize = 1f;
        [SerializeField] private bool useSelectedImage;
        
        private Level _level;
        private MaterialHolder.UniqueMaterial _uniqueMaterial;
        private UnityAction<SelectColorButton> OnSelect;
        private RectTransform _rectTransform;
        private Vector2 startSize;
        
        public void Initialize(SelectColorButtonInitData initData)
        {
            _uniqueMaterial = initData.UniqueMaterial;
            _level = initData.Level;
            colorImage.color = _uniqueMaterial.Color;
            OnSelect = initData.OnSelect;
            _rectTransform = GetComponent<RectTransform>();
            button.onClick.AddListener(SelectColor);
            selectedImage.gameObject.SetActive(false);
            
            StartCoroutine(UnityUIFix());
        }

        public void SelectColor()
        {
            if(useSelectedImage)
                selectedImage.gameObject.SetActive(true);
            
            OnSelect.Invoke(this);
            _level.SelectMaterial(_uniqueMaterial);
            
            _rectTransform.sizeDelta = startSize + Vector2.up * attachmentSelectedSize;
        }

        public void UnSelect()
        {
            if(useSelectedImage)
                selectedImage.gameObject.SetActive(false);
            
            _rectTransform.sizeDelta = startSize;
        }

        private IEnumerator UnityUIFix()
        {
            yield return new WaitForEndOfFrame();
            startSize = _rectTransform.sizeDelta;
        }
    }
}