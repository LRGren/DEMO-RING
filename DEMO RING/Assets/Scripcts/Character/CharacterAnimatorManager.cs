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

    [Header("Damage Animation")]
    public string hit_Forward_Medium_01 = "Hit_Forward_Medium_01";
    public string hit_Back_Medium_01 = "Hit_Back_Medium_01";
    public string hit_Left_Medium_01 = "Hit_Left_Medium_01";
    public string hit_Right_Medium_01 = "Hit_Right_Medium_01";

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
        
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }
    
    public void UpdateAnimatorMovementParameters(float horizontalValue, float verticalValue,bool isSprinting)
    {
        float snappedHorizontal;
        float snappedVertical;

        if(horizontalValue > 0 && horizontalValue < 0.55f)
            snappedHorizontal = 0.5f;
        else if(horizontalValue > 0.55f)
            snappedHorizontal = 1f;
        else if(horizontalValue < 0 && horizontalValue > -0.55f)
            snappedHorizontal = -0.5f;
        else if(horizontalValue < -0.55f)
            snappedHorizontal = -1f;
        else
            snappedHorizontal = 0;

        if(verticalValue > 0 && verticalValue < 0.55f)
            snappedVertical = 0.5f;
        else if(verticalValue > 0.55f)
            snappedVertical = 1f;
        else if(verticalValue < 0 && verticalValue > -0.55f)
            snappedVertical = -0.5f;
        else if(verticalValue < -0.55f)
            snappedVertical = -1f;
        else
            snappedVertical = 0;

        if (isSprinting) snappedVertical = 2f;
        
        character.animator.SetFloat(horizontal, snappedHorizontal,0.1f, Time.deltaTime);
        character.animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }
    
    public virtual void PlayerTargetActionAnimation(string targetAnimation,bool isPerformingAction,bool applyRootMotion = true,bool canRotate = false,bool canMove = false)
    {
        //Debug.Log("Playing Target Action Animation: " + targetAnimation);

        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(targetAnimation, 0.2f);
        character.isPerformingAction = isPerformingAction;
        character.canRotate = canRotate;
        character.canMove = canMove;

        character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }

    public virtual void PlayerTargetAttackActionAnimation(AttackType attackType, string targetAnimation, bool isPerformingAction, bool applyRootMotion = true, bool canRotate = false, bool canMove = false)
    {
        //COMBOS
        //确定攻击类型
        //根据武器更新当前的动作模组
        //确定是否是成对的
        //标记 “Attacking”

        character.characterCombatManager.currentAttackType = attackType;
        character.characterCombatManager.lastAttackAnimation = targetAnimation;
        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(targetAnimation, 0.2f);
        character.isPerformingAction = isPerformingAction;
        character.canRotate = canRotate;
        character.canMove = canMove;

        character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }


    //Animation Event Calls
    public virtual void EnableDoCombo()
    {
    }

    public virtual void DisableDoCombo()
    {
    }

}
