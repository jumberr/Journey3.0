using System.Collections;
using System.Collections.Generic;
using Client.Classes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Client.Scripts.EnemyScripts
{
    public class EnemySpawnController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject[] enemiesPrefabs;
        [SerializeField] private int countOfEnemies;
        [SerializeField] private Transform parent;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnDistance;
        [SerializeField] private float respawnTime;
        private List<Transform> tempSpawn = new List<Transform>();

        public delegate void OnDie(Enemy enemy);
        public event OnDie Respawn;
        private void Awake()
        {
            CreateEnemiesStart();
        }

        private void CreateEnemiesStart()
        {
            tempSpawn = CheckDistanceToSpawn();
            for (var i = 0; i < countOfEnemies; i++)
            {
                var randIndex = Random.Range(0, enemiesPrefabs.Length);
                var spawnPosition = ChooseSpawnPoint(tempSpawn);
                var enemyGo = Instantiate(enemiesPrefabs[randIndex], spawnPosition.position, Quaternion.identity,
                    parent);
                var enemy = enemyGo.GetComponent<Enemy>();
                var enemyHealthBar = enemyGo.GetComponent<EnemyHealthBar>();
                new EnemyRaw(enemyGo, enemyGo.transform, enemy, enemy.EnemySo, enemyHealthBar, Respawn);
                Respawn += StartRespawn;
            }
        }

        private List<Transform> CheckDistanceToSpawn()  //checks distance between player and spawn points
        {
            var temp = new List<Transform>();
            var maxDist = spawnPoints[0];
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if ((spawnPoints[i].position - player.position).magnitude > spawnDistance)  // adds to list points with distance bigger than spawnDistance
                {
                    temp.Add(spawnPoints[i]);
                }

                if ((maxDist.position - player.position).magnitude < (spawnPoints[i].position - player.position).magnitude)  // finds max distance
                {
                    maxDist = spawnPoints[i];
                }
            }

            if (temp.Count.Equals(0))  // adds to list point with max distance if we haven't others
            {
                temp.Add(maxDist);
            }

            return temp;
        }

        private Transform ChooseSpawnPoint(List<Transform> list) => list[Random.Range(0, list.Count)];

        private void StartRespawn(Enemy enemy)
        {
            StartCoroutine(RespawnEnemy(enemy));
        }

        private IEnumerator RespawnEnemy(Enemy enemy)
        {
            yield return new WaitForSeconds(respawnTime);
            tempSpawn = CheckDistanceToSpawn();
            var spawnPosition = ChooseSpawnPoint(tempSpawn);
            enemy.transform.position = spawnPosition.position; // change pos
            enemy.EnemyRaw.RestoreHealth(enemy.EnemyRaw.MaxHealth); // Restore health
            enemy.EnemyRaw.GameObject.SetActive(true); // SetActive enemy
        }
    }
}