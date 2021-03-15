using System.Collections.Generic;
using System.Linq;
using Client.Scripts.EnemyScripts;
using Client.Scripts.ScriptSO;
using UnityEngine;

namespace Client.Classes
{
    public class EnemyRaw : Person
    {
        private static List<EnemyRaw> enemies = new List<EnemyRaw>();

        private static int nextIndex;
        private int index;
        public Enemy Enemy { get; }
        public GameObject GameObject { get; }
        public Transform Transform { get; }

        private readonly EnemySO enemySo;
        private EnemyHealthBar healthBar;

        private readonly float damage;
        private readonly float attackDistance;
        private readonly float chasingDistance;
        private readonly float endAttackDistance;
        private readonly float endChasingDistance;

        public event EnemySpawnController.OnDie OnRespawn;
        public EnemyRaw(GameObject gameObject, Transform transform, Enemy enemy, EnemySO enemySo,
            EnemyHealthBar healthBar, EnemySpawnController.OnDie onRespawn)
            : base(enemySo.Health, enemySo.Health, enemySo.MoveSpeed)
        {
            index = nextIndex++;
            GameObject = gameObject;
            Transform = transform;
            Enemy = enemy;
            this.enemySo = enemySo;
            this.healthBar = healthBar;
            damage = enemySo.Damage;
            attackDistance = enemySo.AttackDistance;
            chasingDistance = enemySo.ChasingDistance;
            endAttackDistance = enemySo.EndAttackDistance;
            endChasingDistance = enemySo.EndChasingDistance;
            enemies.Add(this);
            OnRespawn = onRespawn;
        }
    
        public EnemySO EnemySo => enemySo;
        public float Damage => damage;
        public float AttackDistance => attackDistance;
        public float ChasingDistance => chasingDistance;
        public float EndAttackDistance => endAttackDistance;
        public float EndChasingDistance => endChasingDistance;

        public override void Die()
        {
            //enemies.Remove(this);
            GameObject.SetActive(false);
            OnRespawn?.Invoke(Enemy);
        }

        public static EnemyRaw GetEnemyByIndex(int index)
        {
            return enemies.First(enemy => enemy.index == index);
        }

        public static EnemyRaw GetEnemyRawByGameObject(GameObject enemyGameObject)
        {
            return enemies.First(enemyRaw => enemyRaw.GameObject.Equals(enemyGameObject));
        }

        public static EnemyRaw GetEnemyRawByTransform(Transform enemyTransform)
        {
            return enemies.First(enemyRaw => enemyRaw.Transform.Equals(enemyTransform));
        }

        public static EnemyRaw GetEnemyRawByEnemy(Enemy enemy)
        {
            return enemies.First(enemyRaw => enemyRaw.Enemy.Equals(enemy));
        }
    }
}