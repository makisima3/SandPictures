namespace CorgiFallingSands
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Jobs;
    using Unity.Collections;

    public class FallingSandsMouseWriter : MonoBehaviour
    {
        [Header("Stamp Settings")]
        public int StampSize = 1;
        public bool OverwriteAnything = false;

        [Header("Internal Settings")]
        public Camera ourCamera;
        public LayerMask ScreenMask;

        [System.NonSerialized] private FallingSandsDataObj _stampData = null;
        [System.NonSerialized] private float _stampTemperature;
        [System.NonSerialized] private float _prevStampSoundAt;

        [Header("MySettings")]
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float dist = 4000f;

        private Transform myTransform;
        private void Start()
        {
            // set a default
            ChangeStampData(FallingSandsDataManager.Instance.FindDataObjectFromId(2));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                FallingSandsSystem.Instance.RequestStampAtCanvasPosition(new FallingData(1), _stampTemperature, new int2(1,123), 1);
            }
            
            if (!Input.GetMouseButton(0))
            {
                myTransform = null;
                return;
            }

            var mousePos = Input.mousePosition;

            // only checking here so we don't requests writes when clicking in the ui section in the demo.. 
            var ray = ourCamera.ScreenPointToRay(mousePos);
           /* var hit2 = Physics2D.Raycast(ray.origin, ray.direction, dist,_layerMask);
            //var hit = Physics.Raycast(ray, out RaycastHit myInfo, dist, _layerMask);

            if (hit2.transform != null && myTransform == null)
            {
                myTransform = hit2.transform;

            }
            else if (hit2.transform != null && hit2.transform != myTransform)
            {
                return;
            }*/
            
            var hit = Physics.Raycast(ray, out RaycastHit info, 128f, ScreenMask, QueryTriggerInteraction.Ignore);
            if (!hit)
            {
                return;
            }
            
            // example audio usage 
            if (Time.time > _prevStampSoundAt + 0.1f)
            {
                _prevStampSoundAt = Time.time;
            
                if (_stampData.OnStamp != null)
                {
                    FallingSandsSystem.Instance.ourAudioPlayer.PlaySoundBurst(_stampData.OnStamp, 0.1f + Mathf.SmoothStep(0, 0.1f, StampSize / 10f));
                }
            }

            // request a write here 
            FallingSandsSystem.Instance.RequestStampAtScreenPosition(new FallingData(_stampData.Id), _stampTemperature, mousePos, StampSize);
        }

        public void ChangeStampData(FallingSandsDataObj data)
        {
            _stampData = data;
            _stampTemperature = data.Metadata.temperature;

        }

        public void ChangeStampSize(float newSize)
        {
            StampSize = (int) math.clamp(newSize, 1, 10);
        }
    }
}