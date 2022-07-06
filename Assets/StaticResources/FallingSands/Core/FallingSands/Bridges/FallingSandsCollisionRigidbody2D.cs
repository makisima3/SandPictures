namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingSandsCollisionRigidbody2D : MonoBehaviour
    {
        [System.NonSerialized] public Rigidbody2D ourBody;

        private void Start()
        {
            ourBody = GetComponent<Rigidbody2D>();
            FallingSandsSystem.Instance.RegisterRigidbody(this);
        }

        private void OnDestroy()
        {
            FallingSandsSystem.Instance.UnRegisterRigidbody(this);
        }
    }
}