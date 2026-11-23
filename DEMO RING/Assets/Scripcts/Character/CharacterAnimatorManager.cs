using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterAnimatorManager : MonoBehaviour
{
    private CharacterManager character;

    private int horizontal;
    private int vertical;

    [Header("Flags")]
    public bool applyRootMotion = false;


    [Header("Damage Animation")]
    //Medium Hit Animations
    public string hit_Forward_Medium_01 = "Hit_Forward_Medium_01";
    public string hit_Back_Medium_01 = "Hit_Back_Medium_01";
    public string hit_Left_Medium_01 = "Hit_Left_Medium_01";
    public string hit_Right_Medium_01 = "Hit_Right_Medium_01";

    public bool hasPingHitAnimation = true;
    //Ping Hit Animations
    public string hit_Forward_Ping_01 = "Hit_Forward_Ping_01";
    public string hit_Back_Ping_01 = "Hit_Back_Ping_01";
    public string hit_Left_Ping_01 = "Hit_Left_Ping_01";
    public string hit_Right_Ping_01 = "Hit_Right_Ping_01";

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();

        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorMovementParameters(float horizontalValue, float verticalValue, bool isSprinting)
    {
        float snappedHorizontal;
        float snappedVertical;

        if (horizontalValue > 0 && horizontalValue < 0.55f)
            snappedHorizontal = 0.5f;
        else if (horizontalValue > 0.55f)
            snappedHorizontal = 1f;
        else if (horizontalValue < 0 && horizontalValue > -0.55f)
            snappedHorizontal = -0.5f;
        else if (horizontalValue < -0.55f)
            snappedHorizontal = -1f;
        else
            snappedHorizontal = 0;

        if (verticalValue > 0 && verticalValue < 0.55f)
            snappedVertical = 0.5f;
        else if (verticalValue > 0.55f)
            snappedVertical = 1f;
        else if (verticalValue < 0 && verticalValue > -0.55f)
            snappedVertical = -0.5f;
        else if (verticalValue < -0.55f)
            snappedVertical = -1f;
        else
            snappedVertical = 0;

        if (isSprinting) snappedVertical = 2f;

        character.animator.SetFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
        character.animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }

    public void UpdateAnimatorTriggerParameters(string parameterName)
    {
        character.animator.SetTrigger(parameterName);
    }

    public virtual void PlayerTargetActionAnimation(string targetAnimation, bool isPerformingAction, bool applyRootMotion = true, bool canRotate = false, bool canMove = false, bool canRun = true, bool canRoll = false)
    {
        //Debug.Log("Playing Target Action Animation: " + targetAnimation);

        character.characterAnimatorManager.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(targetAnimation, 0.2f);
        character.isPerformingAction = isPerformingAction;
        character.characterLocomotionManager.canRotate = canRotate;
        character.characterLocomotionManager.canMove = canMove;
        character.characterLocomotionManager.canRun = canRun;
        character.characterLocomotionManager.canRoll = canRoll;

        //只有拥有者（玩家）或服务器（AI）才能发送需要所有权的 ServerRpc
        if (!character.IsOwner)
            return;

        character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }

    public virtual void PlayerTargetActionAnimationInstantly(string targetAnimation, bool isPerformingAction, bool applyRootMotion = true, bool canRotate = false, bool canMove = false, bool canRun = true, bool canRoll = false)
    {
        //Debug.Log("Playing Target Action Animation: " + targetAnimation);

        character.characterAnimatorManager.applyRootMotion = applyRootMotion;
        character.animator.Play(targetAnimation);
        character.isPerformingAction = isPerformingAction;
        character.characterLocomotionManager.canRotate = canRotate;
        character.characterLocomotionManager.canMove = canMove;
        character.characterLocomotionManager.canRun = canRun;
        character.characterLocomotionManager.canRoll = canRoll;

        //只有拥有者（玩家）或服务器（AI）才能发送需要所有权的 ServerRpc
        if (!character.IsOwner)
            return;

        character.characterNetworkManager.NotifyTheServerOfActionAnimationInstantlyServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }

    public virtual void PlayerTargetAttackActionAnimation(WeaponItem weapon, AttackType attackType, string targetAnimation, bool isPerformingAction, bool applyRootMotion = true, bool canRotate = false, bool canMove = false, float fixedTransitionDuration = -1f)
    {
        //COMBOS
        //确定攻击类型
        //根据武器更新当前的动作模组
        //确定是否是成对的
        //标记 “Attacking”

        character.characterCombatManager.currentAttackType = attackType;
        character.characterCombatManager.lastAttackAnimation = targetAnimation;

        UpdateAnimatorController(weapon.weaponAnimator);

        character.characterAnimatorManager.applyRootMotion = applyRootMotion;
        CrossFadeAttack(targetAnimation, fixedTransitionDuration);
        character.isPerformingAction = isPerformingAction;
        character.characterLocomotionManager.canRotate = canRotate;
        character.characterLocomotionManager.canMove = canMove;
        character.characterNetworkManager.isAttacking.Value = true;

        //只有拥有者（玩家）或服务器（AI）才能发送需要所有权的 ServerRpc
        if (!character.IsOwner)
            return;

        character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion, fixedTransitionDuration, weapon.itemID);
    }

    public void CrossFadeAttack(string state, float fixedTransitionDuration)
    {
        if (fixedTransitionDuration >= 0f)
            character.animator.CrossFadeInFixedTime(state, fixedTransitionDuration, -1, 0f);
        else
            character.animator.CrossFade(state, 0.2f);
    }

    public void UpdateAnimatorController(AnimatorOverrideController weaponController)
    {
        if (character.animator.runtimeAnimatorController != weaponController)
            character.animator.runtimeAnimatorController = weaponController;
    }

    //Animation Event Calls

}
