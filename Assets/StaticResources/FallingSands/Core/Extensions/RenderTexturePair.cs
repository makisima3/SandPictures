namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class RenderTexturePair
    {
        public RenderTexture TextureA;
        public RenderTexture TextureB;
        private bool swapFlag;

        public RenderTexturePair(int size, RenderTextureFormat format, TextureWrapMode wrapMode, FilterMode filterMode)
        {
            TextureA = new RenderTexture(size, size, 24, format, RenderTextureReadWrite.Linear);
            TextureB = new RenderTexture(size, size, 24, format, RenderTextureReadWrite.Linear);

            TextureA.wrapMode = wrapMode;
            TextureB.wrapMode = wrapMode;

            TextureA.filterMode = filterMode;
            TextureB.filterMode = filterMode;

            TextureA.enableRandomWrite = true;
            TextureB.enableRandomWrite = true;

            TextureA.autoGenerateMips = false;
            TextureB.autoGenerateMips = false;

            TextureA.Create();
            TextureB.Create();
        }

        public void Release()
        {
            TextureA.Release();
            TextureB.Release();
        }

        public void Swap()
        {
            swapFlag = !swapFlag;
        }

        public RenderTexture GetRead()
        {
            return swapFlag ? TextureB : TextureA;
        }

        public RenderTexture GetWrite()
        {
            return swapFlag ? TextureA : TextureB;
        }
    }
}