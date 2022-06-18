using System.Collections.Generic;
using Code.Levels;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.Events;

namespace Code.InitDatas
{
    public class VesselInitData
    {
        public Vector2Int Size { get; set; }
        public float DropGrainTime { get; set; }
        public SimpleFactory WorldFactory { get; set; }
        public  ResultRenderer ResultRenderer { get; set; }
        public float newRowDelay { get; set; }
        public UnityEvent<bool> OnSpawnStateChange { get; set; }
        public Cell[,] Cells { get; set; }
        public List<List<Cell>> SplitedZones { get; set; }
    }
}