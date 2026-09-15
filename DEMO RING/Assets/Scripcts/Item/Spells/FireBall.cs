using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Spells/Fire Ball")]
public class FireBall : SpellItem
{
    public float upwardForce;
    public float forwardForce;

    public override void AttemptToCastSpell(PlayerManager player)
    {
        base.AttemptToCastSpell(player);

        if (!CanICastSpell(player))
        {
            return;
        }

        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            player.playerAnimatorManager.PlayerTargetActionAnimation(mainHandSpellAnimtion, true);
        }
        if (player.playerNetworkManager.isUsingLeftHand.Value)
        {
            player.playerAnimatorManager.PlayerTargetActionAnimation(offHandSpellAnimtion, true);
        }
    }

    public override void SuccessfullyCastSpell(PlayerManager player)
    {
        base.SuccessfullyCastSpell(player);

        // 1. 销毁WarmUpFX
        if (player.IsOwner)
            player.playerCombatManager.DestoryAllAttemptedActions();

        // 2. 获得Caster的Collider
        /*Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
        Collider characterCollisionCollider = player.GetComponent<Collider>();*/

        // 3. 生成ReleaseFX
        SpellInstantiationLocation spellInstantiationLocation;
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            spellInstantiationLocation = player.playerEquipmentManager.rightHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }
        else
        {
            spellInstantiationLocation = player.playerEquipmentManager.leftHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }

        GameObject instantiatedReleasedFX = Instantiate(spellCastReleaseFX);
        instantiatedReleasedFX.transform.parent = spellInstantiationLocation.transform;
        instantiatedReleasedFX.transform.localPosition = Vector3.zero;
        instantiatedReleasedFX.transform.localRotation = Quaternion.identity;
        instantiatedReleasedFX.transform.parent = null;

        // 4. 使用DamageEffect来处理伤害
        FireBallManager fireBallManager = instantiatedReleasedFX.GetComponent<FireBallManager>();
        fireBallManager.InitializeFireBall(player);

        // 5. 将Caster的Collider传入DamageEffect中，以便在处理伤害时排除Caster自身
        /*Physics.IgnoreCollision(characterCollisionCollider, fireBallManager.damageCollider.damageCollider);
        foreach (Collider collider in characterColliders)
        {
            Physics.IgnoreCollision(collider, fireBallManager.damageCollider.damageCollider);
        }
        我们已经在FireBallDamageCollider判断过了*/

        // 6. 设置ReleaseFX的发射速度等

        if (player.playerNetworkManager.isLockOn.Value)
        {
            instantiatedReleasedFX.transform.LookAt(player.playerCombatManager.currentTarget.transform.position);
        }
        else
        {
            Vector3 forwardDirection = player.transform.forward;
            instantiatedReleasedFX.transform.forward = forwardDirection;
        }

        Rigidbody spellRigidbody = instantiatedReleasedFX.GetComponent<Rigidbody>();
        Vector3 upwardVelocity = instantiatedReleasedFX.transform.up * upwardForce;
        Vector3 forwardVelocity = instantiatedReleasedFX.transform.forward * forwardForce;
        Vector3 totalVelocity = upwardVelocity + forwardVelocity;
        spellRigidbody.velocity = totalVelocity;
    }

    public override void SuccessfullyCastSpellFull(PlayerManager player)
    {
        base.SuccessfullyCastSpellFull(player);

        // 1. 销毁WarmUpFX
        if (player.IsOwner)
            player.playerCombatManager.DestoryAllAttemptedActions();

        // 2. 获得Caster的Collider
        /*Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
        Collider characterCollisionCollider = player.GetComponent<Collider>();*/

        // 3. 生成ReleaseFX
        SpellInstantiationLocation spellInstantiationLocation;
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            spellInstantiationLocation = player.playerEquipmentManager.rightHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }
        else
        {
            spellInstantiationLocation = player.playerEquipmentManager.leftHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }

        GameObject instantiatedReleasedFX = Instantiate(spellCastReleaseFXFullCharge);
        instantiatedReleasedFX.transform.parent = spellInstantiationLocation.transform;
        instantiatedReleasedFX.transform.localPosition = Vector3.zero;
        instantiatedReleasedFX.transform.localRotation = Quaternion.identity;
        instantiatedReleasedFX.transform.parent = null;

        // 4. 使用DamageEffect来处理伤害
        FireBallManager fireBallManager = instantiatedReleasedFX.GetComponent<FireBallManager>();
        fireBallManager.isFullCharge = true;
        fireBallManager.InitializeFireBall(player);

        // 5. 将Caster的Collider传入DamageEffect中，以便在处理伤害时排除Caster自身
        /*Physics.IgnoreCollision(characterCollisionCollider, fireBallManager.damageCollider.damageCollider);
        foreach (Collider collider in characterColliders)
        {
            Physics.IgnoreCollision(collider, fireBallManager.damageCollider.damageCollider);
        }
        我们已经在FireBallDamageCollider判断过了*/

        // 6. 设置ReleaseFX的发射速度等

        if (player.playerNetworkManager.isLockOn.Value)
        {
            instantiatedReleasedFX.transform.LookAt(player.playerCombatManager.currentTarget.transform.position);
        }
        else
        {
            Vector3 forwardDirection = player.transform.forward;
            instantiatedReleasedFX.transform.forward = forwardDirection;
        }

        Rigidbody spellRigidbody = instantiatedReleasedFX.GetComponent<Rigidbody>();
        Vector3 upwardVelocity = instantiatedReleasedFX.transform.up * upwardForce;
        Vector3 forwardVelocity = instantiatedReleasedFX.transform.forward * forwardForce;
        Vector3 totalVelocity = upwardVelocity + forwardVelocity;
        spellRigidbody.velocity = totalVelocity;
    }

    public override void InstantiateSpellCastWarmUpFX(PlayerManager player)
    {
        base.InstantiateSpellCastWarmUpFX(player);

        //Debug.Log("InstantiateSpellCastWarmUpFX");

        // 1. 确定现在施法的是哪个手的法术
        SpellInstantiationLocation spellInstantiationLocation;

        // 2. 根据施法手或者是法杖的不同，确定不同的WarmUpFX的生成位置和旋转角度
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            spellInstantiationLocation = player.playerEquipmentManager.rightHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }
        else
        {
            spellInstantiationLocation = player.playerEquipmentManager.leftHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }


        // 3. 生成WarmUpFX
        GameObject instantiatedWarmUpFX = Instantiate(spellCastWarmUpFX);
        instantiatedWarmUpFX.transform.parent = spellInstantiationLocation.transform;
        instantiatedWarmUpFX.transform.localPosition = Vector3.zero;
        instantiatedWarmUpFX.transform.localRotation = Quaternion.identity;

        // 4. 将WarmUpFX保存为一个遍历，以便在施法结束后或者被打断时销毁WarmUpFX
        player.characterEffectsManager.activeSpellWarmUpFX = instantiatedWarmUpFX;
    }

    public override void SuccessfullyChargeSpell(PlayerManager player)
    {
        base.SuccessfullyChargeSpell(player);

        if (player.IsOwner)
            player.playerCombatManager.DestoryAllAttemptedActions();

        SpellInstantiationLocation spellInstantiationLocation;
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            spellInstantiationLocation = player.playerEquipmentManager.rightHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }
        else
        {
            spellInstantiationLocation = player.playerEquipmentManager.leftHandWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
        }

        GameObject instantiatedChargeFX = Instantiate(spellChargeFX);
        instantiatedChargeFX.transform.parent = spellInstantiationLocation.transform;
        instantiatedChargeFX.transform.localPosition = Vector3.zero;
        instantiatedChargeFX.transform.localRotation = Quaternion.identity;

        player.characterEffectsManager.activeSpellWarmUpFX = instantiatedChargeFX;
    }

}
