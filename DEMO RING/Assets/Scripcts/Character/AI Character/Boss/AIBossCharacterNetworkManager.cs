using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBossCharacterNetworkManager : AIChracterNetworkManager
{
    AIBossCharacterManager aiBossCharacter;

    protected override void Awake()
    {
        base.Awake();
        aiBossCharacter = GetComponent<AIBossCharacterManager>();
    }

    public override void CheckHP(int oldValue, int newValue)
    {
        base.CheckHP(oldValue, newValue);

        if (aiBossCharacter.IsOwner)
        {
            if (currentHealth.Value <= 0)
                return;

            float healthNeedForPhaseShift = maxHealth.Value * (aiBossCharacter.phaseShiftHealthThresholdPercent / 100f);

            if (currentHealth.Value <= healthNeedForPhaseShift)
            {
                aiBossCharacter.PhaseChange();
            }
        }
    }
}
