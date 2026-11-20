using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDamageCollider : DamageCollider
{
    public CharacterManager spellCaster;

    protected override CharacterManager GetAttackingCharacter()
    {
        return spellCaster;
    }
}
