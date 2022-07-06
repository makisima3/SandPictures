namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;

    public class FallingSandsSampleAtTransform : MonoBehaviour
    {
        public SampleData Sample;
        private int _sampleTicket;

        private void OnEnable()
        {
            _sampleTicket = -1; 
        }

        private void Update()
        {
            if(_sampleTicket >= 0)
            {
                Sample = FallingSandsSystem.Instance.GetSample(_sampleTicket);
            }

            _sampleTicket = FallingSandsSystem.Instance.RequestSampleAtWorldPosition(transform.position); 
        }
    }
}