using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICharacterLocomotionManager : CharacterLocomotionManager
{
    public void RotateTowardsAgent(AICharacterManager aiCharacterManager)
    {
        if (aiCharacterManager.aiCharacterNetworkManager.isMoving.Value)
            aiCharacterManager.transform.rotation = aiCharacterManager.navMeshAgent.transform.rotation;
    }
}
