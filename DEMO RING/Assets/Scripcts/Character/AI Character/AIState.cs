using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState : ScriptableObject
{
    public virtual AIState Tick(AICharacterManager aiCharacterManager)
    {
        return this;
    }

    protected virtual AIState SwitchState(AICharacterManager aiCharacterManager, AIState nextState)
    {
        ResetStateFlags(aiCharacterManager);
        return nextState;
    }

    protected virtual void ResetStateFlags(AICharacterManager aiCharacterManager)
    {
        // Reset all flags or states in the AICharacterManager
        // For example, you might want to reset attack flags, movement flags, etc.
    }

}
