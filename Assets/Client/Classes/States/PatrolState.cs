using System.Collections;
using Client.Scripts.EnemyScripts;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Classes.States
{
    public class PatrolState : IState
    {
        private readonly Enemy owner;
        private readonly NavMeshAgent agent;
        private readonly float ownerChillTime;

        public PatrolState(Enemy owner, NavMeshAgent agent, float ownerChillTime)
        {
            this.owner = owner;
            this.agent = agent;
            this.ownerChillTime = ownerChillTime;
        }

        public void Enter()
        {
            owner.StartCoroutine(owner.StateWrapper(Execute()));
        }

        public IEnumerator Execute()
        {
            while (owner.StateMachine.CurrentState is PatrolState)
            {
                var isPatrol = Random.Range(1, 3) == 1;
                if (isPatrol)
                {
                    var startPos = owner.transform.position;
                    var numberOfMoves = Random.Range(1, 10);
                    while (numberOfMoves > 0)
                    {
                        EnemyAnimations.ResetAllAnimParameters(owner);
                        EnemyAnimations.SetAnimation(owner, "Walk");
                        var dest = startPos + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                        agent.SetDestination(dest);
                        var time = (dest - owner.transform.position).magnitude / agent.speed;
                        yield return new WaitForSeconds(time);
                        numberOfMoves--;
                    }
                }
                else
                {
                    agent.ResetPath();
                    EnemyAnimations.ResetAllAnimParameters(owner);
                    EnemyAnimations.SetAnimation(owner, "Idle");
                    yield return new WaitForSeconds(ownerChillTime);
                }
            }
        }

        public void Exit()
        {
            owner.StopCoroutine(owner.StateWrapper(Execute()));
        }
    }
}