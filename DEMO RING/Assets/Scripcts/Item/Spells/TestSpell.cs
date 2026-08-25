using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Spells/Test Spell")]
public class TestSpell : SpellItem
{
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

    public override void InstantiateSpellCastWarmUpFX(PlayerManager player)
    {
        base.InstantiateSpellCastWarmUpFX(player);

        Debug.Log("InstantiateSpellCastWarmUpFX");
    }

    public override void SuccessfullyCastSpell(PlayerManager player)
    {
        base.SuccessfullyCastSpell(player);

        Debug.Log("SuccessfullyCastSpell");
    }

    public override bool CanICastSpell(PlayerManager player)
    {
        if (player.isPerformingAction)
        {
            return false;
        }

        if (player.playerNetworkManager.isJumping.Value)
        {
            return false;
        }

        if (player.playerNetworkManager.currentStamina.Value <= 0)
        {
            return false;
        }

        return true;
    }
}
