using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

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

    [Header("Spells")]
    public int currentMaxSpellCapacity = 2;
    public SpellItem currentSpell;
    public int currentSpellIndex = 0;
    public SpellItem[] spellInventory = new SpellItem[10];

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
    public void AddItemToInventory(Item item, int itemCount = 0)
    {
        if (item == null)
            return;

        bool stacked = itemCount > 0 &&
        (
            TryStackInventoryItemAndQuickSlotItems(item, itemCount)
            || TryStackRangedProjectileItem(item, itemCount)
        );

        if (!stacked)
        {
            var addedItem = Instantiate(item);

            if (itemCount > 0)
            {
                if (addedItem is QuickSlotItem quickSlotItem &&
                    quickSlotItem.isConsumable &&
                    !(quickSlotItem is FlaskItem))
                {
                    quickSlotItem.SetAmountOfItem(player, itemCount);
                }
                else if (addedItem is RangedProjectileItem ammo)
                {
                    ammo.currentAmmoAmount = Mathf.Min(itemCount, ammo.maxAmmoAmount);
                }
            }

            characterInventory.Add(addedItem);
        }

        PlayerUIManager.instance.playerUIHudManager.RefreshHUD();
    }
    private bool TryStackInventoryItemAndQuickSlotItems(Item item, int itemCount)
    {
        // 检查背包中是否已经存在相同的物品，如果存在则尝试堆叠
        var itemInInventory = characterInventory.Find(i => i != null && i.itemID == item.itemID);
        if (itemInInventory != null && itemInInventory is QuickSlotItem quickSlotItem)
        {
            quickSlotItem.SetAmountOfItem(player, quickSlotItem.GetAmountOfItem(player) + itemCount);
            return true;
        }

        // 检查快捷栏中是否已经存在相同的物品，如果存在则尝试堆叠
        itemInInventory = quickSlotItemInventory.ToList().Find(i => i != null && i.itemID == item.itemID);
        if (itemInInventory != null && itemInInventory is QuickSlotItem quickSlotItem2)
        {
            quickSlotItem2.SetAmountOfItem(player, quickSlotItem2.GetAmountOfItem(player) + itemCount);
            PlayerUIManager.instance.playerUIHudManager.RefreshHUDFromPlayer(player);
            return true;
        }

        return false;
    }

    private bool TryStackRangedProjectileItem(Item item, int itemCount)
    {
        var itemInInventory = mainProjectile != null && mainProjectile.itemID == item.itemID ? mainProjectile : null;
        if (itemInInventory != null && itemInInventory is RangedProjectileItem rangedProjectileItem)
        {
            rangedProjectileItem.currentAmmoAmount = Mathf.Min(rangedProjectileItem.currentAmmoAmount + itemCount, rangedProjectileItem.maxAmmoAmount);
            PlayerUIManager.instance.playerUIHudManager.RefreshHUDFromPlayer(player);
            return true;
        }

        itemInInventory = secondaryProjectile != null && secondaryProjectile.itemID == item.itemID ? secondaryProjectile : null;
        if (itemInInventory != null && itemInInventory is RangedProjectileItem rangedProjectileItem2)
        {
            rangedProjectileItem2.currentAmmoAmount = Mathf.Min(rangedProjectileItem2.currentAmmoAmount + itemCount, rangedProjectileItem2.maxAmmoAmount);
            PlayerUIManager.instance.playerUIHudManager.RefreshHUDFromPlayer(player);
            return true;
        }
        return false;
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

    // Spell Management
    public void UpdateMaxSpellCapacity(int newMaxCapacity)
    {
        if (newMaxCapacity < 0)
            return;

        currentMaxSpellCapacity = newMaxCapacity;

        for (int i = currentMaxSpellCapacity; i < spellInventory.Length; i++)
        {
            spellInventory[i] = null;
        }
    }
    public void SetSpellInInventory(SpellItem spell, int index)
    {
        if (spell == null)
        {
            AddItemToInventory(spellInventory[index]);
            spellInventory[index] = null;
            if (index == currentSpellIndex)
            {
                player.playerNetworkManager.currentSpellID.Value = -1;
            }
            return;
        }

        var playerUISpellManager = PlayerUIManager.instance.playerUISpellManager;

        if (playerUISpellManager.hasEquippedSpell(spell))
        {
            int fromIndex = 0;
            for (int i = 0; i < spellInventory.Length; i++)
            {
                if (spellInventory[i] == null)
                    continue;

                if (spellInventory[i].itemID == spell.itemID)
                {
                    fromIndex = i;
                    break;
                }
            }

            var oldSpell = spellInventory[index];
            spellInventory[index] = spell;
            spellInventory[fromIndex] = oldSpell;

            if (fromIndex == currentSpellIndex)
            {
                player.playerNetworkManager.currentSpellID.Value = oldSpell != null ? oldSpell.itemID : -1;
            }

            if (oldSpell == null)
            {
                player.playerEquipmentManager.SwitchSpells();
            }
        }
        else
        {
            var oldSpell = spellInventory[index];
            if (oldSpell != null)
                AddItemToInventory(oldSpell);

            spellInventory[index] = spell;
        }

        if (index == currentSpellIndex)
        {
            player.playerNetworkManager.currentSpellID.Value = spell != null ? spell.itemID : -1;
        }
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
