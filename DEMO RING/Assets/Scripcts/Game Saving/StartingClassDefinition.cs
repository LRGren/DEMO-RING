using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Starting Class Definition")]
public class StartingClassDefinition : ScriptableObject
{
    [Header("Starting Class Information")]
    [Header("Class Name")]
    public string className;

    [Header("Starting Stats")]
    public int level;
    public int vigor;
    public int mind;
    public int endurance;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int faith;
    public int arcane;

    [Header("Starting Weapon")]
    public WeaponItem startingRightWeapon01;
    public WeaponItem startingRightWeapon02;
    public WeaponItem startingRightWeapon03;
    public WeaponItem startingLeftWeapon01;
    public WeaponItem startingLeftWeapon02;
    public WeaponItem startingLeftWeapon03;

    [Header("Starting Equipment")]
    public HeadEquipmentItem startingHeadEquipment;
    public BodyEquipmentItem startingBodyEquipment;
    public HandEquipmentItem startingHandEquipment;
    public LegEquipmentItem startingLegEquipment;

    [Header("Starting QuickSlotItems")]
    public QuickSlotItem startingQuickSlotItem01;
    public QuickSlotItem startingQuickSlotItem02;
    public QuickSlotItem startingQuickSlotItem03;
    public QuickSlotItem startingQuickSlotItem04;
    public QuickSlotItem startingQuickSlotItem05;
    public QuickSlotItem startingQuickSlotItem06;
    public QuickSlotItem startingQuickSlotItem07;
    public QuickSlotItem startingQuickSlotItem08;
    public QuickSlotItem startingQuickSlotItem09;
    public QuickSlotItem startingQuickSlotItem10;

    [Header("Starting Ranged Projectiles")]
    public RangedProjectileItem startingMainRangedProjectile;
    public int startingMainRangedProjectileAmount;
    public RangedProjectileItem startingSecondaryRangedProjectile;
    public int startingSecondaryRangedProjectileAmount;

    [Header("Starting Spells")]
    public SpellItem startingSpell01;

    [Header("Starting Consumables")]
    public int startingHealthFlaskCount;
    public int startingManaFlaskCount;
}
