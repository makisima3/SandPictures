using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.InitDatas
{
    public class VesselInitData
    {
        public Vector2Int Size { get; set; }
        public float DropGrainTime { get; set; }
        public SimpleFactory WorldFactory { get; set; }
    }
}