using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : CharacterInventoryManager
{
    [Header("Weapon Slots")]
    public WeaponItem currentRightHandWeapon;
    public WeaponItem currentLeftHandWeapon;
    public WeaponItem currentTwoHandedWeapon;

    [Header("Quick Slots")]
    public WeaponItem[] weaponsInRightHand = new WeaponItem[3];
    public int rightWeaponIndex = 0;
    public WeaponItem[] weaponsInLeftHand = new WeaponItem[3];
    public int leftWeaponIndex = 0;

    [Header("Equipment Slots")]
    public HeadEquipmentItem headEquipment;
    public BodyEquipmentItem bodyEquipment;
    public HandEquipmentItem handEquipment;
    public LegEquipmentItem legEquipment;

    //自己添加的
    override protected void Awake()
    {
        base.Awake();
    }

}
