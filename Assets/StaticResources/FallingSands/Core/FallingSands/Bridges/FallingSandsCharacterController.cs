namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;

    [RequireComponent(typeof(FallingSandsCollisionRigidbody))]
    public class FallingSandsCharacterController : MonoBehaviour
    {
        [Header("Camera")]
        public Camera MainCamera;
        public bool ShiftWorldWhileMoving;

        [Header("Movement")]
        public float MoveSpeed = 1f;
        public float MoveSpeedInAir = 1f;
        public float JumpForce = 10f;
        public float Friction = 1f;
        public LayerMask FloorMask;

        [System.NonSerialized] public FallingSandsCollisionRigidbody fsBody;
        [System.NonSerialized] public Rigidbody ourBody;
        [System.NonSerialized] public SphereCollider ourCollider;

        private void Start()
        {
            fsBody = GetComponent<FallingSandsCollisionRigidbody>();
            ourBody = GetComponent<Rigidbody>();
            ourCollider = GetComponent<SphereCollider>();
        }

        private void Update()
        {
            var moveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.A)) moveDir -= Vector3.right;
            if (Input.GetKey(KeyCode.D)) moveDir += Vector3.right;

            var touchingFloor = Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, ourCollider.radius * transform.localScale.x + 0.01f, FloorMask, QueryTriggerInteraction.Ignore);
            if (touchingFloor)
            {
                ourBody.velocity = ourBody.velocity.normalized * (ourBody.velocity.magnitude * (1f - Time.deltaTime * Friction));

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ourBody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
                }
                ourBody.AddForce(moveDir * MoveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            }
            else
            {
                ourBody.AddForce(moveDir * MoveSpeedInAir * Time.deltaTime, ForceMode.VelocityChange);
            }

            if (ShiftWorldWhileMoving)
            {
                var viewPosition = MainCamera.WorldToViewportPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
                var fromCenter = (Vector2)viewPosition - new Vector2(0.5f, 0.5f);

                if (fromCenter.magnitude > 0.1f)
                {
                    var shiftDirection = new int2((int)(fromCenter.x * 10f), (int)(fromCenter.y * 10f));
                    shiftDirection = math.clamp(shiftDirection, new int2(-1, -1), new int2(1, 1));

                    FallingSandsSystem.Instance.RequestShiftPosition += shiftDirection;
                }
            }
        }
    }
}