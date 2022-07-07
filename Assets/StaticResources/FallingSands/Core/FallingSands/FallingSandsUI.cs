using Code;
using Code.InitDatas;

namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class FallingSandsUI : MonoBehaviour
    {
        public FallingSandsSystem fallingSandsSystem;
        public FallingSandsMouseWriter mouseWriter;
        public SimpleAudioPlayer ourAudioPlayer;

        public ChangeSandButton PrefabButton;
        public GameObject PrefabSeparator;
        public Transform ButtonParentTransform;
        public TMP_Text StampSizeLabel;
        public TMP_Text StampLabel;
        public TMP_Text StampDescLabel;
        public Button StampButton;

        private List<ChangeSandButton> _changeSandButtons;

        [System.Serializable]
        public class UiGroup
        {
            public FallingDataFluidType fluidType;
            public Transform Content;
        }

        public List<UiGroup> UiGroups = new List<UiGroup>();

        private void Start()
        {
            InitUI();

            
        }

        private Transform GetUiGroupParent(FallingDataFluidType fluidType)
        {
            var count = UiGroups.Count;
            for (var i = 0; i < count; ++i)
            {
                var group = UiGroups[i];
                if (group.fluidType == fluidType)
                {
                    return group.Content;
                }
            }

            return null;
        }

        private void InitUI()
        {
            var count = FallingSandsDataManager.Instance.DataObjects.Count;

            var sorted = new List<FallingSandsDataObj>(count);
            for (var i = 0; i < count; ++i)
            {
                sorted.Add(FallingSandsDataManager.Instance.DataObjects[i]);
            }

            // var orderedList = sorted.OrderBy(a => GetSortIntForFluidTypes(a.Metadata.FluidType)).ThenBy(a => a.listBias);
            // var sortedArray = orderedList.ToArray();

            sorted.Sort((a, b) => a.listBias.CompareTo(b.listBias));
            // sorted.Sort((a, b) => GetSortIntForFluidTypes(a.Metadata.FluidType).CompareTo(GetSortIntForFluidTypes(b.Metadata.FluidType)));

            InitUiForType(FallingDataFluidType.Sand, sorted);
            InitUiForType(FallingDataFluidType.Fluid, sorted);
            InitUiForType(FallingDataFluidType.Solid, sorted);
            InitUiForType(FallingDataFluidType.Gas, sorted);
            InitUiForType(FallingDataFluidType.Air, sorted);
        }

        private void InitUiForType(FallingDataFluidType fluidType, List<FallingSandsDataObj> sorted)
        {
            var parent = GetUiGroupParent(fluidType);

            var count = FallingSandsDataManager.Instance.DataObjects.Count;
            // var previousFluidType = (FallingDataFluidType)(-1);
            for (var i = 0; i < count; ++i)
            {
                var data = sorted[i];
                if (!data.ShowInPicker || data.Metadata.FluidType != fluidType)
                {
                    continue;
                }

                // if (data.Metadata.FluidType != previousFluidType)
                // {
                //     previousFluidType = data.Metadata.FluidType;
                // 
                //     var separatorGo = GameObject.Instantiate(PrefabSeparator, parent);
                //     var separatorLabel = separatorGo.GetComponentInChildren<TMP_Text>();
                //     separatorLabel.text = GetNameKeyFromFluidType(previousFluidType);
                // }

                var newButtonGo = GameObject.Instantiate(PrefabButton.gameObject, parent);
                var newButton = newButtonGo.GetComponent<ChangeSandButton>();

                var newIcon = newButtonGo.transform.Find("Icon").GetComponent<Image>();
                newIcon.sprite = data.Icon;

                var newImage = newButton.ColorImage;
                newImage.color = Color.Lerp(data.Metadata.Color, Color.white, 0.5f); // desaturate 

                var newText = newButton.GetComponentInChildren<TMPro.TMP_Text>(true);
                newText.text = data.nameKey;

                //newButton.Button.onClick.RemoveAllListeners();
                newButton.Initialize(new ChangeSandButtonInitData()
                {
                    OnClickAction = () =>
                    {
                        mouseWriter.ChangeStampData(data);

                        ourAudioPlayer.PlaySoundBurst(ourAudioPlayer.AudioSelectMaterial);
                    },
                    OnSelect = Unselect,
                });
              
                _changeSandButtons ??= new List<ChangeSandButton>();
                
                _changeSandButtons.Add(newButton);
                
                /*newButton.onClick.AddListener(() =>
                {
                    mouseWriter.ChangeStampData(data);

                    var desc = data.descKey;
                    StampLabel.text = $"{data.nameKey}<size=75%> * {data.descKey}</size>";

                    var newIcon = StampButton.transform.Find("Icon").GetComponent<Image>();
                    newIcon.sprite = data.Icon;

                    var newText = StampButton.GetComponentInChildren<TMPro.TMP_Text>(true);
                    newText.text = data.nameKey;

                    ourAudioPlayer.PlaySoundBurst(ourAudioPlayer.AudioSelectMaterial);
                });*/
            }
        }

        private void Unselect(ChangeSandButton btn)
        {
            foreach (var button in _changeSandButtons)
            {
                button.Unselect();
            }
            
            btn.Select();
        }
        
        public void OnKnob_DeltaRotation(float deltaAngle)
        {
            StampSizeLabel.text = $"{mouseWriter.StampSize:N2}x";
            mouseWriter.ChangeStampSize(mouseWriter.StampSize + deltaAngle); 
        }

        public void OnButton_Clear()
        {
            ourAudioPlayer.PlaySoundBurst(ourAudioPlayer.AudioClearScreen);
            fallingSandsSystem.Clear(); 
        }
    }
}