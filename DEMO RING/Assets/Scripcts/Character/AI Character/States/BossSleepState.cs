using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "A.I/AI States/Boss/Boss Sleep State")]
public class BossSleepState : AIState
{
    public override AIState Tick(AICharacterManager aiCharacterManager)
    {
        return base.Tick(aiCharacterManager);
    }
}
