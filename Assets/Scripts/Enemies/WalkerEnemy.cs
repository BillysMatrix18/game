using UnityEngine;

namespace StarboundSprint.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class WalkerEnemy : EnemyBase
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private Transform edgeCheck;
        [SerializeField] private float edgeCheckDistance = 0.5f;
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D _rb;
        private int _dir = -1;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (IsFrozen)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            _rb.linearVelocity = new Vector2(_dir * speed, _rb.linearVelocity.y);

            Vector2 origin = edgeCheck.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);
            if (!hit)
            {
                Flip();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Wall"))
            {
                Flip();
            }
        }

        private void Flip()
        {
            _dir *= -1;
            transform.localScale = new Vector3(_dir, 1f, 1f);
        }
    }
}
