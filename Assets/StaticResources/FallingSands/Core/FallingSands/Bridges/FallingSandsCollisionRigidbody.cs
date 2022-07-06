namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class FallingSandsCollisionRigidbody : MonoBehaviour
    {
        [System.NonSerialized] public Rigidbody ourBody;

        private void Start()
        {
            ourBody = GetComponent<Rigidbody>();
            FallingSandsSystem.Instance.RegisterRigidbody(this);
        }

        private void OnDestroy()
        {
            FallingSandsSystem.Instance.UnRegisterRigidbody(this);
        }
    }
}