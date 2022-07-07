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

        private void Start()
        {
            // set a default
            ChangeStampData(FallingSandsDataManager.Instance.FindDataObjectFromId((int)FallingDataType.Sand));
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                return;
            }

            var mousePos = Input.mousePosition;

            // only checking here so we don't requests writes when clicking in the ui section in the demo.. 
            var ray = ourCamera.ScreenPointToRay(mousePos);
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