using UnityEngine;

namespace StarboundSprint.Enemies
{
    public class FlyingEnemy : EnemyBase
    {
        [SerializeField] private float horizontalSpeed = 1.5f;
        [SerializeField] private float waveAmplitude = 0.8f;
        [SerializeField] private float waveFrequency = 2f;

        private Vector3 _startPos;

        protected override void Awake()
        {
            base.Awake();
            _startPos = transform.position;
        }

        private void Update()
        {
            if (IsFrozen)
            {
                return;
            }

            float x = transform.position.x + horizontalSpeed * Time.deltaTime;
            float y = _startPos.y + Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
            transform.position = new Vector3(x, y, _startPos.z);
        }
    }
}
