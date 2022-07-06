namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ScriptableObjects/FallingSandsDataObj")]
    public class FallingSandsDataObj : ScriptableObject
    {
        // for UI 
        public string nameKey;
        public string descKey;
        public bool ShowInPicker;

        // for physics 
        public FallingDataType Id;
        public FallingDataMetadata Metadata;

        // for vfx/sfx
        public AudioClip OnStamp;
        public AudioClip OnTempHigh;
        public AudioClip OnTempLow;

        public Sprite Icon;

        public int listBias;
    }
}