using System;
using System.Collections;
using Client.Classes;
using UnityEngine;

namespace Client.Scripts.EnemyScripts
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Enemy enemy;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject healthBar;
        private EnemyRaw enemyRaw;
        private MaterialPropertyBlock matBlock;
        private Camera mainCamera;

        private bool init;
        private void Awake()
        {
            matBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            enemyRaw = EnemyRaw.GetEnemyRawByTransform(transform);
            StartCoroutine(UpdateHealthBar());
        }

        private void OnEnable()
        {
            if (!init)
            {
                init = !init;
                return;
            }
            StartCoroutine(UpdateHealthBar());
        }

        private IEnumerator UpdateHealthBar()
        {
            while (true)
            {
                if (enemyRaw.Health < enemyRaw.MaxHealth)
                {
                    meshRenderer.enabled = true;
                    AlignCamera();
                    UpdateParams();
                }
                else
                {
                    meshRenderer.enabled = false;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void UpdateParams()
        {
            meshRenderer.GetPropertyBlock(matBlock);
            matBlock.SetFloat("_Fill", enemyRaw.Health / enemyRaw.MaxHealth);
            meshRenderer.SetPropertyBlock(matBlock);
        }

        private void AlignCamera()
        {
            if (!mainCamera.Equals(null))
            {
                var camXform = mainCamera.transform;
                var forward = healthBar.transform.position - camXform.position;
                forward.Normalize();
                var up = Vector3.Cross(forward, camXform.right);
                healthBar.transform.rotation = Quaternion.LookRotation(forward, up);
            }
        }
    }
}