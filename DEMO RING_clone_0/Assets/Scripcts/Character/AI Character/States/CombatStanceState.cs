using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "A.I/AI States/Combat Stance State")]
public class CombatStanceState : AIState
{
    //  根据攻击距离和面向角度，选择一种攻击方式
    //  在攻击之前，进行（格挡，翻滚，围绕）等COMBAT动作的选择
    //  如果在攻击范围外，PURSUE状态
    //  如果目标已经死亡，IDLE状态

    [Header("Attacks")]
    public List<AICharacterAttackAction> aiCharacterAttacks; // List of attack actions available to the AI character
    private List<AICharacterAttackAction> potentialAttacks; // List of potential attack actions based on the current situation
    private AICharacterAttackAction chosenAttack; // The current attack action being performed by the AI character
    private AICharacterAttackAction previousAttack;
    protected bool hasAttacked = false; // Flag to indicate if the AI character has performed an attack

    [Header("Combo")]
    [SerializeField] protected bool canPerformCombo = false;
    [SerializeField, Range(0f, 100)] protected int chanceToPerformCombo = 25; // Chance to perform a combo attack (0-100)
    protected bool hasRolledForComboChance = false;    //本次状态下是否已经roll过了

    [Header("Engagement Distance")]
    [SerializeField] protected float maximumEngagementDistance = 5;     //互动距离

    public override AIState Tick(AICharacterManager aiCharacterManager)
    {
        if(aiCharacterManager.isPerformingAction)
            return this;

        if (!aiCharacterManager.aiCharacterNetworkManager.isMoving.Value)
        {
            if(aiCharacterManager.aiCharacterCombatManager.viewableAngle < -30 || aiCharacterManager.aiCharacterCombatManager.viewableAngle > 30)
            {
                aiCharacterManager.aiCharacterCombatManager.PivotTowardsTarget(aiCharacterManager);
            }
        }

        //  转向目标
        aiCharacterManager.aiCharacterCombatManager.RotateTowardsAgent(aiCharacterManager);

        if(aiCharacterManager.aiCharacterCombatManager.currentTarget == null)
            return SwitchState(aiCharacterManager, aiCharacterManager.idle);

        if (!hasAttacked)
        {
            GetNewAttack(aiCharacterManager);
        }
        else
        {
            aiCharacterManager.attack.currentAttack = chosenAttack;
            return SwitchState(aiCharacterManager, aiCharacterManager.attack);
        }

        if(aiCharacterManager.aiCharacterCombatManager.distanceFromTarget > maximumEngagementDistance)
            return SwitchState(aiCharacterManager, aiCharacterManager.pursueTarget);

        NavMeshPath navMeshPath = new NavMeshPath();
        aiCharacterManager.navMeshAgent.CalculatePath(aiCharacterManager.aiCharacterCombatManager.currentTarget.transform.position, navMeshPath);
        aiCharacterManager.navMeshAgent.SetPath(navMeshPath);

        return this;
    }

    protected virtual void GetNewAttack(AICharacterManager aiCharacter) {

        potentialAttacks = new List<AICharacterAttackAction>();

        foreach (var potentialAttack in aiCharacterAttacks)
        {
            if (potentialAttack.minimumAttackAngle > aiCharacter.aiCharacterCombatManager.viewableAngle)
                continue;

            if(potentialAttack.maximumAttackAngle < aiCharacter.aiCharacterCombatManager.viewableAngle)
                continue;

            if(potentialAttack.minimumAttackDistance > aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                continue;

            if(potentialAttack.maximumAttackDistance < aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                continue;

            potentialAttacks.Add(potentialAttack);
        }

        if (potentialAttacks.Count <= 0)
            return;

        int totalWeigth = 0;
        foreach (var potentialAttack in potentialAttacks)
            totalWeigth += potentialAttack.attackWeigth;

        int randomValue = Random.Range(1, totalWeigth+1);
        int processWeight = 0;
        foreach (var potentialAttack in potentialAttacks)
        {
            processWeight += potentialAttack.attackWeigth;

            if (processWeight >= randomValue)
            {
                chosenAttack = potentialAttack;
                previousAttack = chosenAttack;
                hasAttacked = true;
                return;
            }
        }

    }

    protected virtual bool RollForOutcomeChance(int outcomeChance)
    {
        bool outcomeWillBePerformed = false;

        int randomPercentage = Random.Range(0, 100);

        if (randomPercentage <= outcomeChance)
            outcomeWillBePerformed = true;

        return outcomeWillBePerformed;
    }

    protected override void ResetStateFlags(AICharacterManager aiCharacterManager)
    {
        base.ResetStateFlags(aiCharacterManager);
        hasAttacked = false;
        hasRolledForComboChance = false;
    }

}
