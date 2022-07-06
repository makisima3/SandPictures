namespace CorgiFallingSands
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Jobs;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public struct LoadChunkJob : IJob
    {
        [WriteOnly] public NativeArray<FallingData> SaveData;
        [WriteOnly] public NativeArray<float> SaveTemp;
        
        [ReadOnly,DeallocateOnJobCompletion] public NativeArray<byte> StorageArray;
        
       

        public unsafe void Execute()
        {
            try
            {
                //var filepath = (string)filepathHandle.Target;

                //var level =  Resources.Load<TextAsset>(filepath);

                /*if (level == null)
                {
                    return;
                }*/

                //var tempStorageArray = new NativeArray<byte>(level.bytes,Allocator.Temp);
                var saveDataBytes = SaveData.Reinterpret<byte>(sizeof(float));
                var saveTempBytes = SaveTemp.Reinterpret<byte>(sizeof(float));

                int compressedCursor = 0;
                int decompressedCursor = 0;

                ArrayE.DecompressReadBuffer_float(StorageArray, saveDataBytes, ref compressedCursor,
                    ref decompressedCursor);

                decompressedCursor = 0; // reset 

                ArrayE.DecompressReadBuffer_float(StorageArray, saveTempBytes, ref compressedCursor,
                    ref decompressedCursor);

                StorageArray.Dispose();

                /* if (!System.IO.File.Exists(filepath))
                 {
                     return;
                 }
 
                 
                 using (var stream = System.IO.File.OpenRead(filepath))
                 {
                     // todo: get pixel count from save data..? 
 
                     // read compressed data 
                     var tempStorageArray = new NativeArray<byte>((int) stream.Length, Allocator.Temp);
                     var tempStorageSpan = new System.Span<byte>(tempStorageArray.GetUnsafePtr(), tempStorageArray.Length);
 
                     stream.Read(tempStorageSpan);
 
                     // decompress 
                     var saveDataBytes = SaveData.Reinterpret<byte>(sizeof(float));
                     var saveTempBytes = SaveTemp.Reinterpret<byte>(sizeof(float));
 
                     int compressedCursor = 0;
                     int decompressedCursor = 0;
 
                     ArrayE.DecompressReadBuffer_float(tempStorageArray, saveDataBytes, ref compressedCursor, ref decompressedCursor);
                     
                     decompressedCursor = 0; // reset 
 
                     ArrayE.DecompressReadBuffer_float(tempStorageArray, saveTempBytes, ref compressedCursor, ref decompressedCursor);
 
                     tempStorageArray.Dispose(); 
                 }*/
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}