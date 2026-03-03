using UnityEngine;

namespace StarboundSprint.Boss
{
    public class BossController : MonoBehaviour
    {
        [System.Serializable]
        public class BossPhase
        {
            public float healthThreshold = 0.66f;
            public float attackInterval = 1.6f;
            public int projectileBurst = 3;
            public float dashSpeed = 8f;
        }

        [SerializeField] private int maxHealth = 12;
        [SerializeField] private BossPhase[] phases;

        private int _health;
        private int _phaseIndex;
        private float _attackTimer;
        private bool _weakPointOpen;

        private void Awake()
        {
            _health = maxHealth;
            _attackTimer = phases != null && phases.Length > 0 ? phases[0].attackInterval : 2f;
        }

        private void Update()
        {
            if (_health <= 0)
            {
                return;
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                ExecutePattern();
            }
        }

        public void OpenWeakPoint(float duration)
        {
            _weakPointOpen = true;
            Invoke(nameof(CloseWeakPoint), duration);
        }

        public void TakeHit(int damage)
        {
            if (!_weakPointOpen)
            {
                return;
            }

            _health -= damage;
            RefreshPhase();

            if (_health <= 0)
            {
                DefeatBoss();
            }
        }

        private void ExecutePattern()
        {
            BossPhase phase = phases[Mathf.Clamp(_phaseIndex, 0, phases.Length - 1)];
            _attackTimer = phase.attackInterval;
            OpenWeakPoint(0.8f);
        }

        private void RefreshPhase()
        {
            float ratio = (float)_health / maxHealth;
            for (int i = phases.Length - 1; i >= 0; i--)
            {
                if (ratio <= phases[i].healthThreshold)
                {
                    _phaseIndex = i;
                    return;
                }
            }

            _phaseIndex = 0;
        }

        private void CloseWeakPoint()
        {
            _weakPointOpen = false;
        }

        private void DefeatBoss()
        {
            Destroy(gameObject);
        }
    }
}
