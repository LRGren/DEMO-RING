using UnityEngine;

public class ComboValidationAnimator : PlayerAnimatorManager
{
    public PlayerManager testPlayer;
    public int attackCalls;

    public override void PlayerTargetAttackActionAnimation(WeaponItem weapon, AttackType attackType,
        string targetAnimation, bool isPerformingAction, bool applyRootMotion = true,
        bool canRotate = false, bool canMove = false)
    {
        attackCalls++;
        testPlayer.playerCombatManager.lastAttackAnimation = targetAnimation;
        testPlayer.playerCombatManager.currentAttackType = attackType;
        testPlayer.isPerformingAction = isPerformingAction;
    }
}
