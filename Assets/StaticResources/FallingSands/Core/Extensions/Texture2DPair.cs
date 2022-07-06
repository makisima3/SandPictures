namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class Texture2DPair
    {
        public Texture2D TextureA;
        public Texture2D TextureB;
        private bool swapFlag;

        public Texture2DPair(int width, int height, TextureFormat format, TextureWrapMode wrapMode, FilterMode filterMode)
        {
            TextureA = new Texture2D(width, height, format, false);
            TextureB = new Texture2D(width, height, format, false);

            TextureA.wrapMode = wrapMode;
            TextureB.wrapMode = wrapMode;

            TextureA.filterMode = filterMode;
            TextureB.filterMode = filterMode;

            TextureA.Apply();
            TextureB.Apply();
        }

        public void Release()
        {
            Texture2D.Destroy(TextureA);
            Texture2D.Destroy(TextureB);

            TextureA = null;
            TextureB = null;
        }

        public void Swap()
        {
            swapFlag = !swapFlag;
        }

        public Texture2D GetRead()
        {
            return swapFlag ? TextureB : TextureA;
        }

        public Texture2D GetWrite()
        {
            return swapFlag ? TextureA : TextureB;
        }

        public void Apply()
        {
            TextureA.Apply();
            TextureB.Apply();
        }
    }
}