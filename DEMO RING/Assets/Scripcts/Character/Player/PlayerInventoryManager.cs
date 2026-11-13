using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : CharacterInventoryManager
{
    PlayerManager player;

    [Header("Weapon Slots")]
    public WeaponItem currentRightHandWeapon;
    public WeaponItem currentLeftHandWeapon;
    public WeaponItem currentTwoHandedWeapon;

    [Header("Quick Slots")]
    public WeaponItem[] weaponsInRightHand = new WeaponItem[3];
    public int rightWeaponIndex = 0;
    public WeaponItem[] weaponsInLeftHand = new WeaponItem[3];
    public int leftWeaponIndex = 0;
    public SpellItem currentSpell;

    [Header("Equipment Slots")]
    public HeadEquipmentItem headEquipment;
    public BodyEquipmentItem bodyEquipment;
    public HandEquipmentItem handEquipment;
    public LegEquipmentItem legEquipment;

    [Header("Projectile Slots")]
    public RangedProjectileItem mainProjectile;
    public RangedProjectileItem secondaryProjectile;

    [Header("Quick Slot Items")]
    public QuickSlotItem currentQuickSlotItem;
    public int currentQuickSlotItemIndex = 0;
    public QuickSlotItem[] quickSlotItemInventory = new QuickSlotItem[10];

    [Header("Inventory")]
    public List<Item> characterInventory = new List<Item>();

    [Header("Test Item")]
    public Item testItem;
    public bool testItemAdded = false;

    //自己添加的
    override protected void Awake()
    {
        base.Awake();
        player = GetComponent<PlayerManager>();
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


    // Quick Slot Item Management
    public void SetItemInQuickSlot(QuickSlotItem item, int slotIndex)
    {
        quickSlotItemInventory[slotIndex] = item;

        if (item == null)
        {
            PlayerUIManager.instance.playerUIEquipmentManager.SetQuickSlotCountText(EquipmentType.QuickSlot01 + slotIndex, 0, false);
            return;
        }

        if (item.isConsumable)
        {
            // Debug.Log("Current Quick Slot Item: " + item.itemName + ", Amount: " + item.GetAmountOfItem(player));
            PlayerUIManager.instance.playerUIEquipmentManager.SetQuickSlotCountText(EquipmentType.QuickSlot01 + slotIndex, item.GetAmountOfItem(player), true);
        }
    }
    public void RestoreQuickSlot(SerializableQuickSlotItem serializableQuickSlotItem, int slotIndex)
    {
        QuickSlotItem quickSlotItem = serializableQuickSlotItem != null ? serializableQuickSlotItem.GetQuickSlotItem() : null;

        if (quickSlotItem != null)
        {
            quickSlotItem.SetAmountOfItem(player, serializableQuickSlotItem.itemCount);
        }

        SetItemInQuickSlot(quickSlotItem, slotIndex);
    }

    void Update()
    {
        if (testItem != null && testItemAdded)
        {
            AddItemToInventory(testItem);
            testItemAdded = false;
        }
    }
}
