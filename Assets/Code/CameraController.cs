using Code.InitDatas;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code
{
    public class CameraController : MonoBehaviour, IInitialized<CameraControllerInitData>
    {

        [SerializeField] private Camera camera;
        public void Initialize(CameraControllerInitData initData)
        {
            /*
              camera.orthographicSize = (initData.Size.x / 2f) * initData.GrainSize;
            
            transform.localPosition = new Vector3(initData.Size.x / 2f  * initData.GrainSize,
                camera.orthographicSize,
                - 10f);
             */
            
            transform.localPosition = new Vector3(initData.Size.x / 4f, initData.Size.y / 2f, - 10f);
            camera.orthographicSize = initData.Size.x / 4f;
        }
    }
}