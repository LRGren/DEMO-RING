using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellItem : Item
{
    [Header("Spell Class")]
    public SpellClass spellClass;

    [Header("Spell Modifiers")]
    public float fullChargeModifier = 2f;

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

    }

    public virtual void SuccessfullyCastSpellFull(PlayerManager player)
    {

    }

    public virtual void SuccessfullyChargeSpell(PlayerManager player)
    {

    }

    public virtual bool CanICastSpell(PlayerManager player)
    {
        return true;
    }

}
