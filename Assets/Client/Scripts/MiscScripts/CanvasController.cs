using UnityEngine;
using UnityEngine.UI;

namespace Client.Scripts.Misc_Scripts
{
    public class CanvasController : MonoBehaviour
    {
        public static CanvasController UI;

        [Header("Player Settings UI")]
        [SerializeField] private Slider sliderStamina;
        [SerializeField] private Slider sliderHealth;
        [SerializeField] private Image reloadCircle;
        [SerializeField] private Text ammoText;
        [SerializeField] private Image scopeWeapon;
        private Transform settingsUI;
        public Slider SliderHealth => sliderHealth;
        public Slider SliderStamina => sliderStamina;
        public Image ReloadCircle => reloadCircle;
        public Text AmmoText => ammoText;
        public Image ScopeWeapon => scopeWeapon;
        
        private void Awake()
        {
            if (UI == null)
                UI = this;
            else
                Destroy(this);
        }
    }
}
