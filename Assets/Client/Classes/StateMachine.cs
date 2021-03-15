using System.Collections;
using Client.Classes.States;
using Client.Scripts.EnemyScripts;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Classes
{
    public class StateMachine
    {
        private readonly Enemy owner;
        private IState currentState;
        private readonly PatrolState patrolState;
        private readonly ChasingState chasingState;
        private readonly AttackState attackState;
        private readonly Transform player;
        private readonly EnemyRaw enemyRaw;
        public IState CurrentState => currentState;

        public StateMachine(Enemy owner, Transform player, EnemyRaw enemyRaw, NavMeshAgent agent, Collider playerCollider, Animator animator, LayerMask layerMask, float chillTime)
        {
            this.owner = owner;
            patrolState = new PatrolState(owner, agent, chillTime);
            chasingState = new ChasingState(owner, player, agent);
            attackState = new AttackState(owner, player, agent, playerCollider, animator, layerMask);
            this.player = player;
            this.enemyRaw = enemyRaw;
        }

        private void ChangeState(IState newState)
        {
            if (!(currentState is null) && currentState.Equals(newState)) return;
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        public IEnumerator CheckDistances()
        {
            while (true)
            {
                var distance = (owner.transform.position - player.position).magnitude;
                if (distance < enemyRaw.AttackDistance || CurrentState is AttackState &&
                    distance < enemyRaw.EndAttackDistance)
                {
                    ChangeState(attackState);
                }
                else if (distance <= enemyRaw.ChasingDistance || CurrentState is ChasingState &&
                    distance < enemyRaw.EndChasingDistance)
                {
                    ChangeState(chasingState);
                }
                else
                {
                    ChangeState(patrolState);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}