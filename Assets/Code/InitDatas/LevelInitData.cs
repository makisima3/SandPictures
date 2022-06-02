using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.UI;

namespace Code.InitDatas
{
    public class LevelInitData
    {
        public SimpleFactory WorldFactory { get; set; }
        
        public Material BaseMaterial { get; set; }
        
        public Image TargetImage { get; set; }
    }
}