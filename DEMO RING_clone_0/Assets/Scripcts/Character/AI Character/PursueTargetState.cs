using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "A.I/AI States/Pursue Target State")]
public class PursueTargetState : AIState
{
    public override AIState Tick(AICharacterManager aiCharacterManager)
    {
        //检测是否正在播放动画isPerformingAction，如果是，不做任何操作
        if(aiCharacterManager.isPerformingAction)
            return this;

        //检测是否有目标，如果没有，返回IdleState
        if(aiCharacterManager.aiCharacterCombatManager.currentTarget == null)
            return aiCharacterManager.idle;

        //检测是否开启NavMeshAgent，如果没有，开启NavMeshAgent
        if(!aiCharacterManager.navMeshAgent.enabled)
            aiCharacterManager.navMeshAgent.enabled = true;

        aiCharacterManager.aiCharacterLocomotionManager.RotateTowardsAgent(aiCharacterManager);

        //检测是否在攻击范围内，如果是，返回AttackState

        //检测是否在追击范围内，如果不是（目标过远），回家

        //如果以上条件都不满足，继续追击目标
        NavMeshPath navMeshPath = new NavMeshPath();
        aiCharacterManager.navMeshAgent.CalculatePath(aiCharacterManager.aiCharacterCombatManager.currentTarget.transform.position, navMeshPath);
        aiCharacterManager.navMeshAgent.SetPath(navMeshPath);

        return this;
    }
}
