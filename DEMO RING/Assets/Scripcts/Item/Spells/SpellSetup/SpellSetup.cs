using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellSetup", menuName = "SpellSetUp", order = 1)]
public class SpellSetup : ScriptableObject
{
    public string spellName;

    [Header("Spell Info")]
    public int physicalDamage;
    public int fireDamage;
    public int magicalDamage;
    public int lightningDamage;
    public int holyDamage;

    public int poiseDamage;

    public float fullChargeMultiplier = 1.5f;
    public bool useGravity = false;
}
