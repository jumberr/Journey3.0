using UnityEngine;

namespace Client.Scripts.ScriptSO
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EnemyScriptableObject")]
    public class EnemySO : ScriptableObject
    {
        public enum EnemyType
        {
            Melee,
            Ranged
        }

        [SerializeField] private EnemyType enemyType;
        [SerializeField] private float health;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float damage;
        [SerializeField] private float attackDistance;
        [SerializeField] private float chasingDistance;
        [SerializeField] private float endAttackDistance;
        [SerializeField] private float endChasingDistance;
        
        public EnemyType TypeEnemy
        {
            get => enemyType;
            set => enemyType = value;
        }
        public float Health
        {
            get => health;
            set => health = value;
        }
        public float MoveSpeed => moveSpeed;
        public float Damage => damage;
        public float AttackDistance => attackDistance;
        public float ChasingDistance => chasingDistance;
        public float EndAttackDistance => endAttackDistance;
        public float EndChasingDistance => endChasingDistance;
    }
}
