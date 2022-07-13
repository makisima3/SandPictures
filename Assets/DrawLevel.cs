using System.Collections;
using System.Collections.Generic;
using CorgiFallingSands;
using Unity.Mathematics;
using UnityEngine;

public class DrawLevel : MonoBehaviour
{
    public Texture2D drawTexture;

    public int x, y;
    public int size;
    public bool drawOnStart;
    void Start()
    {
        if (drawOnStart)
        {
            StartCoroutine(WaitLoad());
        }
    }

    IEnumerator WaitLoad()
    {
        yield return new WaitForSeconds(0.1f);
        ReadImage();
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReadImage();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            FallingSandsSystem.Instance.RequestStampAtCanvasPosition(new FallingData(1), 0, new int2(x,y), size);
        }
    }

    private void ReadImage()
    {
        if (drawTexture != null)
        {
            Debug.Log("Texture w-"+drawTexture.width+"|Texture h-"+drawTexture.height);
            for (int i = 0; i < drawTexture.width; i++)
            {
                for (int j = 0; j < drawTexture.height; j++)
                {
                    Color pixel = drawTexture.GetPixel(i, j);
                    
                    if (pixel != Color.white)
                    {
                        Debug.Log("1x=" + i + " //  y=" + j);
                        FallingSandsSystem.Instance.RequestStampAtCanvasPosition(new FallingData(1), 0, new int2(i,j), 1);
                    
                        Debug.Log("x="+i+" //  y="+j);
                    }
                }
            }
        }
    }
}
