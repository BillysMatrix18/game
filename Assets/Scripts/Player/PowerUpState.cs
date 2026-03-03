using UnityEngine;
using StarboundSprint.Combat;

namespace StarboundSprint.Player
{
    public enum PowerUpForm
    {
        Normal,
        BlazeFruit,
        FrostCap,
        ShellArmor
    }

    public class PowerUpState : MonoBehaviour
    {
        [SerializeField] private PowerUpForm currentForm = PowerUpForm.Normal;
        [SerializeField] private bool hasDoubleJump;
        [SerializeField] private float defenseMultiplier = 1f;

        public PowerUpForm CurrentForm => currentForm;
        public bool HasDoubleJump => hasDoubleJump;
        public bool CanShootProjectile => currentForm == PowerUpForm.BlazeFruit || currentForm == PowerUpForm.FrostCap;
        public DamageType ProjectileType => currentForm == PowerUpForm.FrostCap ? DamageType.Frost : DamageType.Fire;
        public float DefenseMultiplier => defenseMultiplier;

        public void SetForm(PowerUpForm newForm)
        {
            currentForm = newForm;
            ApplyFormStats();
        }

        private void ApplyFormStats()
        {
            switch (currentForm)
            {
                case PowerUpForm.Normal:
                    defenseMultiplier = 1f;
                    break;
                case PowerUpForm.BlazeFruit:
                    defenseMultiplier = 1f;
                    break;
                case PowerUpForm.FrostCap:
                    defenseMultiplier = 1f;
                    break;
                case PowerUpForm.ShellArmor:
                    defenseMultiplier = 0.65f;
                    break;
            }
        }

        public void UnlockDoubleJump(bool unlocked)
        {
            hasDoubleJump = unlocked;
        }
    }
}
