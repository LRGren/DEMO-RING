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

    [Header("Inventory")]
    public List<Item> characterInventory = new List<Item>();

    //自己添加的
    override protected void Awake()
    {
        base.Awake();
    }

    public void AddItemToInventory(Item item)
    {
        characterInventory.Add(Instantiate(item));
    }

    public void RemoveItemFromInventory(Item item)
    {
        characterInventory.Remove(item);

        for (int i = characterInventory.Count - 1; i >= 0; i--)
        {
            if (characterInventory[i] == null)
            {
                characterInventory.RemoveAt(i);
            }
        }
    }

}
