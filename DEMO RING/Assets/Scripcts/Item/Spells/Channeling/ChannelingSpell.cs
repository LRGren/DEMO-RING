using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channeling Spell", menuName = "Items/Spells/Channeling Spell")]
public class ChannelingSpell : SpellItem
{
    [Header("Channeling Spell Settings")]
    public int focusCostPerTick = 2;
    public float tickInterval = .2f;

    [Header("Channeling Spell Animation")]
    public string Beam_Loop;
    public string Beam_End;

    public override void AttemptToCastSpell(PlayerManager player)
    {
        if (!CanICastSpell(player))
            return;

        player.playerCombatManager.isChannelingSpell = true;
        player.playerCombatManager.currentChannelingSpell = this;
        // 部分Root Motion要保留，不然会脚底滑动
        player.playerAnimatorManager.PlayerTargetActionAnimation(mainHandSpellAnimation, true, true, false, false, false, false);
    }

    public override void SuccessfullyCastSpell(PlayerManager player)
    {
        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if (!PlayerInputManager.instance.isChannelingSpell_Right)
            return;

        if (player.playerCombatManager.currentChannelingSpellManager)
            return;

        var startPoint = player.playerEquipmentManager.rightWeaponModel.transform.GetComponentInChildren<SpellInstantiationLocation>().transform;
        if (!startPoint)
            return;

        if (!spellCastReleaseFX)
            return;

        base.SuccessfullyCastSpell(player);

        player.playerCombatManager.currentChannelingSpellManager = Instantiate(spellCastReleaseFX, startPoint.position, Quaternion.identity).GetComponent<ChannelingSpellManager>();
        player.playerCombatManager.currentChannelingSpellManager.player = player;
        if (player.playerCombatManager.currentTarget)
        {
            Vector3 targetDirection = player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - startPoint.position;
            player.playerCombatManager.currentChannelingSpellManager.SetChannelForward(targetDirection.normalized);
        }
        else
        {
            player.playerCombatManager.currentChannelingSpellManager.SetChannelForward(player.transform.forward);
        }
        player.playerCombatManager.currentChannelingSpellManager.SetStartPoint(startPoint);
        player.playerCombatManager.currentChannelingSpellManager.channelingSpell = this;
    }

    public void StopChannelingSpell(PlayerManager player, bool playEndAnimation = true)
    {
        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if (player.playerCombatManager.currentChannelingSpellManager)
        {
            Destroy(player.playerCombatManager.currentChannelingSpellManager.gameObject);
            player.playerCombatManager.currentChannelingSpellManager = null;
        }

        player.playerCombatManager.isChannelingSpell = false;
        player.playerCombatManager.currentChannelingSpell = null;

        if (playEndAnimation)
            player.playerAnimatorManager.PlayerTargetActionAnimation(Beam_End, true, false, false, false, false, false);
    }

}
