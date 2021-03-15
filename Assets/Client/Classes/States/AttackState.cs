using System.Collections;
using Client.Scripts.EnemyScripts;
using Client.Scripts.PlayerScripts;
using Client.Scripts.ScriptSO;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Classes.States
{
    public class AttackState : IState
    {
        private readonly Enemy owner;
        private readonly Transform player;
        private readonly NavMeshAgent agent;
        private readonly Collider playerCollider;
        private readonly Animator ownerAnimator;
        private readonly LayerMask layerMask;
        public AttackState(Enemy owner, Transform player, NavMeshAgent agent, Collider playerCollider, Animator ownerAnimator, LayerMask layerMask)
        {
            this.owner = owner;
            this.player = player;
            this.agent = agent;
            this.playerCollider = playerCollider;
            this.ownerAnimator = ownerAnimator;
            this.layerMask = layerMask;
        }

        public void Enter()
        {
            owner.StartCoroutine(owner.StateWrapper(Execute()));
        }

        public IEnumerator Execute()
        {
            agent.ResetPath();

            if (owner.EnemySo.TypeEnemy == EnemySO.EnemyType.Ranged)
            {
                Coroutine lookAtCoroutine = null;
                while (owner.StateMachine.CurrentState is AttackState)
                {
                    EnemyAnimations.ResetAllAnimParameters(owner);
                    EnemyAnimations.SetAnimation(owner, "Attack");
                    if (!(lookAtCoroutine is null))
                        owner.StopCoroutine(lookAtCoroutine);
                    lookAtCoroutine = owner.StartCoroutine(LookAtPlayer(player));

                    if (Physics.Raycast(owner.transform.position, owner.transform.TransformDirection(Vector3.forward), out var hit,
                        layerMask))
                    {
                        owner.Line.SetPosition(0, owner.transform.position);
                        owner.Line.SetPosition(1, hit.point);

                        if (hit.transform.TryGetComponent<PlayerHealth>(out var plHealth))
                        {
                            plHealth.playerHealthDecrease.Invoke(owner.EnemyRaw.Damage);
                        }
                    }
                    else
                    {
                        owner.Line.SetPosition(0, owner.transform.position);
                        owner.Line.SetPosition(1, owner.transform.TransformDirection(Vector3.forward));
                    }

                    yield return new WaitForSeconds(ownerAnimator.GetCurrentAnimatorStateInfo(0).length);
                }
            }

            if (owner.EnemySo.TypeEnemy == EnemySO.EnemyType.Melee)
            {
                while (owner.StateMachine.CurrentState is AttackState)
                {
                    EnemyAnimations.ResetAllAnimParameters(owner);
                    EnemyAnimations.SetAnimation(owner, "Attack");
                    OnTriggerEnter(playerCollider);
                    yield return new WaitForSeconds(ownerAnimator.GetCurrentAnimatorStateInfo(0).length);
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                health.playerHealthDecrease.Invoke(owner.EnemySo.Damage);
            }
        }

        private bool LookAt(Transform target)
        {
            var direction = (target.position - owner.transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            var ownerRotation = owner.transform.rotation;
            var rotation = (agent.angularSpeed * Time.deltaTime) / Quaternion.Angle(ownerRotation, lookRotation);
            ownerRotation = Quaternion.Slerp(ownerRotation, lookRotation, rotation);
            owner.transform.rotation = ownerRotation;
            return rotation >= 1;
        }

        private IEnumerator LookAtPlayer(Transform target)
        {
            while (!LookAt(target))
            {
                yield return null;
            }
        }
        
        public void Exit()
        {
            owner.StopCoroutine(owner.StateWrapper(Execute()));
        }
    
    
    }
}
