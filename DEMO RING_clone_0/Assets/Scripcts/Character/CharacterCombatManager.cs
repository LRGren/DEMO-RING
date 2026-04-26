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
}
