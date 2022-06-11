using System;
using UnityEngine;

namespace Code.InitDatas
{
    public class GrainInitData
    {
        public Vector2Int GridPosition { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public Vector3 RenderPosition { get; set; }
        public Transform RenderParent { get; set; }
        public MaterialHolder.UniqueMaterial UniqueMaterial { get; set; }
        public float TimeToMove { get; set; }
    }
}