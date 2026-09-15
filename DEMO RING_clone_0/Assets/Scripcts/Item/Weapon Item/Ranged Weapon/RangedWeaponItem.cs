using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapons/Ranged Weapon")]
public class RangedWeaponItem : WeaponItem
{
    [Header("Sounds")]
    public AudioClip[] bowDrawSounds;
    public AudioClip[] bowReleaseSounds;
}
