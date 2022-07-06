namespace CorgiFallingSands
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Jobs;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public struct SaveChunkJob : IJob
    {
        [ReadOnly] public NativeArray<FallingData> SaveData;
        [ReadOnly] public NativeArray<float> SaveTemp;

        [NativeDisableUnsafePtrRestriction] public System.Runtime.InteropServices.GCHandle filepathHandle;

        public unsafe void Execute()
        {
            try
            {
                var filepath = (string)filepathHandle.Target;

                using (var stream = System.IO.File.OpenWrite(filepath))
                {
                    // todo: store pixel length?
                    var saveDataByteLength = sizeof(float) * 1 * SaveData.Length;
                    var saveTempByteLength = sizeof(float) * 1 * SaveTemp.Length;

                    // compress 
                    var tempStorageSize = saveDataByteLength + saveTempByteLength;
                    var tempStorageArray = new NativeArray<byte>(tempStorageSize * 2, Allocator.Temp);

                    var cursor = 0;
                    var saveDataBytes = SaveData.Reinterpret<byte>(sizeof(float));
                    var saveTempBytes = SaveTemp.Reinterpret<byte>(sizeof(float));
                    
                    ArrayE.CompressWrittenBuffer_Floats(saveDataBytes, tempStorageArray, 0, saveDataBytes.Length, ref cursor);
                    ArrayE.CompressWrittenBuffer_Floats(saveTempBytes, tempStorageArray, 0, saveTempBytes.Length, ref cursor);

                    // write to file 
                    var tempStorageArrayPointer = tempStorageArray.GetUnsafeReadOnlyPtr();
                    var tempStorageSpan = new System.Span<byte>(tempStorageArrayPointer, cursor);
                    stream.Write(tempStorageSpan);

                    tempStorageArray.Dispose();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                filepathHandle.Free(); 
            }
        }
    }
}