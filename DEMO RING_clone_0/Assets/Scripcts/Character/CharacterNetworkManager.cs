using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterNetworkManager : NetworkBehaviour
{
    private CharacterManager character;

    [Header("Is Active")]
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
    public NetworkVariable<bool> isParrying = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isParryable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isBlocking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isLockOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isChargingAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isRipostable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isBeingRiposted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Stats")]
    public NetworkVariable<int> endurance = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> mind = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> vitality = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> strength = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Stats Modifiers")]
    public NetworkVariable<int> strengthModifiers = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    [Header("Resources")]
    public NetworkVariable<float> currentStamina = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentFocusPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxFocus = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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

    public virtual void CheckFP(int oldValue, int newValue)
    {
        if (!character.IsOwner)
            return;

        if (currentFocusPoints.Value >= maxFocus.Value)
        {
            currentFocusPoints.Value = maxFocus.Value;
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

            if (IsOwner && PlayerCamera.instance != null)
            {
                PlayerCamera.instance.SetLockOnCameraHeight();
            }
        }
    }

    public void OnIsChargingAttackChanged(bool old, bool newStatus)
    {
        character.animator.SetBool("isChargingAttack", isChargingAttack.Value);
    }

    public void OnIsMovingChanged(bool old, bool newStatus)
    {
        character.animator.SetBool("isMoving", isMoving.Value);
    }

    public void OnIsActiveChanged(bool old, bool newStatus)
    {
        gameObject.SetActive(isActive.Value);
    }

    public virtual void OnIsBlockingChanged(bool old, bool isLockedOn)
    {
        character.animator.SetBool("isBlocking", isBlocking.Value);
    }

    public virtual void OnIsDeadChanged(bool old, bool newStatus)
    {
        character.animator.SetBool("isDead", character.isDead.Value);
    }

    #region Cancel All Attempted Actions (FX, Animations, etc.)
    [ServerRpc]
    public void DestoryAllAttemptedActionsServerRpc()
    {
        if (IsServer)
        {
            DestoryAllAttemptedActionsClientRpc();
        }
    }

    // 清理所有尝试中的动作特效（普通方法，不是 RPC，可以在子类 override 里安全调用）
    protected void ClearAllAttemptedActionsFX()
    {
        if (character.characterEffectsManager.activeQuickSlotItemFX != null)
        {
            Destroy(character.characterEffectsManager.activeQuickSlotItemFX.gameObject);
            character.characterEffectsManager.activeQuickSlotItemFX = null;
        }

        if (character.characterEffectsManager.activeSpellWarmUpFX != null)
        {
            Destroy(character.characterEffectsManager.activeSpellWarmUpFX.gameObject);
            character.characterEffectsManager.activeSpellWarmUpFX = null;
        }

        if (character.characterEffectsManager.activeProjectileFX != null)
        {
            Destroy(character.characterEffectsManager.activeProjectileFX.gameObject);
            character.characterEffectsManager.activeProjectileFX = null;
        }
    }

    [ClientRpc]
    public virtual void DestoryAllAttemptedActionsClientRpc()
    {
        ClearAllAttemptedActionsFX();
    }
    #endregion

    #region Action Animation
    [ServerRpc]
    public void NotifyTheServerOfActionAnimationServerRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (IsServer)
        {
            PlayActionAnimationForAllClientsClientRpc(clientId, animationName, applyRootMotion);
        }
    }

    [ClientRpc]
    public void PlayActionAnimationForAllClientsClientRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PerformActionAnimationFromServer(animationName, applyRootMotion);
        }
    }

    public void PerformActionAnimationFromServer(string animationName, bool applyRootMotion)
    {
        character.characterAnimatorManager.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(animationName, 0.2f);
    }
    #endregion

    #region Action Animation Instant
    [ServerRpc]
    public void NotifyTheServerOfActionAnimationInstantlyServerRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (IsServer)
        {
            PlayActionAnimationInstantlyForAllClientsClientRpc(clientId, animationName, applyRootMotion);
        }
    }

    [ClientRpc]
    public void PlayActionAnimationInstantlyForAllClientsClientRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PerformActionAnimationInstantlyFromServer(animationName, applyRootMotion);
        }
    }

    public void PerformActionAnimationInstantlyFromServer(string animationName, bool applyRootMotion)
    {
        character.characterAnimatorManager.applyRootMotion = applyRootMotion;
        character.animator.Play(animationName);
    }
    #endregion

    #region Attack Animation
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
        character.characterAnimatorManager.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(animationName, 0.2f);
    }
    #endregion

    #region Damage
    [ServerRpc(RequireOwnership = false)]
    public void NotifyTheServerOfCharacterDamageServerRpc(
        ulong damageCharacterID, ulong charcterCausingDamageID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float angleHitFrom,
        float poiseDamage,
        float contactPointX,
        float contactPointY,
        float contactPointZ)
    {
        if (IsServer)
        {
            NotifyTheServerOfCharacterDamageClientRpc(damageCharacterID, charcterCausingDamageID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, angleHitFrom, poiseDamage, contactPointX, contactPointY, contactPointZ);
        }
    }

    [ClientRpc]
    public void NotifyTheServerOfCharacterDamageClientRpc(
        ulong damageCharacterID,
        ulong charcterCausingDamageID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float angleHitFrom,
        float poiseDamage,
        float contactPointX,
        float contactPointY,
        float contactPointZ)
    {
        ProcessCharacterDamageFromServer(damageCharacterID, charcterCausingDamageID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, angleHitFrom, poiseDamage, contactPointX, contactPointY, contactPointZ);
    }

    public void ProcessCharacterDamageFromServer(
        ulong damageCharacterID,
        ulong charcterCausingDamageID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float angleHitFrom,
        float poiseDamage,
        float contactPointX,
        float contactPointY,
        float contactPointZ)
    {
        CharacterManager damageCharacter = NetworkManager.SpawnManager.SpawnedObjects[damageCharacterID].GetComponent<CharacterManager>();
        CharacterManager characterCausingDamage = NetworkManager.SpawnManager.SpawnedObjects[charcterCausingDamageID].GetComponent<CharacterManager>();
        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);

        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicalDamage = magicalDamage;
        damageEffect.fireDamage = fireDamage;
        damageEffect.holyDamage = holyDamage;
        damageEffect.lightningDamage = lightningDamage;
        damageEffect.characterCausingDamage = characterCausingDamage;
        damageEffect.poiseDamage = poiseDamage;
        damageEffect.angleHitFrom = angleHitFrom;
        damageEffect.contactPoint = new Vector3(contactPointX, contactPointY, contactPointZ);

        damageCharacter.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }


    // Riposte
    [ServerRpc(RequireOwnership = false)]
    public void NotifyTheServerOfRiposteServerRpc(
        ulong damageCharacterID,
        ulong charcterCausingDamageID,
        string criticalDamageAnimation,
        int weaponID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float poiseDamage)
    {
        if (IsServer)
        {
            NotifyTheServerOfRiposteClientRpc(damageCharacterID, charcterCausingDamageID, criticalDamageAnimation, weaponID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, poiseDamage);
        }
    }

    [ClientRpc]
    public void NotifyTheServerOfRiposteClientRpc(ulong damageCharacterID,
        ulong charcterCausingDamageID,
        string criticalDamageAnimation,
        int weaponID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float poiseDamage)
    {
        ProcessRiposteFromServer(damageCharacterID, charcterCausingDamageID, criticalDamageAnimation, weaponID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, poiseDamage);
    }

    public void ProcessRiposteFromServer(ulong damageCharacterID,
        ulong charcterCausingDamageID,
        string criticalDamageAnimation,
        int weaponID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float poiseDamage)
    {
        CharacterManager damageCharacter = NetworkManager.SpawnManager.SpawnedObjects[damageCharacterID].GetComponent<CharacterManager>();
        CharacterManager characterCausingDamage = NetworkManager.SpawnManager.SpawnedObjects[charcterCausingDamageID].GetComponent<CharacterManager>();
        WeaponItem weapon = WorldItemDatabase.instance.GetWeaponByID(weaponID);

        TakeCriticalDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeCriticalDamageEffect);

        if (damageCharacter.IsOwner)
            damageCharacter.characterNetworkManager.isBeingRiposted.Value = true;

        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicalDamage = magicalDamage;
        damageEffect.fireDamage = fireDamage;
        damageEffect.holyDamage = holyDamage;
        damageEffect.lightningDamage = lightningDamage;
        damageEffect.characterCausingDamage = characterCausingDamage;

        damageEffect.poiseDamage = poiseDamage;

        damageCharacter.characterEffectsManager.ProcessInstantEffect(damageEffect);

        if (damageCharacter.IsOwner)
            damageCharacter.characterAnimatorManager.PlayerTargetActionAnimationInstantly(criticalDamageAnimation, true);

        StartCoroutine(characterCausingDamage.characterCombatManager.ForceMoveEnemyCharacterToRipostePosition
        (damageCharacter, WorldUtilityManager.instance.GetRipostingPositionBasedOnWeaponClass(weapon.weaponClass)));
    }

    // Backstab
    [ServerRpc(RequireOwnership = false)]
    public void NotifyTheServerOfBackstabServerRpc(
        ulong damageCharacterID,
        ulong charcterCausingDamageID,
        string criticalDamageAnimation,
        int weaponID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float poiseDamage)
    {
        if (IsServer)
        {
            NotifyTheServerOfBackstabClientRpc(damageCharacterID, charcterCausingDamageID, criticalDamageAnimation, weaponID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, poiseDamage);
        }
    }

    [ClientRpc]
    public void NotifyTheServerOfBackstabClientRpc(ulong damageCharacterID,
        ulong charcterCausingDamageID,
        string criticalDamageAnimation,
        int weaponID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float poiseDamage)
    {
        ProcessBackstabFromServer(damageCharacterID, charcterCausingDamageID, criticalDamageAnimation, weaponID, physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage, poiseDamage);
    }

    public void ProcessBackstabFromServer(ulong damageCharacterID,
        ulong charcterCausingDamageID,
        string criticalDamageAnimation,
        int weaponID,
        float physicalDamage,
        float magicalDamage,
        float fireDamage,
        float holyDamage,
        float lightningDamage,
        float poiseDamage)
    {
        CharacterManager damageCharacter = NetworkManager.SpawnManager.SpawnedObjects[damageCharacterID].GetComponent<CharacterManager>();
        CharacterManager characterCausingDamage = NetworkManager.SpawnManager.SpawnedObjects[charcterCausingDamageID].GetComponent<CharacterManager>();
        WeaponItem weapon = WorldItemDatabase.instance.GetWeaponByID(weaponID);

        TakeCriticalDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeCriticalDamageEffect);

        if (damageCharacter.IsOwner)
            damageCharacter.characterNetworkManager.isBeingRiposted.Value = true;

        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicalDamage = magicalDamage;
        damageEffect.fireDamage = fireDamage;
        damageEffect.holyDamage = holyDamage;
        damageEffect.lightningDamage = lightningDamage;
        damageEffect.characterCausingDamage = characterCausingDamage;

        damageEffect.poiseDamage = poiseDamage;

        damageCharacter.characterEffectsManager.ProcessInstantEffect(damageEffect);

        if (damageCharacter.IsOwner)
            damageCharacter.characterAnimatorManager.PlayerTargetActionAnimationInstantly(criticalDamageAnimation, true);

        StartCoroutine(characterCausingDamage.characterCombatManager.ForceMoveEnemyCharacterToBackstabPosition
        (damageCharacter, WorldUtilityManager.instance.GetBackstabbingPositionBasedOnWeaponClass(weapon.weaponClass)));
    }

    #endregion

    #region Parry
    [ServerRpc(RequireOwnership = false)]
    public void NotifyTheServerOfCharacterParriedServerRpc(ulong charcterCausingDamageID)
    {
        if (IsServer)
        {
            NotifyTheServerOfCharacterParriedClientRpc(charcterCausingDamageID);
        }
    }

    [ClientRpc]
    private void NotifyTheServerOfCharacterParriedClientRpc(ulong charcterCausingDamageID)
    {
        ProcessCharacterParriedFromServer(charcterCausingDamageID);
    }

    private void ProcessCharacterParriedFromServer(ulong charcterCausingDamageID)
    {
        CharacterManager characterCausingDamage = NetworkManager.SpawnManager.SpawnedObjects[charcterCausingDamageID].gameObject.GetComponent<CharacterManager>();

        if (characterCausingDamage == null)
            return;

        if (characterCausingDamage.IsOwner)
        {
            characterCausingDamage.characterAnimatorManager.PlayerTargetActionAnimationInstantly("Parried_01", true);
        }
    }

    #endregion

}
