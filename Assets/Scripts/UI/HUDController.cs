using UnityEngine;
using UnityEngine.UI;
using StarboundSprint.World;

namespace StarboundSprint.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private Text livesText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text powerUpText;

        private void OnEnable()
        {
            if (levelManager == null)
            {
                return;
            }

            levelManager.OnLivesChanged.AddListener(UpdateLives);
            levelManager.OnCoinsChanged.AddListener(UpdateCoins);
            levelManager.OnTimerChanged.AddListener(UpdateTimer);
            levelManager.OnPowerupChanged.AddListener(UpdatePowerup);
        }

        private void OnDisable()
        {
            if (levelManager == null)
            {
                return;
            }

            levelManager.OnLivesChanged.RemoveListener(UpdateLives);
            levelManager.OnCoinsChanged.RemoveListener(UpdateCoins);
            levelManager.OnTimerChanged.RemoveListener(UpdateTimer);
            levelManager.OnPowerupChanged.RemoveListener(UpdatePowerup);
        }

        private void UpdateLives(int lives) => livesText.text = $"Lives: {lives}";
        private void UpdateCoins(int coins) => coinsText.text = $"Coins: {coins}";
        private void UpdateTimer(int timeLeft) => timerText.text = $"Time: {timeLeft}";
        private void UpdatePowerup(string power) => powerUpText.text = $"Power: {power}";
    }
}
