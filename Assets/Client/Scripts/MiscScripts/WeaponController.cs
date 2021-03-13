using System.Collections;
using Client.Classes;
using Client.Scripts.EnemyScripts;
using Client.Scripts.Misc_Scripts;
using Client.Scripts.ScriptSO;
using UnityEngine;
using UnityEngine.Events;

namespace Client.Scripts.MiscScripts
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        
        [Header("Muzzle flash particles")] 
        [SerializeField] private float lightDuration = 0.02f;

        [SerializeField] private Light gunLight;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private ParticleSystem muzzleParticles;
        [SerializeField] private GameObject bloodEffect;
        [SerializeField] private GameObject holeEffect;

        [Header("Weapon Audio")] 
        [SerializeField] private AudioSource audioSource;

        [Header("Arms & gun transform")] 
        [SerializeField] private GameObject armsPistol;
        [SerializeField] private GameObject armsRifle;
        [SerializeField] private GameObject[] guns;
        [SerializeField] private WeaponSO[] weaponStats;
        
        private UnityEvent ammoTextEvent;
        private UnityEvent aimingEvent;
        private UnityEvent endAimingEvent;

        private bool auto;
        private float nextFire;
        private float fireRatio;
        private int currentAmmo; // in current magazine
        private int leftAmmo; // all ammo
        private bool isReloading;
        private bool isAiming;
        private float waitBeforeAnimation;
        private WeaponSO[] editableWeaponStats;
        private WeaponSO weaponSo;
        private static readonly int Shoot = Animator.StringToHash("Shoot");
        private static readonly int Aiming = Animator.StringToHash("Aiming");
        private static readonly int ShootAiming = Animator.StringToHash("Shoot when aiming");
        private static readonly int ReloadAmmo = Animator.StringToHash("ReloadAmmo");
        private static readonly int EndZoom = Animator.StringToHash("End Zoom");
        private static readonly int ShowGun = Animator.StringToHash("Show Gun");

        private KeyCode[] inputGun = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6
        };

        public bool IsAiming
        {
            get => isAiming;
            set => isAiming = value;
        }

        private void Start()
        {
            CreateLocalSo();

            if (ammoTextEvent == null)
                ammoTextEvent = new UnityEvent();
            if (aimingEvent == null)
                aimingEvent = new UnityEvent();
            if (endAimingEvent == null)
                endAimingEvent = new UnityEvent();

            ammoTextEvent.AddListener(ShowAmmo);
            ApplyNewCharacteristics();

            aimingEvent.AddListener(Aim);
            endAimingEvent.AddListener(EndAim);
        }

        private void Update()
        {
            if (isReloading)
                return;
            // shooting for automatic weapons
            if (Input.GetButton("Fire1") && Time.time > nextFire && auto)
            {
                nextFire = Time.time + fireRatio;
                Shooting();
                ammoTextEvent.Invoke();
            }
            // shooting for non-automatic  weapons
            else if (Input.GetButtonDown("Fire1") && !auto)
            {
                Shooting();
                ammoTextEvent.Invoke();
            }

            //aiming
            if (Input.GetButtonDown("Fire2") && !isReloading)
            {
                isAiming = !isAiming;
                if (isAiming)
                    aimingEvent.Invoke();
                else
                    endAimingEvent.Invoke();
            }

            //Reloading
            if (Input.GetKeyDown(KeyCode.R) && currentAmmo != weaponSo.AmmoMagazine && leftAmmo > 0 && !isReloading)
            {
                StartCoroutine(Reload());
            }

            CheckForChangingGun();
        }

        private void CreateLocalSo()
        {
            editableWeaponStats = new WeaponSO[weaponStats.Length];
            for (var i = 0; i < editableWeaponStats.Length; i++)
            {
                editableWeaponStats[i] = Instantiate(weaponStats[i]);
            }

            weaponSo = editableWeaponStats[1];
        }

        private void CheckForChangingGun()
        {
            for (var i = 0; i < inputGun.Length; i++)
            {
                if (Input.GetKeyDown(inputGun[i]))
                {
                    StartCoroutine(ChangeGun((WeaponSO.WeaponType)i));
                }
            }
        }

        private IEnumerator ChangeGun( WeaponSO.WeaponType nextWeapon)
        {
            if(weaponSo.TypeWeapon == nextWeapon)
                yield break;

            waitBeforeAnimation = 0.2f;
            var prevIndex = (int)weaponSo.TypeWeapon;
            var prevGun = guns[prevIndex];

            SaveAmmoLocal(prevIndex); // save ammo to instantiated db
            
            var nextIndex = (int) (nextWeapon);
            var nextGun = guns[nextIndex];
            
            if (nextWeapon == WeaponSO.WeaponType.Glock17) // change to pistol
            {
                if (weaponSo.TypeWeapon != WeaponSO.WeaponType.Glock17) // current gun
                {
                    Player.localPlayer.PlayerAnimator.SetTrigger(ShowGun);
                    audioSource.PlayOneShot(weaponSo.ChangeGun);
                    yield return new WaitForSeconds(waitBeforeAnimation);
                    armsRifle.SetActive(false);
                    armsPistol.SetActive(true); // change arms for pistol pose
                    ChangeWithoutArms(prevGun, nextGun, nextIndex);
                    ApplyNewCharacteristics();
                }
                else
                {
                    Player.localPlayer.PlayerAnimator.SetTrigger(ShowGun);
                    audioSource.PlayOneShot(weaponSo.ChangeGun);
                    yield return new WaitForSeconds(waitBeforeAnimation);
                    ChangeWithoutArms(prevGun, nextGun, nextIndex);
                    ApplyNewCharacteristics();
                }
            }
            else // change to rifle, shotgun, etc
            {
                if (weaponSo.TypeWeapon == WeaponSO.WeaponType.Glock17) // current gun is pistol
                {
                    Player.localPlayer.PlayerAnimator.SetTrigger(ShowGun);
                    audioSource.PlayOneShot(weaponSo.ChangeGun);
                    yield return new WaitForSeconds(waitBeforeAnimation);
                    armsPistol.SetActive(false);
                    armsRifle.SetActive(true);
                    ChangeWithoutArms(prevGun, nextGun, nextIndex);
                    ApplyNewCharacteristics();
                }
                else
                {
                    Player.localPlayer.PlayerAnimator.SetTrigger(ShowGun);
                    audioSource.PlayOneShot(weaponSo.ChangeGun);
                    yield return new WaitForSeconds(waitBeforeAnimation);
                    ChangeWithoutArms(prevGun, nextGun, nextIndex);
                    ApplyNewCharacteristics();
                }
            }
        }

        private void ChangeWithoutArms(GameObject prevGun, GameObject nextGun, int nextIndex)
        {
            nextGun.SetActive(true); //change gun
            prevGun.SetActive(false);
            weaponSo = editableWeaponStats[nextIndex]; //change stats of current gun
        }

        private void SaveAmmoLocal(int prevIndex)
        {
            editableWeaponStats[prevIndex].AllAmmo = leftAmmo;
            editableWeaponStats[prevIndex].CurrAmmoMagazine = currentAmmo;
        }

        private void ApplyNewCharacteristics()
        {
            auto = weaponSo.IsAuto;
            fireRatio = weaponSo.FireRate;
            currentAmmo = weaponSo.CurrAmmoMagazine;
            leftAmmo = weaponSo.AllAmmo;
            ammoTextEvent.Invoke();
        }

        private void Shooting()
        {
            //Reload
            if (currentAmmo == 0 && leftAmmo > 0 && !isReloading)
            {
                StartCoroutine(Reload());
            }
            
            if (currentAmmo > 0 && !isReloading)
            {
                // weapon SFX 
                audioSource.volume = Random.Range(0.7f, 0.8f);
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(weaponSo.ShootAudio);

                // weapon shoot animation
                Player.localPlayer.PlayerAnimator.SetTrigger(isAiming ? ShootAiming : Shoot);

                StartCoroutine(MuzzleEffect());

                var dir = RandShootDir();

                if (Physics.Raycast(cam.transform.position, dir, out var hit, weaponSo.FireRange))
                {
                    if (hit.transform.TryGetComponent<Enemy>(out var enemy))
                    {
                        if (enemy.EnemySo.TypeEnemy == EnemySO.EnemyType.Melee)
                        {
                            //enemy.SetAnimation("Damage");
                            StartCoroutine(enemy.SetAnimationDamage());
                        }
                        DoDamage(enemy, weaponSo.Damage);
                        
                        // blood anim
                        var blood = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(blood, 1f);
                    }
                    else
                    {
                        // hole anim
                        var hole = Instantiate(holeEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(hole, 1f);
                    }
                }

                currentAmmo--;
            }
        }

        #region RandScatter

        private float RandDistance() => weaponSo.Accuracy * Mathf.Sqrt(-2 * Mathf.Log(1 - Random.Range(0, 1f)));

        private Vector3 RandShootDir()
        {
            var distanceDispersion = RandDistance(); // dispersion distance
            var angleDispersion = Random.Range(0, 2 * Mathf.PI); // dispersion angle

            var coordX = distanceDispersion * Mathf.Cos(angleDispersion);
            var coordY = distanceDispersion * Mathf.Sin(angleDispersion);
            return cam.transform.forward * weaponSo.AccuracyDistance + new Vector3(coordX, coordY, 0);
        }

        #endregion

        private void Aim()
        {
            Player.localPlayer.PlayerAnimator.SetTrigger(Aiming);
            Player.localPlayer.PlayerAnimator.SetBool(EndZoom, false);
            CanvasController.UI.ScopeWeapon.enabled = false;
            // audio scope on
            audioSource.PlayOneShot(weaponSo.ZoomAudio);
        }

        private void EndAim()
        {
            Player.localPlayer.PlayerAnimator.SetBool(EndZoom, true);
            audioSource.PlayOneShot(weaponSo.ZoomAudio);
            StartCoroutine(ScopeOn());
        }

        private IEnumerator ScopeOn()
        {
            yield return new WaitForSeconds(0.6f);
            CanvasController.UI.ScopeWeapon.enabled = true;
        }

        private IEnumerator Reload()
        {
            if (isAiming)
            {
                isAiming = !isAiming;
                EndAim();
                yield return new WaitForSeconds(
                    Player.localPlayer.PlayerAnimator.GetCurrentAnimatorStateInfo(0).length +
                    Player.localPlayer.PlayerAnimator.GetCurrentAnimatorStateInfo(0)
                        .normalizedTime);
            }

            isReloading = true;
            Player.localPlayer.PlayerAnimator.SetTrigger(ReloadAmmo);
            audioSource.PlayOneShot(weaponSo.ReloadAudio);
            #region fill reload image

            var time = 0f;
            const float start = 0f;
            var end = weaponSo.ReloadTime;
            CanvasController.UI.ReloadCircle.fillAmount = 0f;
            while (time < end)
            {
                var t = Mathf.InverseLerp(start, end, time);
                time += Time.deltaTime;
                CanvasController.UI.ReloadCircle.fillAmount = t;
                yield return null;
            }

            CanvasController.UI.ReloadCircle.fillAmount = 0f;

            #endregion

            #region Ammo Logic

            var usedAmmo = weaponSo.AmmoMagazine - currentAmmo; // ammo, that were used (shoot)
            if (leftAmmo > usedAmmo)
            {
                leftAmmo -= usedAmmo;
                currentAmmo += usedAmmo;
            }

            if (leftAmmo <= usedAmmo)
            {
                currentAmmo += leftAmmo;
                leftAmmo = 0;
                if (currentAmmo > weaponSo.AmmoMagazine)
                {
                    var overMagazine = currentAmmo - weaponSo.AmmoMagazine;
                    currentAmmo = weaponSo.AmmoMagazine;
                    leftAmmo += overMagazine;
                }
            }

            #endregion

            isReloading = false;
            ammoTextEvent.Invoke();
        }

        private IEnumerator MuzzleEffect()
        {
            // muzzle flash
            gunLight.enabled = true;
            muzzleFlash.Play();
            muzzleParticles.Play();
            yield return new WaitForSecondsRealtime(lightDuration);
            gunLight.enabled = false;
        }

        private void ShowAmmo()
        {
            CanvasController.UI.AmmoText.text = $"{currentAmmo} / {leftAmmo}";
        }

        private void DoDamage(Enemy enemy, float damage)
        {
            enemy.EnemyRaw.TakeDamage(damage);
        }
    }
}