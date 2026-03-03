using UnityEngine;
using StarboundSprint.Combat;

namespace StarboundSprint.Enemies
{
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected int maxHealth = 1;
        [SerializeField] protected bool immuneToStomp;
        [SerializeField] protected bool immuneToProjectiles;
        [SerializeField] protected bool freezeable = true;

        protected int CurrentHealth;
        protected bool IsFrozen;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public virtual void TakeDamage(int amount, DamageType damageType)
        {
            if (immuneToProjectiles && (damageType == DamageType.Fire || damageType == DamageType.Frost))
            {
                return;
            }

            if (damageType == DamageType.Frost && freezeable)
            {
                IsFrozen = true;
            }

            CurrentHealth -= amount;
            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public virtual bool TryStomp()
        {
            if (immuneToStomp)
            {
                return false;
            }

            TakeDamage(1, DamageType.Physical);
            return true;
        }

        protected virtual void Die()
        {
            Destroy(gameObject);
        }
    }
}
