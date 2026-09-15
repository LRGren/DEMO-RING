using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellItem : Item
{
    [Header("Spell Class")]
    public SpellClass spellClass;

    [Header("Spell Modifiers")]
    public float fullCastStaminaCostModifier = 1.5f;

    [Header("Spell Cost")]
    public int staminaCost = 10;
    public int focusCost = 10;

    [Header("Spell FX")]
    public GameObject spellCastWarmUpFX;
    public GameObject spellChargeFX;
    public GameObject spellCastReleaseFX;
    public GameObject spellCastReleaseFXFullCharge;

    [Header("Spell Animations")]
    public string mainHandSpellAnimtion;
    public string offHandSpellAnimtion;

    [Header("Spell Sound FX")]
    public AudioClip spellCastWarmUpSound;
    public AudioClip spellCastReleaseSound;

    public virtual void AttemptToCastSpell(PlayerManager player)
    {
    }

    public virtual void InstantiateSpellCastWarmUpFX(PlayerManager player)
    {

    }

    public virtual void SuccessfullyCastSpell(PlayerManager player)
    {
        if (player.IsOwner)
        {
            player.playerNetworkManager.currentStamina.Value -= staminaCost;
            player.playerNetworkManager.currentFocusPoints.Value -= focusCost;
        }
    }

    public virtual void SuccessfullyCastSpellFull(PlayerManager player)
    {
        if (player.IsOwner)
        {
            player.playerNetworkManager.currentStamina.Value -= Mathf.RoundToInt(staminaCost * fullCastStaminaCostModifier);
            player.playerNetworkManager.currentFocusPoints.Value -= focusCost;
        }
    }

    public virtual void SuccessfullyChargeSpell(PlayerManager player)
    {

    }

    public virtual bool CanICastSpell(PlayerManager player)
    {
        if (player.playerNetworkManager.currentStamina.Value <= 0)
            return false;

        if (player.playerNetworkManager.currentFocusPoints.Value < focusCost)
            return false;

        if (player.isPerformingAction)
        {
            return false;
        }

        if (player.playerNetworkManager.isJumping.Value)
        {
            return false;
        }

        return true;
    }

}
