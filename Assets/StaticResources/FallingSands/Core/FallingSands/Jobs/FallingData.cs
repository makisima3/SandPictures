namespace CorgiFallingSands
{
    [System.Serializable]
    public struct FallingData
    {
        private float data;

        public FallingData(int dataType)
        {
            data = (float) (int) dataType;
        }

        public int GetDataType()
        {
            return (int) data; 
        }

        public static implicit operator float(FallingData d)
        {
            return d.data;
        }

        public static implicit operator FallingData(float f)
        {
            return new FallingData((int) f);
        }
    }
}