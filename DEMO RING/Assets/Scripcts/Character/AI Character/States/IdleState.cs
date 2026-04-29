using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "A.I/AI States/Idle State")]
public class IdleState : AIState
{
    public override AIState Tick(AICharacterManager aiCharacterManager)
    {
        if(aiCharacterManager.characterCombatManager.currentTarget != null)
        {
            return SwitchState(aiCharacterManager, aiCharacterManager.pursueTarget);
        }
        else
        {
            //Debug.Log("We are searching for a target");
            aiCharacterManager.aiCharacterCombatManager.FindATargetViaLineOfSight(aiCharacterManager);

            return this;
        }
    }
}
