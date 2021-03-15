using System.Collections;
using Client.Scripts.EnemyScripts;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Classes.States
{
    public class ChasingState : IState
    {
        private readonly Enemy owner;
        private readonly Transform player;
        private readonly NavMeshAgent agent;
        
        public ChasingState(Enemy owner, Transform player, NavMeshAgent agent)
        {
            this.owner = owner;
            this.player = player;
            this.agent = agent;
        }
    
        public void Enter()
        {
            owner.StartCoroutine(owner.StateWrapper(Execute()));
        }

        public IEnumerator Execute()
        {
            EnemyAnimations.ResetAllAnimParameters(owner);
            EnemyAnimations.SetAnimation(owner, "Walk");
            while (owner.StateMachine.CurrentState is ChasingState)
            {
                LookAt(player);
                agent.SetDestination(player.position);
                yield return null;
            }
        }

        private void LookAt(Transform target)
        {
            var direction = (target.position - owner.transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            var ownerRotation = owner.transform.rotation;
            var rotation = (agent.angularSpeed * Time.deltaTime) / Quaternion.Angle(ownerRotation, lookRotation);
            ownerRotation = Quaternion.Slerp(ownerRotation, lookRotation, rotation);
            owner.transform.rotation = ownerRotation;
        }
        
        public void Exit()
        {
            owner.StopCoroutine(owner.StateWrapper(Execute()));
        }
        
    }
}
