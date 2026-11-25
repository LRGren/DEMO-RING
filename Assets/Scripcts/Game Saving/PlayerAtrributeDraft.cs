using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAtrributeDraft
{
    public int level;
    public int vigor;
    public int mind;
    public int endurance;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int faith;
    public int arcane;

    public PlayerAtrributeDraft(PlayerAtrributeDraft other)
    {
        level = other.level;
        vigor = other.vigor;
        mind = other.mind;
        endurance = other.endurance;
        strength = other.strength;
        dexterity = other.dexterity;
        intelligence = other.intelligence;
        faith = other.faith;
        arcane = other.arcane;
    }

    public PlayerAtrributeDraft(int level, int vigor, int mind, int endurance, int strength, int dexterity, int intelligence, int faith, int arcane)
    {
        this.level = level;
        this.vigor = vigor;
        this.mind = mind;
        this.endurance = endurance;
        this.strength = strength;
        this.dexterity = dexterity;
        this.intelligence = intelligence;
        this.faith = faith;
        this.arcane = arcane;
    }
}
