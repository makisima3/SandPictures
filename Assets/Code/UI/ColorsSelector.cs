using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.UI
{
    public class ColorsSelector : MonoBehaviour,IInitialized<ColorsSelectorInitData>
    {
        [SerializeField] private Transform container;

        private List<SelectColorButton> _buttons;

        public List<SelectColorButton> Buttons => _buttons;

        public void Initialize(ColorsSelectorInitData initData)
        {
            _buttons = new List<SelectColorButton>();
            
            foreach (var uniqueMaterial in initData.Level.MaterialHolder.UniqueMaterials)
            {
                var button = initData.UIFactory.Create<SelectColorButton, SelectColorButtonInitData>(
                    new SelectColorButtonInitData()
                    {
                        UniqueMaterial = uniqueMaterial,
                        Level = initData.Level,
                        OnSelect = UnselectButtons
                    });
                button.transform.SetParent(container,true);
                
                _buttons.Add(button);
            }
            
            //_buttons.First().SelectColor();
        }

        public void UnselectButtons(SelectColorButton button)
        {
            foreach (var colorButton in _buttons.Where(b => b != button))
            {
                colorButton.UnSelect();
            }
        }
    }
}