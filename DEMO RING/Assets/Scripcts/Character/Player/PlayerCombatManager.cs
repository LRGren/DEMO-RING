using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombatManager : CharacterCombatManager
{
    PlayerManager player;
    public WeaponItem currentWeaponBedingUsed;

    [Header("Flags")]
    public bool canComboWithMainHandWeapon = false;
    //public bool canComboWithOffHandWeapon = false;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<PlayerManager>();
    }

    public void PerformWeaponBasedAction(WeaponItemAction weaponAction, WeaponItem weaponPerformingAction)
    {
        if (player.IsOwner)
        {
            if (weaponAction == null)
                return;

            weaponAction.AttemptToPerformAction(player, weaponPerformingAction);

            //执行对应的动画
            player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID); ;

        }
    }

    public override void AttemptRiposte(RaycastHit hit)
    {
        //base.AttemptRiposte(hit);
        CharacterManager targetCharacter = hit.transform.GetComponentInParent<CharacterManager>();

        if (targetCharacter == null)
            return;

        if (!targetCharacter.characterNetworkManager.isRipostable.Value)
            return;

        if (targetCharacter.characterNetworkManager.isBeingRiposted.Value)
            return;

        MeleeWeaponItem riposteWeapon;
        if (player.playerNetworkManager.isTwoHandingLeftWeapon.Value)
        {
            riposteWeapon = player.playerInventoryManager.currentLeftHandWeapon as MeleeWeaponItem;
        }
        else
        {
            riposteWeapon = player.playerInventoryManager.currentRightHandWeapon as MeleeWeaponItem;
        }

        characterManager.characterAnimatorManager.PlayerTargetActionAnimationInstantly("Riposte_01", true);

        if (player.IsOwner)
        {
            player.playerNetworkManager.isInvulnerable.Value = true;
        }

        // Take Critical Damage Effect
        TakeCriticalDamageEffect damageEffect = WorldCharacterEffectsManager.instance.takeCriticalDamageEffect;

        // Apply All Stats From The Collider To The Damage Effect
        damageEffect.physicalDamage = riposteWeapon.physicalDamage;
        damageEffect.magicalDamage = riposteWeapon.magicalDamage;
        damageEffect.fireDamage = riposteWeapon.fireDamage;
        damageEffect.lightningDamage = riposteWeapon.lightningDamage;
        damageEffect.holyDamage = riposteWeapon.holyDamage;
        damageEffect.poiseDamage = riposteWeapon.poiseDamage;

        float modifier = (float)(riposteWeapon.criticalDamage / 100f) * WorldUtilityManager.instance.GetCriticalAttackDamageMultiplierBasedOnWeaponClass(riposteWeapon.weaponClass);

        damageEffect.physicalDamage *= modifier;
        damageEffect.magicalDamage *= modifier;
        damageEffect.fireDamage *= modifier;
        damageEffect.lightningDamage *= modifier;
        damageEffect.holyDamage *= modifier;

        // Using A Server RPC
        targetCharacter.characterNetworkManager.NotifyTheServerOfRiposteServerRpc(targetCharacter.NetworkObjectId, characterManager.NetworkObjectId, "Riposted_01", riposteWeapon.itemID, damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.lightningDamage, damageEffect.holyDamage, damageEffect.poiseDamage);

    }

    public override void AttemptBackstab(RaycastHit hit)
    {
        CharacterManager targetCharacter = hit.transform.GetComponentInParent<CharacterManager>();

        if (targetCharacter == null)
            return;

        if (!targetCharacter.characterCombatManager.canBeBackstabbed)
            return;

        if (targetCharacter.characterNetworkManager.isBeingRiposted.Value)
            return;

        MeleeWeaponItem riposteWeapon;
        if (player.playerNetworkManager.isTwoHandingLeftWeapon.Value)
        {
            riposteWeapon = player.playerInventoryManager.currentLeftHandWeapon as MeleeWeaponItem;
        }
        else
        {
            riposteWeapon = player.playerInventoryManager.currentRightHandWeapon as MeleeWeaponItem;
        }

        characterManager.characterAnimatorManager.PlayerTargetActionAnimationInstantly("Backstab_01", true);

        if (player.IsOwner)
        {
            player.playerNetworkManager.isInvulnerable.Value = true;
        }

        // Take Critical Damage Effect
        TakeCriticalDamageEffect damageEffect = WorldCharacterEffectsManager.instance.takeCriticalDamageEffect;

        // Apply All Stats From The Collider To The Damage Effect
        damageEffect.physicalDamage = riposteWeapon.physicalDamage;
        damageEffect.magicalDamage = riposteWeapon.magicalDamage;
        damageEffect.fireDamage = riposteWeapon.fireDamage;
        damageEffect.lightningDamage = riposteWeapon.lightningDamage;
        damageEffect.holyDamage = riposteWeapon.holyDamage;
        damageEffect.poiseDamage = riposteWeapon.poiseDamage;

        float modifier = (float)(riposteWeapon.criticalDamage / 100f) * WorldUtilityManager.instance.GetBackstabAttackDamageMultiplierBasedOnWeaponClass(riposteWeapon.weaponClass);

        damageEffect.physicalDamage *= modifier;
        damageEffect.magicalDamage *= modifier;
        damageEffect.fireDamage *= modifier;
        damageEffect.lightningDamage *= modifier;
        damageEffect.holyDamage *= modifier;
        damageEffect.poiseDamage *= modifier;

        // Using A Server RPC
        targetCharacter.characterNetworkManager.NotifyTheServerOfBackstabServerRpc(targetCharacter.NetworkObjectId, characterManager.NetworkObjectId, "Backstabbed_01", riposteWeapon.itemID, damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.lightningDamage, damageEffect.holyDamage, damageEffect.poiseDamage);
    }

    public virtual void DrainStaminaBasedOnAttack()
    {
        if (!player.IsOwner)
            return;

        if (currentWeaponBedingUsed == null)
            return;

        float staminaCost = currentWeaponBedingUsed.basicStaminaCost;

        switch (currentAttackType)
        {
            case AttackType.LightAttack01:
            case AttackType.LightAttack02:
                staminaCost *= currentWeaponBedingUsed.lightAttackStaminaModifier;
                break;
            case AttackType.HeavyAttack01:
            case AttackType.HeavyAttack02:
                staminaCost *= currentWeaponBedingUsed.heavyAttackStaminaModifier;
                break;
            case AttackType.ChargedAttack01:
            case AttackType.ChargedAttack02:
                staminaCost *= currentWeaponBedingUsed.chargedAttackStaminaModifier;
                break;
            case AttackType.RunningAttack01:
                staminaCost *= currentWeaponBedingUsed.runningAttackStaminaModifier;
                break;
            case AttackType.RollingAttack01:
                staminaCost *= currentWeaponBedingUsed.rollingAttackStaminaModifier;
                break;
            case AttackType.BackstepAttack01:
                staminaCost *= currentWeaponBedingUsed.backstepAttackStaminaModifier;
                break;
            default:
                break;
        }

        player.playerNetworkManager.currentStamina.Value -= staminaCost;
    }

    public override void SetTarget(CharacterManager newTarget)
    {
        base.SetTarget(newTarget);

        if (player.IsOwner)
        {
            PlayerCamera.instance.SetLockOnCameraHeight();
        }
    }

    public override void EnableDoCombo()
    {
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            player.playerCombatManager.canComboWithMainHandWeapon = true;
        }
        else
        {

        }
    }

    public override void DisableDoCombo()
    {
        player.playerCombatManager.canComboWithMainHandWeapon = false;
    }

    public WeaponItem SelectWeaponToPerformAshOfWar()
    {
        WeaponItem selectedWeapon = player.playerInventoryManager.currentLeftHandWeapon;
        player.playerNetworkManager.SetCharacterActionHand(false);
        player.playerCombatManager.currentWeaponBedingUsed = selectedWeapon;

        return selectedWeapon;
    }

}
