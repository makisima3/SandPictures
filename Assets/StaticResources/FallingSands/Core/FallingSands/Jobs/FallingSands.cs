namespace CorgiFallingSands
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;


    [BurstCompile]
    public struct FallingSands : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> TemperatureIn;

        [NativeDisableParallelForRestriction] public NativeArray<bool> TrackedChanges;
        [NativeDisableParallelForRestriction] public NativeArray<FallingData> Data;
        public int textureWidth;
        public int textureHeight;

        [ReadOnly] public NativeArray<FallingDataMetadata> Metadata;
        public NativeQueue<FallingSandsEventData>.ParallelWriter EventsOutput;

        public int direction_index;
        public int pass_index;
        public bool generate_events;
        public bool boundaries_are_voids;

        public void Execute(int index)
        {
            int2 id = MathE.Get1DTo2D(index, textureWidth, textureHeight);

            if (pass_index == 0 && id.x % 2 != 0 && id.y % 2 != 0) return;
            if (pass_index == 1 && id.x % 2 != 1 && id.y % 2 != 0) return;
            if (pass_index == 2 && id.x % 2 != 0 && id.y % 2 != 1) return;
            if (pass_index == 3 && id.x % 2 != 1 && id.y % 2 != 1) return;

            FallingData fromData = Data[index];
            var fromDataType = fromData.GetDataType();
            var fromMetadata = Metadata[(int)fromDataType];

            // don't move things twice.. 
            if (TrackedChanges[index]) return;

            // try to change the value of the current pixel if the temperature is high or low 
            if (TryTemperatureConvert(index, fromData))
            {
                return;
            }

            // try to spread grass 
            if(TrySpreadToWater(id, fromData))
            {
                return;
            }

            // try to create something from nothing (spouts) 
            if(TryCreateFromThinAir(id, fromData))
            {
                return;
            }

            if (fromMetadata.FluidType == FallingDataFluidType.Sand || fromMetadata.FluidType == FallingDataFluidType.Fluid)
            {
                if (TryTransfer(id, id + new int2(+0, -1), fromData)) return;
                if (TryTransfer(id, id + new int2(-1, -1), fromData)) return;
                if (TryTransfer(id, id + new int2(+1, -1), fromData)) return;

                // only liquids try to transfer sideways.. 
                if (fromMetadata.FluidType == FallingDataFluidType.Fluid)
                {
                    if (TryTransfer(id, id + new int2(+1, +0), fromData)) return;
                    if (TryTransfer(id, id + new int2(-1, +0), fromData)) return;
                }
            }

            else if (fromMetadata.FluidType == FallingDataFluidType.Gas)
            {
                if (TryTransfer(id, id + new int2(+0, +1), fromData)) return;
                if (TryTransfer(id, id + new int2(-1, +1), fromData)) return;
                if (TryTransfer(id, id + new int2(+1, +1), fromData)) return;
                if (TryTransfer(id, id + new int2(+1, +0), fromData)) return;
                if (TryTransfer(id, id + new int2(-1, +0), fromData)) return;
            }

            // else if (fromMetadata.FluidType == FallingDataFluidType.HeavyGas)
            // {
            //     if (TryTransfer(id, id + new int2(+0, +1), fromData)) return;
            //     if (TryTransfer(id, id + new int2(+0, -1), fromData)) return;
            // 
            //     if (direction_index == 0 && TryTransfer(id, id + new int2(+1, +0), fromData)) return;
            //     if (direction_index == 1 && TryTransfer(id, id + new int2(-1, +0), fromData)) return;
            // 
            //     // if (direction_index == 4 && TryTransfer(id, id + new int2(-1, -1), fromData)) return;
            //     // if (direction_index == 5 && TryTransfer(id, id + new int2(+1, +1), fromData)) return;
            //     // if (direction_index == 6 && TryTransfer(id, id + new int2(-1, +1), fromData)) return;
            //     // if (direction_index == 7 && TryTransfer(id, id + new int2(+1, -1), fromData)) return;
            // }
        }

        private bool TrySpreadToWater(int2 id, FallingData fromData)
        {
            var meta = Metadata[(int) fromData.GetDataType()];
            if (!meta.spreadsInWater)
            {
                return false;
            }

            if (TrySpread(id, id + new int2(+0, -1), fromData)) return true;
            if (TrySpread(id, id + new int2(+0, +1), fromData)) return true;
            if (TrySpread(id, id + new int2(-1, +0), fromData)) return true;
            if (TrySpread(id, id + new int2(+1, +0), fromData)) return true;

            return false;
        }

        private bool TrySpread(int2 from, int2 to, FallingData fromData)
        {
            var fromIndex = MathE.Get2DTo1D(from, textureWidth, textureHeight);
            var toIndex = MathE.Get2DTo1D(to, textureWidth, textureHeight);

            // boundary: go off-screen? 
            if (to.x < 0 || to.y < 0 || to.x >= textureWidth || to.y >= textureHeight)
            {
                return false;
            }

            FallingData toData = Data[toIndex];
            var toDataType = toData.GetDataType();

            if (toDataType == FallingDataType.Water)
            {
                Data[toIndex] = new FallingData(fromData.GetDataType());
                TrackedChanges[toIndex] = true;

                return true;
            }

            return false;
        }

        private bool TryCreateFromThinAir(int2 id, FallingData fromData)
        {
            var meta = Metadata[(int)fromData.GetDataType()];
            if (meta.createsFromThinAir == FallingDataType.Air)
            {
                return false;
            }

            var createData = new FallingData(meta.createsFromThinAir);

            CreateFromThinAir(id + new int2(+0, -1), createData);
            CreateFromThinAir(id + new int2(+0, +1), createData);
            CreateFromThinAir(id + new int2(+1, +0), createData);
            CreateFromThinAir(id + new int2(-1, +0), createData);

            return true;
        }

        private bool CreateFromThinAir(int2 to, FallingData createData)
        {
            var toIndex = MathE.Get2DTo1D(to, textureWidth, textureHeight);

            // boundary: go off-screen? 
            if (to.x < 0 || to.y < 0 || to.x >= textureWidth || to.y >= textureHeight)
            {
                return false;
            }

            FallingData toData = Data[toIndex];
            var toDataType = toData.GetDataType();

            if (toDataType == FallingDataType.Air)
            {
                Data[toIndex] = createData;
                TrackedChanges[toIndex] = true;

                return true;
            }

            return false;
        }

        private bool TryTransfer(int2 from, int2 to, FallingData fromData)
        {
            var fromIndex = MathE.Get2DTo1D(from, textureWidth, textureHeight);
            var toIndex = MathE.Get2DTo1D(to, textureWidth, textureHeight);

            // boundary: go off-screen? 
            if (to.x < 0 || to.y < 0 || to.x >= textureWidth || to.y >= textureHeight)
            {
                if(boundaries_are_voids)
                {
                    Data[fromIndex] = new FallingData();
                    TrackedChanges[toIndex] = true;
                    return true;
                }
                else
                {
                    return false; 
                }
            }

            FallingData toData = Data[toIndex];
            var toDataType = toData.GetDataType();

            var fromMetadata = Metadata[(int) fromData.GetDataType()];
            var toMetadata = Metadata[(int) toDataType];
            var fromIsPowder = fromMetadata.FluidType == FallingDataFluidType.Sand || fromMetadata.FluidType == FallingDataFluidType.Gas;
            var toIsFluid = toMetadata.FluidType == FallingDataFluidType.Fluid;

            if ((fromIsPowder && toIsFluid) || toDataType == FallingDataType.Air)
            {
                Data[toIndex] = new FallingData(fromData.GetDataType());
                Data[fromIndex] = new FallingData(toData.GetDataType());

                TrackedChanges[toIndex] = true;

                return true;
            }



            return false;
        }

        private bool TryTemperatureConvert(int index, FallingData fromData)
        {

            var dataType = fromData.GetDataType();
            var metadata = Metadata[(int)dataType];

            // early out, this data doesnt have a min or max 
            if (metadata.MaxTemperature == 0f && metadata.MinTemperature == 0f)
            {
                return false;
            }

            var temperature = TemperatureIn[index];
            if (metadata.MinTemperature != 0f && temperature < metadata.MinTemperature)
            {
                TrackedChanges[index] = true;

                var newData = fromData;
                newData = new FallingData(metadata.MinTemperatureBecomes);

                Data[index] = newData;

                if(generate_events)
                {
                    var newEvent = new FallingSandsEventData()
                    {
                        index = index,
                        id_a = fromData.GetDataType(),
                        id_b = newData.GetDataType(),
                        temperature = temperature,
                    };

                    EventsOutput.Enqueue(newEvent);
                }

                return true;
            }

            if (metadata.MaxTemperature != 0f && temperature > metadata.MaxTemperature)
            {
                TrackedChanges[index] = true;

                var newData = fromData;
                newData = new FallingData(metadata.MaxTemperatureBecomes);

                Data[index] = newData;

                if(generate_events)
                {
                    var newEvent = new FallingSandsEventData()
                    {
                        index = index,
                        id_a = fromData.GetDataType(),
                        id_b = newData.GetDataType(),
                        temperature = temperature,
                    };

                    EventsOutput.Enqueue(newEvent);
                }

                return true;
            }

            return false;
        }
    }
}