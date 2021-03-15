using System.Collections;
using System.Linq;
using Client.Scripts.EnemyScripts;
using UnityEditor.Animations;
using UnityEngine;

namespace Client.Classes
{
    public static class EnemyAnimations
    {
        private const float OFF_ANIMATION_VALUE = -1f;

        public static void GetBlendMotionsCount(Enemy enemy, AnimatorController animatorController )
        {
            var rootStateMachine = animatorController.layers[0].stateMachine;
            for (var i = 0; i < rootStateMachine.states.Length; i++)
            {
                var blendTree = rootStateMachine.states[i].state.motion as BlendTree;
                if (blendTree is null) continue;
                switch (rootStateMachine.states[i].state.name)
                {
                    case "Attack":
                        enemy.AnimationDict.Add("Attack", blendTree.children.Length);
                        break;
                    case "Idle":
                        enemy.AnimationDict.Add("Idle", blendTree.children.Length);
                        break;
                    case "Damage":
                        enemy.AnimationDict.Add("Damage", blendTree.children.Length);
                        break;
                    case "Death":
                        enemy.AnimationDict.Add("Death", blendTree.children.Length);
                        break;
                    case "Walk":
                        enemy.AnimationDict.Add("Walk", blendTree.children.Length);
                        break;
                }
            }
        }
    
        public static void SetAnimation(Enemy enemy, string animationName)
        {
            var rnd = enemy.AnimationDict[animationName] != 1
                ? Random.Range(0, enemy.AnimationDict[animationName]) / (enemy.AnimationDict[animationName] - 1f)
                : 1f;
            enemy.EnemyAnimator.SetFloat(animationName, rnd);
        }

        public static void OffAnimation(Enemy enemy, string animationName)
        {
            enemy.EnemyAnimator.SetFloat(animationName, OFF_ANIMATION_VALUE);
        }
    
        public static void ResetAllAnimParameters(Enemy enemy)
        {
            enemy.AnimationDict.Keys.ToList().ForEach(animName => enemy.EnemyAnimator.SetFloat(animName, OFF_ANIMATION_VALUE));
        }
    
        public static IEnumerator SetAnimationDamage(Enemy enemy)
        {
            SetAnimation(enemy, "Damage");
            yield return new WaitForSeconds(enemy.EnemyAnimator.GetCurrentAnimatorStateInfo(0).length);
            OffAnimation(enemy, "Damage");
        }
    }
}
