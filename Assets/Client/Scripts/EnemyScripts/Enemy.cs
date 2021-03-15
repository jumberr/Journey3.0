using System.Collections;
using System.Collections.Generic;
using Client.Classes;
using Client.Scripts.ScriptSO;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Scripts.EnemyScripts
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy characteristics")] 
        [SerializeField] private EnemySO enemySo;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private LineRenderer line;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private AnimatorController animatorController;
        [SerializeField] private Animator enemyAnimator;
        [SerializeField] private float enemyChillTime;

        private Transform player;
        private Collider playerCollider;
        private bool init;
        private Dictionary<string, int> animationDict = new Dictionary<string, int>();
        private EnemyRaw enemyRaw;
        private StateMachine stateMachine;
        
        public LineRenderer Line => line;
        public StateMachine StateMachine => stateMachine;
        public Dictionary<string, int> AnimationDict
        {
            get => animationDict;
            set => animationDict = value;
        }
        public EnemyRaw EnemyRaw => enemyRaw;
        public EnemySO EnemySo => enemySo;

        public Animator EnemyAnimator
        {
            get => enemyAnimator;
            set => enemyAnimator = value;
        }

        private void Start()
        {
            playerCollider = Player.localPlayer.Transform.GetComponent<Collider>();
            enemyRaw = EnemyRaw.GetEnemyRawByTransform(transform);
            agent.speed = enemyRaw.MoveSpeed;
            player = Player.localPlayer.Transform;
            
            stateMachine = new StateMachine(this, player, enemyRaw, agent, playerCollider, enemyAnimator, layerMask, enemyChillTime);

            EnemyAnimations.GetBlendMotionsCount(this, animatorController);
            StartCoroutine(stateMachine.CheckDistances());
        }

        private void OnEnable()
        {
            if (!init)
            {
                init = !init;
                return;
            }

            StartCoroutine(stateMachine.CheckDistances());
        }
        
        public IEnumerator StateWrapper(IEnumerator state)
        {
            while (state.MoveNext())
                yield return state.Current;
        }
    }
}