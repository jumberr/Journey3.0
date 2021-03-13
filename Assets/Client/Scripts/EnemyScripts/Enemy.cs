using System.Collections;
using System.Collections.Generic;
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
        [Header("Enemy characteristics")] 
        [SerializeField] private EnemySO enemySo;
        [SerializeField] private NavMeshAgent agent;
        
        [SerializeField] private LineRenderer line;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private AnimatorController animatorController;
        [SerializeField] private Animator enemyAnimator;
        [ReadOnlyAttribute] [SerializeField] private float enemyChillTime;

        private Transform player;
        private Collider playerCollider;

        private bool init;

        private Dictionary<string, int> animationDict = new Dictionary<string, int>();

        private EnemyRaw enemyRaw;
        private EnemyState enemyState = new EnemyState(EnemyState.State.Patrol, EnemyState.State.Patrol);

        private delegate void EnemyEvent();

        private event EnemyEvent OnChangeState;
        
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
            OnChangeState += ChangeState;
            EnemyAnimations.GetBlendMotionsCount(this, animatorController);
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
                    EnemyAnimations.ResetAllAnimParameters(this);
                    EnemyAnimations.SetAnimation(this, "Attack");
                    if (!(lookAtCoroutine is null))
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
                    EnemyAnimations.ResetAllAnimParameters(this);
                    EnemyAnimations.SetAnimation(this, "Attack");
                    OnTriggerEnter(playerCollider);
                    yield return new WaitForSeconds(EnemyAnimator.GetCurrentAnimatorStateInfo(0).length);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                health.playerHealthDecrease.Invoke(enemySo.Damage);
            }
        }

        private IEnumerator EnemyChasing()
        {
            EnemyAnimations.ResetAllAnimParameters(this);
            EnemyAnimations.SetAnimation(this, "Walk");
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
                        EnemyAnimations.ResetAllAnimParameters(this);
                        EnemyAnimations.SetAnimation(this, "Walk");
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
                    EnemyAnimations.ResetAllAnimParameters(this);
                    EnemyAnimations.SetAnimation(this, "Idle");
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