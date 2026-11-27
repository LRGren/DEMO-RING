using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "New Channeling Spell", menuName = "Items/Spells/Channeling Spell")]
public class ChannelingSpell : SpellItem
{
    [Header("Channeling Spell Settings")]
    public float channelingRotationSpeed = 3f;
    public int focusCostPerTick = 2;
    public float tickInterval = .2f;

    [Header("Channeling Spell Animation")]
    public string Beam_End;
    public string Beam_End_Left;

    [Header("Channeling Spell Sound FX")]
    public AudioClip startChannelingSound;
    public AudioClip endChannelingSound;

    public override void AttemptToCastSpell(PlayerManager player)
    {
        if (!CanICastSpell(player))
            return;

        if (player.playerNetworkManager.isTwoHandingWeapon.Value)
        {
            player.playerCombatManager.isChannelingRightHand = true;
        }
        else
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
                player.playerCombatManager.isChannelingRightHand = true;
            else
                player.playerCombatManager.isChannelingRightHand = false;
        }

        player.playerCombatManager.isChannelingSpell = true;
        player.playerCombatManager.currentChannelingSpell = this;
        // 部分Root Motion要保留，不然会脚底滑动
        if (player.playerCombatManager.isChannelingRightHand)
            player.playerAnimatorManager.PlayerTargetActionAnimation(mainHandSpellAnimation, true, true, true, false, false, false);
        else
            player.playerAnimatorManager.PlayerTargetActionAnimation(offHandSpellAnimation, true, true, true, false, false, false);

        player.playerLocomotionManager.ChangeRotationSpeed(channelingRotationSpeed);
    }

    public override void SuccessfullyCastSpell(PlayerManager player)
    {
        if (!player.IsOwner)
            return;

        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if ((!PlayerInputManager.instance.isChannelingSpell_Right && player.playerCombatManager.isChannelingRightHand) || (!PlayerInputManager.instance.isChannelingSpell_Left && !player.playerCombatManager.isChannelingRightHand))
            return;

        if (player.playerCombatManager.currentChannelingSpellManager)
            return;

        bool isUsingRightWeapon;
        if (player.playerNetworkManager.isTwoHandingWeapon.Value)
        {
            // 左侧武器双手持握时仍是 leftWeaponModel，rightWeaponModel 已收在背后。
            isUsingRightWeapon = !player.playerNetworkManager.isTwoHandingLeftWeapon.Value;
        }
        else
        {
            isUsingRightWeapon = player.playerCombatManager.isChannelingRightHand;
        }

        GameObject weaponModel = isUsingRightWeapon
            ? player.playerEquipmentManager.rightWeaponModel
            : player.playerEquipmentManager.leftWeaponModel;

        if (weaponModel == null)
            return;

        var castLocation = weaponModel.GetComponentInChildren<SpellInstantiationLocation>();
        if (castLocation == null)
            return;

        Vector3 forward = player.transform.forward;
        var target = player.playerCombatManager.currentTarget;
        if (target != null && target.characterCombatManager.lockOnTransform != null)
        {
            Vector3 des = new Vector3(target.characterCombatManager.lockOnTransform.position.x, castLocation.transform.position.y, target.characterCombatManager.lockOnTransform.position.z);

            Vector3 targetDirection = des - castLocation.transform.position;
            if (targetDirection.sqrMagnitude > 0.0001f)
                forward = targetDirection.normalized;
        }

        player.transform.forward = forward;
        if (CreateBeamVisual(player, forward, isUsingRightWeapon))
        {
            base.SuccessfullyCastSpell(player);
            player.playerNetworkManager.NotifyTheServerOfStartedBeamServerRpc(NetworkManager.Singleton.LocalClientId, itemID, forward, isUsingRightWeapon);

            PlayerCamera.instance.ClearLockOnTargets();

            player.playerNetworkManager.isLockOn.Value = false;
        }
    }

    public bool CreateBeamVisual(PlayerManager player, Vector3 forward, bool isUsingRightHand)
    {
        if (player == null || spellCastReleaseFX == null || forward.sqrMagnitude <= 0.0001f)
            return false;

        if (player.playerCombatManager.currentChannelingSpellManager)
            return false;

        // 本地和远端都使用发送方确定的武器来源，不重新按 RB/LB 判断。
        GameObject weaponModel = isUsingRightHand
            ? player.playerEquipmentManager.rightWeaponModel
            : player.playerEquipmentManager.leftWeaponModel;
        if (weaponModel == null)
            return false;

        var castLocation = weaponModel.GetComponentInChildren<SpellInstantiationLocation>();
        if (castLocation == null)
            return false;

        if (spellCastReleaseFX.GetComponent<ChannelingSpellManager>() == null)
            return false;

        var startPoint = castLocation.transform;
        var beamManager = Instantiate(spellCastReleaseFX, startPoint.position, Quaternion.identity).GetComponent<ChannelingSpellManager>();
        beamManager.player = player;
        beamManager.SetChannelForward(forward.normalized);
        beamManager.SetStartPoint(startPoint);
        beamManager.channelingSpell = this;
        player.playerCombatManager.currentChannelingSpellManager = beamManager;
        if (startChannelingSound != null)
            player.characterSoundFXManager.PlaySoundFX(startChannelingSound);
        return true;
    }

    public void StopChannelingSpell(PlayerManager player, bool playEndAnimation = true)
    {
        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if (player.playerCombatManager.currentChannelingSpell != this)
            return;

        if (player.playerCombatManager.currentChannelingSpellManager != null)
            DestoryBeamVisual(player);

        player.playerNetworkManager.NotifyTheServerOfStoppedBeamServerRpc(NetworkManager.Singleton.LocalClientId, itemID);
        player.playerCombatManager.isChannelingSpell = false;
        player.playerCombatManager.currentChannelingSpell = null;
        player.playerLocomotionManager.ResetRotationSpeed();

        if (playEndAnimation)
        {
            if (player.playerCombatManager.isChannelingRightHand)
                player.playerAnimatorManager.PlayerTargetActionAnimation(Beam_End, true, false, false, false, false, false);
            else
                player.playerAnimatorManager.PlayerTargetActionAnimation(Beam_End_Left, true, false, false, false, false, false);
        }
    }

    public void DestoryBeamVisual(PlayerManager player)
    {
        if (player.playerCombatManager.currentChannelingSpellManager)
        {
            Destroy(player.playerCombatManager.currentChannelingSpellManager.gameObject);
            player.playerCombatManager.currentChannelingSpellManager = null;
            if (endChannelingSound != null)
                player.characterSoundFXManager.PlaySoundFX(endChannelingSound);
        }
    }
}
