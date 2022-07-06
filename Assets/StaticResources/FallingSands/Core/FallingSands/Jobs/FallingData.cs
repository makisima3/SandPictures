namespace CorgiFallingSands
{
    [System.Serializable]
    public struct FallingData
    {
        private float data;

        public FallingData(FallingDataType dataType)
        {
            data = (float) (int) dataType;
        }

        public FallingDataType GetDataType()
        {
            return (FallingDataType) (int) data; 
        }

        public static implicit operator float(FallingData d)
        {
            return d.data;
        }

        public static implicit operator FallingData(float f)
        {
            return new FallingData((FallingDataType) f);
        }
    }
}