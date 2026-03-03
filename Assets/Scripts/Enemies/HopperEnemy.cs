using UnityEngine;

namespace StarboundSprint.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class HopperEnemy : EnemyBase
    {
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float jumpInterval = 1.8f;
        [SerializeField] private float horizontalHopForce = 2f;

        private Rigidbody2D _rb;
        private float _timer;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (IsFrozen)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = jumpInterval;
                Hop();
            }
        }

        private void Hop()
        {
            int dir = Random.value > 0.5f ? 1 : -1;
            _rb.linearVelocity = new Vector2(dir * horizontalHopForce, 0f);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}
