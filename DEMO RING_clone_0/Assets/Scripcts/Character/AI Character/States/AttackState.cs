using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "A.I/AI States/Attack State")]
public class AttackState : AIState
{
    [Header("Current Attack")]
    [HideInInspector] public AICharacterAttackAction currentAttack;
    [HideInInspector] public bool willPerformCombo = false;

    [Header("State Flags")]
    protected bool hasPerformedAttack = false;
    protected bool hasPerformedCombo = false;

    [Header("Pivot After Attack")]
    [SerializeField] protected bool pivotAfterAttack = false;

    public override AIState Tick(AICharacterManager aiCharacterManager)
    {
        if(aiCharacterManager.aiCharacterCombatManager.currentTarget == null)
            return SwitchState(aiCharacterManager, aiCharacterManager.idle);

        if(aiCharacterManager.isDead.Value)
            return SwitchState(aiCharacterManager, aiCharacterManager.idle);


        // Rotate towards the target

        // 更新Animator中的"Vertical"参数'
        aiCharacterManager.characterAnimatorManager.UpdateAnimatorMovementParameters(0, 0, false);

        if (willPerformCombo && !hasPerformedCombo)
        {
            if (currentAttack.comboAction != null)
            {

            }
        }

        if (!hasPerformedAttack)
        {
            if (aiCharacterManager.aiCharacterCombatManager.actionRecoveryTimer > 0)
                return this;

            if(aiCharacterManager.isPerformingAction)
                return this;

            PerformAttack(aiCharacterManager);

            return this;
        }

        if (pivotAfterAttack)
            aiCharacterManager.aiCharacterCombatManager.PivotTowardsTarget(aiCharacterManager);

        return SwitchState(aiCharacterManager,aiCharacterManager.combatStance);
    }

    protected void PerformAttack(AICharacterManager aiCharacter)
    {
        hasPerformedAttack = true;
        currentAttack.AttemptToPerformAction(aiCharacter);
        aiCharacter.aiCharacterCombatManager.actionRecoveryTimer = currentAttack.attackRecoveryTime;
    }

    protected override void ResetStateFlags(AICharacterManager aiCharacterManager)
    {
        base.ResetStateFlags(aiCharacterManager);

        hasPerformedAttack = false;
        hasPerformedCombo = false;
    }

}
