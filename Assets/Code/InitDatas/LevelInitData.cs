using Code.UI;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.UI;

namespace Code.InitDatas
{
    public class LevelInitData
    {
        public SimpleFactory WorldFactory { get; set; }
        public ColorsSelector ColorsSelector { get; set; }
        public int Level { get; set; }
        public Material BaseMaterial { get; set; }
        public TutorialView TutorialView { get; set; }
        public Image TargetImage { get; set; }
        public LevelCompleteView LevelCompleteView { get; set; }
    }
}