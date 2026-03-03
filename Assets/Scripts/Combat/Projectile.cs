using UnityEngine;

namespace StarboundSprint.Combat
{
    public enum DamageType
    {
        Physical,
        Fire,
        Frost
    }

    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifeSeconds = 4f;

        private int _direction = 1;
        private DamageType _damageType = DamageType.Physical;

        public void Initialize(int direction, DamageType damageType)
        {
            _direction = direction;
            _damageType = damageType;
        }

        private void Update()
        {
            transform.position += Vector3.right * (_direction * speed * Time.deltaTime);
            lifeSeconds -= Time.deltaTime;
            if (lifeSeconds <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out StarboundSprint.Enemies.EnemyBase enemy))
            {
                enemy.TakeDamage(damage, _damageType);
                Destroy(gameObject);
                return;
            }

            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
