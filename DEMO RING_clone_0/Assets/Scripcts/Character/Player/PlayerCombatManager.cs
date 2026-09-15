using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombatManager : CharacterCombatManager
{
    PlayerManager player;
    public WeaponItem currentWeaponBedingUsed;
    public ProjectileSlot currentProjectileSlotBeingUsed;

    [Header("Projectile")]
    public Vector3 projectileAimDirection;

    [Header("Flags")]
    public bool canComboWithMainHandWeapon = false;
    //public bool canComboWithOffHandWeapon = false;
    public bool isUsingItem = false;

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

            //应该在↑↑↑↑↑↑↑↑↑↑ （AttemptToPerformAction）中实现
            //执行对应的动画
            //不应该在这里发送RPC，这会导致不必要的浪费，应该在排除所有情况后再发送RPC，确保发送次数最少
            //player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID);

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
            case AttackType.JumpLightAttack01:
                staminaCost *= currentWeaponBedingUsed.jumpLightAttackStaminaModifier;
                break;
            case AttackType.JumpHeavyAttack01:
                staminaCost *= currentWeaponBedingUsed.jumpHeavyAttackStaminaModifier;
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


    // Combo
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

    // Projectile
    public void ReleaseArrow()
    {
        if (!player.IsOwner)
            return;

        // 播放射箭音效
        player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.releaseArrowSFX));

        player.playerNetworkManager.hasArrowNotched.Value = false;

        // 删掉手中的箭矢
        if (player.playerEffectsManager.activeProjectileFX != null)
        {
            Destroy(player.playerEffectsManager.activeProjectileFX.gameObject);
            player.playerEffectsManager.activeProjectileFX = null;
        }

        RangedProjectileItem projectileToFire = null;
        switch (currentProjectileSlotBeingUsed)
        {
            case ProjectileSlot.MainProjectileSlot:
                projectileToFire = player.playerInventoryManager.mainProjectile;
                break;
            case ProjectileSlot.SecondaryProjectileSlot:
                projectileToFire = player.playerInventoryManager.secondaryProjectile;
                break;
        }

        if (projectileToFire == null)
            return;

        if (projectileToFire.currentAmmoAmount <= 0)
            return;

        Transform projectileInstantiatePoint = player.playerCombatManager.lockOnTransform;
        GameObject liveProjectileGameObject = Instantiate(projectileToFire.releaseProjectileModel, projectileInstantiatePoint);
        RangedProjectileDamageCollider liveProjectileDamageCollider = liveProjectileGameObject.GetComponent<RangedProjectileDamageCollider>();
        Rigidbody liveProjectileRigidbody = liveProjectileDamageCollider.projectileRigidbody;

        // (TODO:) 伤害计算，距离衰减
        liveProjectileDamageCollider.physicalDamage = projectileToFire.physicalDamage;
        liveProjectileDamageCollider.characterShootingProjectile = player;

        // 三种发射方式
        if (player.playerNetworkManager.isAiming.Value)
        {
            // 瞄准
            // 3. 不锁定 瞄准
            Ray ray = new Ray(lockOnTransform.position, PlayerCamera.instance.aimDirection);
            projectileAimDirection = ray.GetPoint(5000);
            liveProjectileGameObject.transform.LookAt(projectileAimDirection);
        }
        else
        {
            // 不瞄准
            // // 1. 锁定
            if (player.playerCombatManager.currentTarget != null)
            {
                liveProjectileGameObject.transform.rotation = Quaternion.LookRotation(player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - liveProjectileGameObject.transform.position);
            }
            // 2. 不锁定 但是 不瞄准
            else
            {
                liveProjectileGameObject.transform.rotation = Quaternion.LookRotation(player.transform.forward);
            }
        }

        // 无视碰撞
        Collider[] collisions = player.GetComponentsInChildren<Collider>();
        List<Collider> collisionList = new List<Collider>(collisions);
        foreach (var col in collisions)
            collisionList.Add(col);

        foreach (var col in collisionList)
            Physics.IgnoreCollision(liveProjectileDamageCollider.damageCollider, col, true);

        // 减少箭矢数量
        //projectileToFire.currentAmmoAmount--;

        // 动量
        //liveProjectileRigidbody.mass = projectileToFire.ammoMass;

        liveProjectileRigidbody.AddForce(liveProjectileGameObject.transform.forward * projectileToFire.forwardVelocity);
        liveProjectileRigidbody.AddForce(liveProjectileGameObject.transform.up * projectileToFire.upwardVelocity);

        liveProjectileGameObject.transform.parent = null;

        // RPC
        player.playerNetworkManager.NotifyTheServerOfReleasedProjectileServerRpc(
            player.OwnerClientId,
            projectileToFire.itemID, liveProjectileGameObject.transform.position.x, liveProjectileGameObject.transform.position.y, liveProjectileGameObject.transform.position.z,
            liveProjectileGameObject.transform.rotation.eulerAngles.y);

    }


    // Spell Casting
    public void InstantiateSpellCastWarmUpFX()
    {
        if (player.playerInventoryManager.currentSpell == null)
            return;

        player.playerInventoryManager.currentSpell.InstantiateSpellCastWarmUpFX(player);
    }

    public void SuccessfullyCastSpell()
    {
        if (player.playerInventoryManager.currentSpell == null)
            return;

        player.playerInventoryManager.currentSpell.SuccessfullyCastSpell(player);
    }

    public void SuccessfullyCastSpellFull()
    {
        if (player.playerInventoryManager.currentSpell == null)
            return;

        player.playerInventoryManager.currentSpell.SuccessfullyCastSpellFull(player);
    }

    public void SuccessfullyChargeSpell()
    {
        if (player.playerInventoryManager.currentSpell == null)
            return;

        player.playerInventoryManager.currentSpell.SuccessfullyChargeSpell(player);
    }

    // Quick Slot Item
    public void SuccessfullyUseQuickSlotItem()
    {
        if (player.playerInventoryManager.currentQuickSlotItem == null)
            return;

        player.playerInventoryManager.currentQuickSlotItem.SuccessfullyUsedItem(player);
    }

    // Ash of War
    public WeaponItem SelectWeaponToPerformAshOfWar()
    {
        WeaponItem selectedWeapon = player.playerInventoryManager.currentLeftHandWeapon;
        player.playerNetworkManager.SetCharacterActionHand(false);
        player.playerCombatManager.currentWeaponBedingUsed = selectedWeapon;

        return selectedWeapon;
    }

    public override void CloseAllDamageColliders()
    {
        base.CloseAllDamageColliders();

        if (player.IsOwner)
        {
            player.playerEquipmentManager.rightHandWeaponManager.meleeWeaponDamageCollider.DisableDamageCollider();
            player.playerEquipmentManager.leftHandWeaponManager.meleeWeaponDamageCollider.DisableDamageCollider();
        }
    }

}
