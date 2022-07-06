namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Collections;
    using UnityEngine;

    [System.Serializable]
    public class NativeArrayPair<T> where T : struct
    {
        public NativeArray<T> DataA;
        public NativeArray<T> DataB;
        private bool swapFlag;

        public NativeArrayPair(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            DataA = new NativeArray<T>(length, allocator, options);
            DataB = new NativeArray<T>(length, allocator, options);
        }

        public void Release()
        {
            DataA.Dispose();
            DataB.Dispose();
        }

        public void Swap()
        {
            swapFlag = !swapFlag;
        }

        public NativeArray<T> GetRead()
        {
            return swapFlag ? DataB : DataA;
        }

        public NativeArray<T> GetWrite()
        {
            return swapFlag ? DataA : DataB;
        }
    }
}
