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
            transform.localPosition = new Vector3(initData.Size.x / 4f, initData.Size.y / 2f, - 10f);
            camera.orthographicSize = initData.Size.x / 4f;
        }
    }
}