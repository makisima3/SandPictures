using Code.InitDatas;
using Code.Levels;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.UI
{
    public class SelectColorButton : MonoBehaviour, IInitialized<SelectColorButtonInitData>
    {
        [SerializeField] private Button button;
        [SerializeField] private Image colorImage;
        [SerializeField] private Image selectedImage;
        
        private Level _level;
        private MaterialHolder.UniqueMaterial _uniqueMaterial;

        private UnityAction<SelectColorButton> OnSelect;

        public void Initialize(SelectColorButtonInitData initData)
        {
            _uniqueMaterial = initData.UniqueMaterial;
            _level = initData.Level;
            colorImage.color = _uniqueMaterial.Color;
            OnSelect = initData.OnSelect;

            button.onClick.AddListener(SelectColor);
        }

        public void SelectColor()
        {
            selectedImage.gameObject.SetActive(true);
            OnSelect.Invoke(this);
            _level.SelectMaterial(_uniqueMaterial);
        }

        public void UnSelect()
        {
            selectedImage.gameObject.SetActive(false);
        }
    }
}