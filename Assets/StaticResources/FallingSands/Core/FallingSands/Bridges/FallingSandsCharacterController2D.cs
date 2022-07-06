namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;

    [RequireComponent(typeof(FallingSandsCollisionRigidbody2D))]
    public class FallingSandsCharacterController2D : MonoBehaviour
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

        [Header("Visuals")]
        public SpriteRenderer OurSpriteRenderer;
        public Sprite SpriteIdle;
        public Sprite[] SpriteWalk;
        public float SpriteWalkSpeed;

        [System.NonSerialized] public FallingSandsCollisionRigidbody2D fsBody;
        [System.NonSerialized] public Rigidbody2D ourBody;
        [System.NonSerialized] public CircleCollider2D ourCollider;
        [System.NonSerialized] private float _animStep;

        private void Start()
        {
            fsBody = GetComponent<FallingSandsCollisionRigidbody2D>();
            ourBody = GetComponent<Rigidbody2D>();
            ourCollider = GetComponent<CircleCollider2D>();
        }

        private void UpdateCharacterSprite(bool walking, bool flip)
        {
            _animStep += Time.deltaTime;

            var targetSprite = SpriteIdle;

            if(walking)
            {
                targetSprite = SpriteWalk[ (int) Mathf.Repeat(_animStep * SpriteWalkSpeed, SpriteWalk.Length) ];
            }

            OurSpriteRenderer.sprite = targetSprite;

            var localScale = OurSpriteRenderer.transform.localScale;
                localScale.x = flip ? -Mathf.Abs(localScale.x): Mathf.Abs(localScale.x);

            OurSpriteRenderer.transform.localScale = localScale;

        }

        private void UpdateMovement(Vector2 moveDir)
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position + transform.up * ourCollider.offset.y, Vector2.down, ourCollider.radius + 0.02f, FloorMask);
            if (hitInfo.collider != null)
            {
                // friction 
                ourBody.velocity = ourBody.velocity.normalized * (ourBody.velocity.magnitude * (1f - Time.deltaTime * Friction));

                // jump 
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ourBody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
                }

                // movement 
                ourBody.velocity = Vector2.Lerp(ourBody.velocity, moveDir * MoveSpeed, Time.deltaTime * 10f);

                // no gravity when touching the ground
                ourBody.gravityScale = 0f;
            }
            else
            {
                // air movement 
                ourBody.AddForce(moveDir * MoveSpeedInAir * Time.deltaTime, ForceMode2D.Impulse);

                // return gravity when in the air 
                ourBody.gravityScale = 1f;
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

        [System.NonSerialized] private bool _flipDir;

        private void Update()
        {
            var moveDir = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) moveDir -= Vector2.right;
            if (Input.GetKey(KeyCode.D)) moveDir += Vector2.right;

            if (moveDir.x < 0) _flipDir = true;
            if (moveDir.x > 0) _flipDir = false;

            UpdateMovement(moveDir); 
            UpdateCharacterSprite(moveDir.magnitude > 0.01f, _flipDir); 
        }
    }
}