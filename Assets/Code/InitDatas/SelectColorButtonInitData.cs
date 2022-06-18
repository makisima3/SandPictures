using Code.Levels;
using Code.UI;
using UnityEngine.Events;

namespace Code.InitDatas
{
    public class SelectColorButtonInitData
    {
        public MaterialHolder.UniqueMaterial UniqueMaterial { get; set; }
        public Level Level { get; set; }
        public UnityAction<SelectColorButton> OnSelect { get; set; } 
    }
}