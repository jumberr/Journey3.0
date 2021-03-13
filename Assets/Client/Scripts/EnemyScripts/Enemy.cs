using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Client.Classes;
using Client.Scripts.PlayerScripts;
using Client.Scripts.ScriptSO;
using Client.Scripts.Utils;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Client.Scripts.EnemyScripts
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy characteristics")] [SerializeField]
        private EnemySO enemySo;

        [SerializeField] private NavMeshAgent agent;

        //[SerializeField] private Transform gun;
        [SerializeField] private LineRenderer line;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private AnimatorController animatorController;
        [SerializeField] private Animator enemyAnimator;
        [ReadOnlyAttribute] [SerializeField] private float enemyChillTime;

        private Transform player;
        private Collider playerCollider;

        private bool init;

        private Dictionary<string, int> animationDict = new Dictionary<string, int>();
        private const float OFF_ANIMATION_VALUE = -1f;

        private EnemyRaw enemyRaw;
        private EnemyState enemyState = new EnemyState(EnemyState.State.Patrol, EnemyState.State.Patrol);

        private delegate void EnemyEvent();

        private event EnemyEvent OnChangeState;


        // private static readonly int Idle = Animator.StringToHash("Idle");
        // private static readonly int Attack = Animator.StringToHash("Attack");
        // private static readonly int Walk = Animator.StringToHash("Walk");
        // private static readonly int Damage = Animator.StringToHash("Damage");
        // private static readonly int Death = Animator.StringToHash("Death");
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
            OnChangeState += ChangeState;
            GetBlendMotionsCount();
            StartCoroutine(CheckDistances());
        }

        private void OnEnable()
        {
            if (!init)
            {
                init = !init;
                return;
            }

            OnChangeState += ChangeState;
            StartCoroutine(CheckDistances());
        }

        private IEnumerator CheckDistances()
        {
            while (true)
            {
                var distance = (transform.position - player.position).magnitude;
                if (distance < enemyRaw.AttackDistance || enemyState.CurrentState == EnemyState.State.Attack &&
                    distance < enemyRaw.EndAttackDistance)
                {
                    enemyState.CurrentState = EnemyState.State.Attack;
                }
                else if (distance <= enemyRaw.ChasingDistance || enemyState.CurrentState == EnemyState.State.Chasing &&
                    distance < enemyRaw.EndChasingDistance)
                {
                    enemyState.CurrentState = EnemyState.State.Chasing;
                }
                else
                {
                    enemyState.CurrentState = EnemyState.State.Patrol;
                }

                if (enemyState.IsChanged)
                    OnChangeState?.Invoke();
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator EnemyAttack()
        {
            agent.ResetPath();

            if (enemySo.TypeEnemy == EnemySO.EnemyType.Ranged)
            {
                Coroutine lookAtCoroutine = null;
                while (enemyState.CurrentState == EnemyState.State.Attack)
                {
                    ResetAllAnimParameters();
                    SetAnimation("Attack");
                    if (!(lookAtCoroutine is  null))
                        StopCoroutine(lookAtCoroutine);
                    lookAtCoroutine = StartCoroutine(LookAtPlayer(player));
                    
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out var hit,
                        layerMask))
                    {
                        line.SetPosition(0, transform.position);
                        line.SetPosition(1, hit.point);

                        if (hit.transform.TryGetComponent<PlayerHealth>(out var plHealth))
                        {
                            plHealth.playerHealthDecrease.Invoke(enemyRaw.Damage);
                        }
                    }
                    else
                    {
                        line.SetPosition(0, transform.position);
                        line.SetPosition(1, transform.TransformDirection(Vector3.forward));
                    }

                    yield return new WaitForSeconds(EnemyAnimator.GetCurrentAnimatorStateInfo(0).length);
                }
            }

            if (enemySo.TypeEnemy == EnemySO.EnemyType.Melee)
            {
                while (enemyState.CurrentState == EnemyState.State.Attack)
                {
                    ResetAllAnimParameters();
                    SetAnimation("Attack");
                    OnTriggerEnter(playerCollider);
                    yield return new WaitForSeconds(EnemyAnimator.GetCurrentAnimatorStateInfo(0).length);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerHealth>(out var player))
            {
                player.playerHealthDecrease.Invoke(enemySo.Damage);
            }
        }

        private IEnumerator EnemyChasing()
        {
            ResetAllAnimParameters();
            SetAnimation("Walk");
            while (enemyState.CurrentState == EnemyState.State.Chasing)
            {
                LookAt(player);
                agent.SetDestination(player.position);
                yield return null;
            }
        }

        private IEnumerator EnemyPatrol()
        {
            while (enemyState.CurrentState == EnemyState.State.Patrol)
            {
                var isPatrol = Rnd(1, 3) == 1;
                if (isPatrol)
                {
                    var startPos = transform.position;
                    var numberOfMoves = Rnd(1, 10);
                    while (numberOfMoves > 0)
                    {
                        ResetAllAnimParameters();
                        SetAnimation("Walk");
                        var dest = startPos + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                        agent.SetDestination(dest);
                        var time = (dest - transform.position).magnitude / agent.speed;
                        yield return new WaitForSeconds(time);
                        numberOfMoves--;
                    }
                }
                else
                {
                    agent.ResetPath();
                    ResetAllAnimParameters();
                    SetAnimation("Idle");
                    yield return new WaitForSeconds(enemyChillTime);
                }
            }
        }

        private void ChangeState()
        {
            switch (enemyState.CurrentState)
            {
                case EnemyState.State.Attack:
                    StartCoroutine(EnemyAttack());
                    break;
                case EnemyState.State.Chasing:
                    StartCoroutine(EnemyChasing());
                    break;
                case EnemyState.State.Patrol:
                    StartCoroutine(EnemyPatrol());
                    break;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            OnChangeState -= ChangeState;
        }

        private void GetBlendMotionsCount()
        {
            var rootStateMachine = animatorController.layers[0].stateMachine;
            for (var i = 0; i < rootStateMachine.states.Length; i++)
            {
                Debug.Log(rootStateMachine.states[i].state.name);
                var blendTree = rootStateMachine.states[i].state.motion as BlendTree;
                if (blendTree is null) continue;
                switch (rootStateMachine.states[i].state.name)
                {
                    case "Attack":
                        animationDict.Add("Attack", blendTree.children.Length);
                        break;
                    case "Idle":
                        animationDict.Add("Idle", blendTree.children.Length);
                        break;
                    case "Damage":
                        animationDict.Add("Damage", blendTree.children.Length);
                        break;
                    case "Death":
                        animationDict.Add("Death", blendTree.children.Length);
                        break;
                    case "Walk":
                        animationDict.Add("Walk", blendTree.children.Length);
                        break;
                }
            }
        }

        private void SetAnimation(string animationName)
        {
            var rnd = animationDict[animationName] != 1
                ? Rnd(0, animationDict[animationName]) / (animationDict[animationName] - 1f)
                : 1f;
            EnemyAnimator.SetFloat(animationName, rnd);
        }

        private void OffAnimation(string animationName)
        {
            EnemyAnimator.SetFloat(animationName, OFF_ANIMATION_VALUE);
        }

        internal IEnumerator SetAnimationDamage()
        {
            SetAnimation("Damage");
            yield return new WaitForSeconds(EnemyAnimator.GetCurrentAnimatorStateInfo(0).length);
            OffAnimation("Damage");
        }

        private void ResetAllAnimParameters()
        {
            animationDict.Keys.ToList().ForEach(animName => enemyAnimator.SetFloat(animName, OFF_ANIMATION_VALUE));
        }

        private bool LookAt(Transform target)
        {
            var direction = (target.position - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            var rotation = (agent.angularSpeed * Time.deltaTime) / Quaternion.Angle(transform.rotation, lookRotation);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotation);
            return rotation >= 1;
        }

        private IEnumerator LookAtPlayer(Transform target)
        {
            while (!LookAt(target))
            {
                yield return null;
            }
        }

        private int Rnd(int min, int max) => Random.Range(min, max);
    }

    public class EnemyState
    {
        private State currState;

        public EnemyState(State prev, State curr)
        {
            /*PrevState = prev;*/
            /*currState = curr;*/
        }

        public State PrevState { get; set; }

        public State CurrentState
        {
            get => currState;
            set
            {
                PrevState = currState;
                currState = value;
            }
        }

        public bool IsChanged => PrevState != CurrentState;

        public enum State
        {
            None,
            Patrol,
            Chasing,
            Attack
        }
    }
}