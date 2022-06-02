using Code.InitDatas;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code
{
    public class CameraController : MonoBehaviour, IInitialized<CameraControllerInitData>
    {
        
        public void Initialize(CameraControllerInitData initData)
        {
            transform.position = new Vector3(initData.Size.x / 2f, initData.Size.y / 2f, -initData.Size.x/2 * 3 - 10f);
        }
    }
}