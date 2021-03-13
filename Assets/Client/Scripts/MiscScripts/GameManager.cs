using Client.Classes;
using Client.Scripts.Misc_Scripts;
using Client.Scripts.PlayerScripts;
using UnityEngine;

namespace Client.Scripts.MiscScripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        
        [Header("Player")]
        [SerializeField] private Transform player;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
            
            InstantiatePlayer();
        }

        private Player InstantiatePlayer()
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            var playerMovement = player.GetComponent<PlayerMovement>();
            var weaponController = player.GetComponent<WeaponController>();
            var playerAnimator = player.GetComponent<Animator>();
            var playerController = player.GetComponent<PlayerController>();

            return new Player(player, playerHealth, playerMovement, weaponController, playerAnimator, playerMovement.stamina,
                playerMovement.staminaLess, playerMovement.staminaRenew, playerController, Instantiate(playerController.PlayerSo));
        }
    }
}
