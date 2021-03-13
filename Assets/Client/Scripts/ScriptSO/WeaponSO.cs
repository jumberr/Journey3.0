using UnityEngine;

namespace Client.Scripts.ScriptSO
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponScriptableObject")]
    public class WeaponSO : ScriptableObject
    {
        public enum WeaponType
        {
            Glock17 = 0,
            Ak47 = 1,
            M4,
            Shotgun,
            M249,
            M107
        }

        [SerializeField] private WeaponType weaponType;
        [SerializeField] private string weaponName;
        [SerializeField] private float fireRange;
        [SerializeField] private float fireRate;
        [SerializeField] private int currAmmoMagazine;
        [SerializeField] private int ammoMagazine;
        [SerializeField] private int allAmmo;
        [SerializeField] private bool isAuto;
        [SerializeField] private float damage;
        [SerializeField] private float reloadTime;
        [SerializeField] private float accuracy;
        [SerializeField] private float accuracyDistance;
        [SerializeField] private AudioClip shootAudio;
        [SerializeField] private AudioClip reloadAudio;
        [SerializeField] private AudioClip zoomAudio;
        [SerializeField] private AudioClip changeGun;
        
        public WeaponType TypeWeapon
        {
            get => weaponType;
            set => weaponType = value;
        }

        public string WeaponName => weaponName;
        public float FireRange => fireRange;
        public float FireRate => fireRate;
        public int CurrAmmoMagazine
        {
            get => currAmmoMagazine;
            set => currAmmoMagazine = value;
        }
        public int AmmoMagazine
        {
            get => ammoMagazine;
        }
        public int AllAmmo
        {
            get => allAmmo;
            set => allAmmo = value;
        }
        public bool IsAuto => isAuto;
        public float Damage => damage;
        public float ReloadTime => reloadTime;
        public float Accuracy
        {
            get => accuracy;
            set => accuracy = value;
        }

        public float AccuracyDistance
        {
            get => accuracyDistance;
            set => accuracyDistance = value;
        }

        public AudioClip ShootAudio => shootAudio;
        public AudioClip ReloadAudio => reloadAudio;
        public AudioClip ZoomAudio => zoomAudio;
        public AudioClip ChangeGun => changeGun;
    }
}