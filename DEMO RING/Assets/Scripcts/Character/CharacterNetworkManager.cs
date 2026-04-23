using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterNetworkManager : NetworkBehaviour
{
    private CharacterManager character;
    
    [Header("Position")] 
    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Vector3 networkPositionVelocity;
    public float networkPositionSmoothTime = 0.1f;
    public float networkRotationSmoothTime = 0.1f;

    [Header("Animator")]
    public NetworkVariable<float> horizontalMovement = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> verticalMovement = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> moveAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Target")]
    public NetworkVariable<ulong> currentTargetNetworkObjectID = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Flags")]
    public NetworkVariable<bool> isLockOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isChargingAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Stats")]
    public NetworkVariable<int> endurance = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> vitality = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [Header("Resources")]
    public NetworkVariable<float> currentStamina = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public virtual void CheckHP(int oldValue, int newValue)
    {
        if (!character.IsOwner)
            return;

        if (currentHealth.Value <= 0)
        {
            StartCoroutine(character.ProcessDeathEvent());
        }

        if (currentHealth.Value >= maxHealth.Value)
        {
            currentHealth.Value = maxHealth.Value;
        }
    }
    
    public void OnLockOnTargetIDChange(ulong oldID, ulong newID)
    {
        if (!IsOwner)
        {
            character.characterCombatManager.currentTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[newID].GetComponent<CharacterManager>();
        }
    }

    public void OnIsLockOnChanged(bool old, bool isLockedOn)
    {
        if (!isLockedOn)
        {
            character.characterCombatManager.currentTarget = null;
        }
    }

    public void OnIsChargingAttackChanged(bool old,bool newStatus)
    {
        character.animator.SetBool("isChargingAttack", isChargingAttack.Value);
    }

    [ServerRpc]
    public void NotifyTheServerOfActionAnimationServerRpc(ulong clientId,string animationName,bool applyRootMotion)
    {
        if (IsServer)
        {
            PlayActionAnimationForAllClientsClientRpc(clientId, animationName, applyRootMotion);
        }
    }

    [ClientRpc]
    public void PlayActionAnimationForAllClientsClientRpc(ulong clientId,string animationName,bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PerformActionAnimationFromServer(animationName, applyRootMotion);
        }
    }

    public void PerformActionAnimationFromServer(string animationName, bool applyRootMotion)
    {
        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(animationName, 0.2f);
    }

    //  Attack
    [ServerRpc]
    public void NotifyTheServerOfAttackActionAnimationServerRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (IsServer)
        {
            PlayAttackActionAnimationForAllClientsClientRpc(clientId, animationName, applyRootMotion);
        }
    }

    [ClientRpc]
    public void PlayAttackActionAnimationForAllClientsClientRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PerformAttackActionAnimationFromServer(animationName, applyRootMotion);
        }
    }

    public void PerformAttackActionAnimationFromServer(string animationName, bool applyRootMotion)
    {
        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(animationName, 0.2f);
    }

    //  Damage
    [ServerRpc(RequireOwnership = false)]
    public void NotifyTheServerOfCharacterDamageServerRpc(ulong damageCharacterID, ulong charcterCausingDamageID,
        float physicalDamage, float magicalDamage, float fireDamage, float holyDamage, float lightningDamage, float angleHitFrom,
        float contactPointX, float contactPointY, float contactPointZ)
    {
        if (IsServer)
        {
            NotifyTheServerOfCharacterDamageClientRpc(damageCharacterID, charcterCausingDamageID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, angleHitFrom,
                contactPointX, contactPointY, contactPointZ);
        }
    }

    [ClientRpc]
    public void NotifyTheServerOfCharacterDamageClientRpc(ulong damageCharacterID, ulong charcterCausingDamageID,
        float physicalDamage, float magicalDamage, float fireDamage, float holyDamage, float lightningDamage, float angleHitFrom,
        float contactPointX, float contactPointY, float contactPointZ)
    {
        ProcessCharacterDamageFromServer(damageCharacterID, charcterCausingDamageID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, angleHitFrom,
            contactPointX, contactPointY, contactPointZ);
    }

    public void ProcessCharacterDamageFromServer(ulong damageCharacterID, ulong charcterCausingDamageID,
        float physicalDamage, float magicalDamage, float fireDamage, float holyDamage, float lightningDamage, float angleHitFrom,
        float contactPointX, float contactPointY, float contactPointZ)
    {
        CharacterManager damageCharacter = NetworkManager.SpawnManager.SpawnedObjects[damageCharacterID].GetComponent<CharacterManager>();
        CharacterManager characterCausingDamage = NetworkManager.SpawnManager.SpawnedObjects[charcterCausingDamageID].GetComponent<CharacterManager>();
        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);

        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicalDamage = magicalDamage;
        damageEffect.fireDamage = fireDamage;
        damageEffect.holyDamage = holyDamage;
        damageEffect.lightningDamage = lightningDamage;
        damageEffect.angleHitFrom = angleHitFrom;
        damageEffect.contactPoint = new Vector3(contactPointX, contactPointY, contactPointZ);

        damageCharacter.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }
}
