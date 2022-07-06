namespace CorgiFallingSands
{
    using Unity.Mathematics;

    [System.Serializable]
    public struct StampData
    {
        public FallingData id;
        public float temperature;
        public int2 position;
        public int radius;
        public bool overwriteAnything;
    }
}