namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FallingSandsDataManager : MonoBehaviour
    {
        public static FallingSandsDataManager Instance;

        public List<FallingSandsDataObj> DataObjects = new List<FallingSandsDataObj>();
        public bool Persistent;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                this.enabled = false;
                return;
            }

            Instance = this;

            if (Persistent)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }

        public FallingSandsDataObj FindDataObjectFromId(FallingDataType id)
        {
            var count = DataObjects.Count;
            for (var i = 0; i < count; ++i)
            {
                var data = DataObjects[i];
                if (data.Id == id)
                {
                    return data;
                }
            }

            return null;
        }

        private string GetNameKeyFromFluidType(FallingDataFluidType fluidType)
        {
            switch (fluidType)
            {
                default: return "unknown";
                case FallingDataFluidType.Air: return "air";
                case FallingDataFluidType.Fluid: return "fluids";
                case FallingDataFluidType.Gas: return "gasses";
                case FallingDataFluidType.Sand: return "powders";
                case FallingDataFluidType.Solid: return "solids";
            }
        }

        private int GetSortIntForFluidTypes(FallingDataFluidType fluidType)
        {
            switch (fluidType)
            {
                case FallingDataFluidType.Sand: return 0;
                case FallingDataFluidType.Fluid: return 1;
                case FallingDataFluidType.Solid: return 2;
                case FallingDataFluidType.Gas: return 3;
                case FallingDataFluidType.Air: return 4;
                default: return 128;
            }
        }
    }
}