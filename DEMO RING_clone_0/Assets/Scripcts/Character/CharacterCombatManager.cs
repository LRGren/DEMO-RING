using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterCombatManager : NetworkBehaviour
{
    protected CharacterManager characterManager;

    [Header("Attack Target")]
    public CharacterManager currentTarget;

    [Header("Last Attack Animation")]
    public string lastAttackAnimation = "";

    [Header("Attack Type")]
    public AttackType currentAttackType;

    [Header("Lock On Transform")]
    public Transform lockOnTransform;

    [Header("Attack Flags")]
    public bool canPerformRollingAttack = false;
    public bool canPerformBackstepAttack = false;

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public virtual void SetTarget(CharacterManager newTarget)
    {
        if (characterManager.IsOwner)
        {
            if (newTarget != null)
            {
                currentTarget = newTarget;

                //通知NETWORK，让其他玩家知道这个角色锁定了一个目标
                characterManager.characterNetworkManager.currentTargetNetworkObjectID.Value = newTarget.gameObject.GetComponent<NetworkObject>().NetworkObjectId;

            }
            else
            {
                currentTarget = null;
            }
        }
    }

    public void EnableIsInvulnerable()
    {
        if (characterManager.IsOwner)
            characterManager.characterNetworkManager.isInvulnerable.Value = true;
    }

    public void DisableIsInvulnerable()
    {
        if (characterManager.IsOwner)
            characterManager.characterNetworkManager.isInvulnerable.Value = false;
    }

    public void EnableCanPerformRollingAttack()
    {
        canPerformRollingAttack = true;
    }

    public void DisableCanPerformRollingAttack()
    {
        canPerformRollingAttack = false;
    }

    public void EnableCanPerformBackstepAttack()
    {
        canPerformBackstepAttack = true;
    }

    public void DisableCanPerformBackstepAttack()
    {
        canPerformBackstepAttack = false;
    }

    public virtual void EnableDoCombo()
    {
    }

    public virtual void DisableDoCombo()
    {
    }
}
