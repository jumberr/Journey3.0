using System.Collections;
using Client.Classes;
using Client.Scripts.Misc_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Client.Scripts.PlayerScripts
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float healthRegeneration;
        [SerializeField] private float regenerationDelay;
        [SerializeField] private float timeBetweenHealing;
        private Coroutine addHealth;
        private bool isStarted;
        
        public HealthEvent playerHealthDecrease;

        private void Start()
        {
            if (playerHealthDecrease == null)
                playerHealthDecrease = new HealthEvent();
            playerHealthDecrease.AddListener(HealthDecrease);

            StartCoroutine(Regeneration());
        }

        private void HealthDecrease(float value)
        {
            if (addHealth != null)
            {
                StopCoroutine(addHealth);
                isStarted = false;
            }
            Player.localPlayer.TakeDamage(value);
            CanvasController.UI.SliderHealth.value = Player.localPlayer.Health / Player.localPlayer.MaxHealth;
        }

        private IEnumerator Regeneration()
        {
            while (true)
            {
                if (Player.localPlayer.Health < Player.localPlayer.MaxHealth && !isStarted){
                    addHealth = StartCoroutine(HealthIncrease());
                    isStarted = true;
                }
                if (addHealth != null && Player.localPlayer.Health >= Player.localPlayer.MaxHealth && isStarted)
                {
                    StopCoroutine(addHealth);
                    isStarted = false;
                }
                yield return new WaitForSeconds(1f);
            }
        }
        private IEnumerator HealthIncrease()
        {
            yield return new WaitForSecondsRealtime(regenerationDelay);
            while (Player.localPlayer.Health < Player.localPlayer.MaxHealth)
            {
                Player.localPlayer.RestoreHealth(healthRegeneration);
                CanvasController.UI.SliderHealth.value = Player.localPlayer.Health / Player.localPlayer.MaxHealth;
                yield return new WaitForSecondsRealtime(timeBetweenHealing);
            }
        }
    }

    public class HealthEvent : UnityEvent<float>
    {
    }
}