using UnityEngine;
using UnityEngine.Events;

namespace StarboundSprint.World
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float startTimeSeconds = 300f;

        public UnityEvent<int> OnCoinsChanged;
        public UnityEvent<int> OnLivesChanged;
        public UnityEvent<int> OnTimerChanged;
        public UnityEvent<string> OnPowerupChanged;

        private int _coins;
        private int _lives;
        private float _timer;

        private void Start()
        {
            _lives = startingLives;
            _timer = startTimeSeconds;
            BroadcastAll();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            OnTimerChanged?.Invoke(Mathf.Max(0, Mathf.CeilToInt(_timer)));
            if (_timer <= 0f)
            {
                LoseLife();
                _timer = startTimeSeconds;
            }
        }

        public void AddCoin(int amount = 1)
        {
            _coins += amount;
            OnCoinsChanged?.Invoke(_coins);
        }

        public void LoseLife()
        {
            _lives = Mathf.Max(0, _lives - 1);
            OnLivesChanged?.Invoke(_lives);
        }

        public void ReachGoal()
        {
            Systems.SaveSystem.SaveProgress(new Systems.SaveData
            {
                coins = _coins,
                lives = _lives,
                remainingTime = Mathf.CeilToInt(_timer)
            });
        }

        public void EnterWarp(string destinationScene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(destinationScene);
        }

        private void BroadcastAll()
        {
            OnCoinsChanged?.Invoke(_coins);
            OnLivesChanged?.Invoke(_lives);
            OnTimerChanged?.Invoke(Mathf.CeilToInt(_timer));
        }
    }
}
