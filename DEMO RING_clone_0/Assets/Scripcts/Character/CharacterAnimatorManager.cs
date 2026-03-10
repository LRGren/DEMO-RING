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

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
        
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }
    
    public void UpdateAnimatorMovementParameters(float horizontalValue, float verticalValue,bool isSprinting)
    {
        float horizontalAmount = horizontalValue;
        float verticalAmount = verticalValue;

        if (isSprinting) verticalAmount = 2f;
        
        character.animator.SetFloat(horizontal, horizontalAmount,0.1f, Time.deltaTime);
        character.animator.SetFloat(vertical, verticalAmount, 0.1f, Time.deltaTime);
    }
    
    public virtual void PlayerTargetActionAnimation(string targetAnimation,bool isPerformingAction,bool applyRootMotion = true,bool canRotate = false,bool canMove = false)
    {
        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(targetAnimation, 0.2f);
        character.isPerformingAction = isPerformingAction;
        character.canRotate = canRotate;
        character.canMove = canMove;

        character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }

    public virtual void PlayerTargetAttackActionAnimation(string targetAnimation, bool isPerformingAction, bool applyRootMotion = true, bool canRotate = false, bool canMove = false)
    {
        //COMBOS
        //确定攻击类型
        //根据武器更新当前的动作模组
        //确定是否是成对的
        //标记 “Attacking”

        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(targetAnimation, 0.2f);
        character.isPerformingAction = isPerformingAction;
        character.canRotate = canRotate;
        character.canMove = canMove;

        character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(
            NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
    }
}
