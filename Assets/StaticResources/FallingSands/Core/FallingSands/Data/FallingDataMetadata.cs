namespace CorgiFallingSands
{
    using UnityEngine;

    [System.Serializable]
    public struct FallingDataMetadata
    {
        public FallingDataFluidType FluidType;
        [ColorUsage(true, true)] public Color Color;
        public float temperature;

        public bool hasSolidCollision; // for the meshing pass
        public bool spreadsInWater; // grass  
        public FallingDataType createsFromThinAir; // "spout" might mater "water"

        // water -> ice etc 
        public float MaxTemperature;
        public float MinTemperature;

        public FallingDataType MaxTemperatureBecomes;
        public FallingDataType MinTemperatureBecomes;
    }
}