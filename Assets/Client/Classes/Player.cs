using Client.Scripts.Misc_Scripts;
using Client.Scripts.MiscScripts;
using Client.Scripts.PlayerScripts;
using Client.Scripts.ScriptSO;
using UnityEngine;

namespace Client.Classes
{
    public class Player : Person
    {
        public static Player localPlayer;

        public Transform Transform { get; }
        public PlayerSO PlayerSo { get; }
        public PlayerMovement PlayerMovement { get; }
        public PlayerController PlayerController { get; }
        public WeaponController WeaponController { get; }
        public PlayerHealth PlayerHealth { get; }
        public Animator PlayerAnimator { get; }
        
        public float StaminaCurrent { get; set; }
        public float Stamina { get; }
        public float StaminaLess { get; }
        public float StaminaRenew { get; }

        public Player(Transform player, PlayerHealth playerHealth, PlayerMovement playerMovement,
            WeaponController weaponController, Animator playerAnimator, float stamina, float staminaLess,
            float staminaRenew, PlayerController playerController, PlayerSO playerSo) : base(playerSo.Health, playerSo.Health, playerSo.MoveSpeed)
        {
            localPlayer = this;
            Transform = player;
            PlayerHealth = playerHealth;
            PlayerMovement = playerMovement;
            WeaponController = weaponController;
            PlayerSo = playerSo;
            Stamina = stamina;
            StaminaCurrent = stamina;
            StaminaLess = staminaLess;
            StaminaRenew = staminaRenew;
            WeaponController = weaponController;
            PlayerController = playerController;
            PlayerAnimator = playerAnimator;
        }

        public override void Die()
        {
            //Player is dead
        }
    }
}