namespace CorgiFallingSands
{
    [System.Serializable]
    public enum FallingDataFluidType
    {
        Air = 0, // does nothing 
        Solid = 1, // does nothing 
        Sand = 2, // falls down
        Fluid = 3, // falls down and spreads out 
        Gas = 4, // falls up and spreads out 
                 // HeavyGas        = 5, // falls up and spreads out 
    }
}