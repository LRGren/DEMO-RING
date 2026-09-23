using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerUIEquipmentManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menu;

    [Header("Equipment Slots")]
    [Header("Right Hand Weapon Slots")]
    [SerializeField] Image rightWeaponSlot01;
    [SerializeField] Image rightWeaponSlot02;
    [SerializeField] Image rightWeaponSlot03;

    [Header("Left Hand Weapon Slots")]
    [SerializeField] Image leftWeaponSlot01;
    [SerializeField] Image leftWeaponSlot02;
    [SerializeField] Image leftWeaponSlot03;

    [Header("Armor Slots")]
    [SerializeField] Image headSlot;
    [SerializeField] Image bodySlot;
    [SerializeField] Image legsSlot;
    [SerializeField] Image handSlot;

    [Header("Projectile Slots")]
    [SerializeField] Image MainProjectile;
    [SerializeField] TextMeshProUGUI MainProjectileCount;
    [SerializeField] Image SecondaryProjectile;
    [SerializeField] TextMeshProUGUI SecondaryProjectileCount;

    [Header("Quick Slots")]
    [SerializeField] Image[] quickSlots;
    [SerializeField] TextMeshProUGUI[] quickSlotCounts;

    [Header("Equipment Inventory Slots")]
    public GameObject equipmentInventoryWindow;
    public EquipmentType currentSelectedEquipmentType;
    public GameObject equipmentInventorySlotPrefab;
    public Transform equipmentInventoryContentWindow;
    public Item currentlySelectedItem;

    void Awake()
    {

    }

    public void OpenEquipmentMenu()
    {
        menu.SetActive(false);
        PlayerUIManager.instance.menuWindowIsOpen = true;
        menu.SetActive(true);

        equipmentInventoryWindow.SetActive(false);

        RefreshMenu();
    }

    public void RefreshMenu()
    {
        ClearEquipmentInventoryWindow();
        RefreshEquipmentSlotIcons();
    }

    public void SetMainProjectileCountText(int count, bool status = true)
    {
        MainProjectileCount.enabled = status;
        MainProjectileCount.text = count.ToString();
    }

    public void SetSecondaryProjectileCountText(int count, bool status = true)
    {
        SecondaryProjectileCount.enabled = status;
        SecondaryProjectileCount.text = count.ToString();
    }

    public void SetQuickSlotCountText(EquipmentType equipmentType, int count, bool status = true)
    {
        int slotIndex = (int)equipmentType - (int)EquipmentType.QuickSlot01;

        quickSlotCounts[slotIndex].enabled = status;
        quickSlotCounts[slotIndex].text = count.ToString();
    }

    public void SelectLastSelectedEquipmentSlot()
    {
        Button buttonToSelect = null;
        switch (currentSelectedEquipmentType)
        {
            case EquipmentType.RightWeapon01:
                buttonToSelect = rightWeaponSlot01.GetComponentInParent<Button>();
                break;
            case EquipmentType.RightWeapon02:
                buttonToSelect = rightWeaponSlot02.GetComponentInParent<Button>();
                break;
            case EquipmentType.RightWeapon03:
                buttonToSelect = rightWeaponSlot03.GetComponentInParent<Button>();
                break;
            case EquipmentType.LeftWeapon01:
                buttonToSelect = leftWeaponSlot01.GetComponentInParent<Button>();
                break;
            case EquipmentType.LeftWeapon02:
                buttonToSelect = leftWeaponSlot02.GetComponentInParent<Button>();
                break;
            case EquipmentType.LeftWeapon03:
                buttonToSelect = leftWeaponSlot03.GetComponentInParent<Button>();
                break;
            case EquipmentType.Head:
                buttonToSelect = headSlot.GetComponentInParent<Button>();
                break;
            case EquipmentType.Body:
                buttonToSelect = bodySlot.GetComponentInParent<Button>();
                break;
            case EquipmentType.Hands:
                buttonToSelect = handSlot.GetComponentInParent<Button>();
                break;
            case EquipmentType.Legs:
                buttonToSelect = legsSlot.GetComponentInParent<Button>();
                break;
            case EquipmentType.MainProjectile:
                buttonToSelect = MainProjectile.GetComponentInParent<Button>();
                break;
            case EquipmentType.SecondaryProjectile:
                buttonToSelect = SecondaryProjectile.GetComponentInParent<Button>();
                break;
            case EquipmentType.QuickSlot01:
            case EquipmentType.QuickSlot02:
            case EquipmentType.QuickSlot03:
            case EquipmentType.QuickSlot04:
            case EquipmentType.QuickSlot05:
            case EquipmentType.QuickSlot06:
            case EquipmentType.QuickSlot07:
            case EquipmentType.QuickSlot08:
            case EquipmentType.QuickSlot09:
            case EquipmentType.QuickSlot10:
                int quickSlotIndex = (int)currentSelectedEquipmentType - (int)EquipmentType.QuickSlot01;
                buttonToSelect = quickSlots[quickSlotIndex].GetComponentInParent<Button>();
                break;
        }

        if (buttonToSelect != null)
        {
            RefreshMenu();
            buttonToSelect.Select();
            buttonToSelect.OnSelect(null);
        }
    }

    public void CloseEquipmentMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = false;
        menu.SetActive(false);
    }

    public void CloseEquipmentInventoryWindow()
    {
        equipmentInventoryWindow.SetActive(false);
    }

    public void RefreshEquipmentSlotIcons()
    {
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        if (player == null)
        {
            Debug.LogError("PlayerManager not found for the local player.");
            return;
        }

        WeaponItem rightHandWeapon01 = player.playerInventoryManager.weaponsInRightHand[0];
        if (rightHandWeapon01.itemIcon != null)
        {
            rightWeaponSlot01.enabled = rightHandWeapon01.itemIcon != null;
            rightWeaponSlot01.sprite = rightHandWeapon01.itemIcon;
        }
        else
        {
            rightWeaponSlot01.enabled = false;
        }

        WeaponItem rightHandWeapon02 = player.playerInventoryManager.weaponsInRightHand[1];
        if (rightHandWeapon02.itemIcon != null)
        {
            rightWeaponSlot02.enabled = rightHandWeapon02.itemIcon != null;
            rightWeaponSlot02.sprite = rightHandWeapon02.itemIcon;
        }
        else
        {
            rightWeaponSlot02.enabled = false;
        }

        WeaponItem rightHandWeapon03 = player.playerInventoryManager.weaponsInRightHand[2];
        if (rightHandWeapon03.itemIcon != null)
        {
            rightWeaponSlot03.enabled = rightHandWeapon03.itemIcon != null;
            rightWeaponSlot03.sprite = rightHandWeapon03.itemIcon;
        }
        else
        {
            rightWeaponSlot03.enabled = false;
        }

        WeaponItem leftHandWeapon01 = player.playerInventoryManager.weaponsInLeftHand[0];
        if (leftHandWeapon01.itemIcon != null)
        {
            leftWeaponSlot01.enabled = leftHandWeapon01.itemIcon != null;
            leftWeaponSlot01.sprite = leftHandWeapon01.itemIcon;
        }
        else
        {
            leftWeaponSlot01.enabled = false;
        }

        WeaponItem leftHandWeapon02 = player.playerInventoryManager.weaponsInLeftHand[1];
        if (leftHandWeapon02.itemIcon != null)
        {
            leftWeaponSlot02.enabled = leftHandWeapon02.itemIcon != null;
            leftWeaponSlot02.sprite = leftHandWeapon02.itemIcon;
        }
        else
        {
            leftWeaponSlot02.enabled = false;
        }

        WeaponItem leftHandWeapon03 = player.playerInventoryManager.weaponsInLeftHand[2];
        if (leftHandWeapon03.itemIcon != null)
        {
            leftWeaponSlot03.enabled = leftHandWeapon03.itemIcon != null;
            leftWeaponSlot03.sprite = leftHandWeapon03.itemIcon;
        }
        else
        {
            leftWeaponSlot03.enabled = false;
        }

        HeadEquipmentItem headEquipment = player.playerInventoryManager.headEquipment;
        if (headEquipment != null)
        {
            headSlot.enabled = headEquipment.itemIcon != null;
            headSlot.sprite = headEquipment.itemIcon;
        }
        else
        {
            headSlot.enabled = false;
        }

        BodyEquipmentItem bodyEquipment = player.playerInventoryManager.bodyEquipment;
        if (bodyEquipment != null)
        {
            bodySlot.enabled = bodyEquipment.itemIcon != null;
            bodySlot.sprite = bodyEquipment.itemIcon;
        }
        else
        {
            bodySlot.enabled = false;
        }

        HandEquipmentItem handEquipment = player.playerInventoryManager.handEquipment;
        if (handEquipment != null)
        {
            handSlot.enabled = handEquipment.itemIcon != null;
            handSlot.sprite = handEquipment.itemIcon;
        }
        else
        {
            handSlot.enabled = false;
        }

        LegEquipmentItem legEquipment = player.playerInventoryManager.legEquipment;
        if (legEquipment != null)
        {
            legsSlot.enabled = legEquipment.itemIcon != null;
            legsSlot.sprite = legEquipment.itemIcon;
        }
        else
        {
            legsSlot.enabled = false;
        }

        RangedProjectileItem mainProjectile = player.playerInventoryManager.mainProjectile;
        if (mainProjectile != null)
        {
            MainProjectile.enabled = mainProjectile.itemIcon != null;
            MainProjectile.sprite = mainProjectile.itemIcon;
            SetMainProjectileCountText(mainProjectile.currentAmmoAmount, mainProjectile.itemIcon != null);
        }
        else
        {
            MainProjectile.enabled = false;
            MainProjectile.sprite = null;
            SetMainProjectileCountText(0, false);
        }

        RangedProjectileItem secondaryProjectile = player.playerInventoryManager.secondaryProjectile;
        if (secondaryProjectile != null)
        {
            SecondaryProjectile.enabled = secondaryProjectile.itemIcon != null;
            SecondaryProjectile.sprite = secondaryProjectile.itemIcon;
            SetSecondaryProjectileCountText(secondaryProjectile.currentAmmoAmount, secondaryProjectile.itemIcon != null);
        }
        else
        {
            SecondaryProjectile.enabled = false;
            SecondaryProjectile.sprite = null;
            SetSecondaryProjectileCountText(0, false);
        }

        foreach (EquipmentType equipmentType in System.Enum.GetValues(typeof(EquipmentType)))
        {
            if (equipmentType >= EquipmentType.QuickSlot01 && equipmentType <= EquipmentType.QuickSlot10)
            {
                int slotIndex = (int)equipmentType - (int)EquipmentType.QuickSlot01;
                QuickSlotItem quickSlotItem = player.playerInventoryManager.quickSlotItemInventory[slotIndex];

                if (quickSlotItem != null)
                {
                    quickSlots[slotIndex].enabled = quickSlotItem.itemIcon != null;
                    quickSlots[slotIndex].sprite = quickSlotItem.itemIcon;
                    SetQuickSlotCountText(equipmentType, quickSlotItem.GetAmountOfItem(player), true);
                }
                else
                {
                    quickSlots[slotIndex].enabled = false;
                    SetQuickSlotCountText(equipmentType, 0, false);
                }
            }
        }
    }

    public void ClearEquipmentInventoryWindow()
    {
        foreach (Transform child in equipmentInventoryContentWindow)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadEquipmentInventorySlots()
    {
        equipmentInventoryWindow.SetActive(true);

        switch (currentSelectedEquipmentType)
        {
            case EquipmentType.RightWeapon01:
            case EquipmentType.RightWeapon02:
            case EquipmentType.RightWeapon03:
            case EquipmentType.LeftWeapon01:
            case EquipmentType.LeftWeapon02:
            case EquipmentType.LeftWeapon03:
                LoadWeaponInventorySlots();
                break;
            case EquipmentType.Head:
                LoadHeadEquipmentInventorySlots();
                break;
            case EquipmentType.Body:
                LoadBodyEquipmentInventorySlots();
                break;
            case EquipmentType.Hands:
                LoadHandEquipmentInventorySlots();
                break;
            case EquipmentType.Legs:
                LoadLegEquipmentInventorySlots();
                break;
            case EquipmentType.MainProjectile:
            case EquipmentType.SecondaryProjectile:
                LoadProjectileEquipmentInventorySlots();
                break;
            case EquipmentType.QuickSlot01:
            case EquipmentType.QuickSlot02:
            case EquipmentType.QuickSlot03:
            case EquipmentType.QuickSlot04:
            case EquipmentType.QuickSlot05:
            case EquipmentType.QuickSlot06:
            case EquipmentType.QuickSlot07:
            case EquipmentType.QuickSlot08:
            case EquipmentType.QuickSlot09:
            case EquipmentType.QuickSlot10:
                LoadQuickSlotInventorySlots();
                break;
            default:
                Debug.LogWarning("Unhandled equipment type: " + currentSelectedEquipmentType);
                break;
        }
    }

    public void LoadWeaponInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<WeaponItem> weaponInventory = new List<WeaponItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            WeaponItem weapon = item as WeaponItem;


            if (weapon != null)
            {
                weaponInventory.Add(weapon);
            }
        }

        if (weaponInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (WeaponItem weapon in weaponInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(weapon);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }

    }

    public void LoadHeadEquipmentInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<HeadEquipmentItem> headEquipmentInventory = new List<HeadEquipmentItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            HeadEquipmentItem headEquipment = item as HeadEquipmentItem;

            if (headEquipment != null)
            {
                headEquipmentInventory.Add(headEquipment);
            }
        }

        if (headEquipmentInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (HeadEquipmentItem headEquipment in headEquipmentInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(headEquipment);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }
    }

    public void LoadBodyEquipmentInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<BodyEquipmentItem> bodyEquipmentInventory = new List<BodyEquipmentItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            BodyEquipmentItem bodyEquipment = item as BodyEquipmentItem;

            if (bodyEquipment != null)
            {
                bodyEquipmentInventory.Add(bodyEquipment);
            }
        }

        if (bodyEquipmentInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (BodyEquipmentItem bodyEquipment in bodyEquipmentInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(bodyEquipment);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }
    }

    public void LoadHandEquipmentInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<HandEquipmentItem> handEquipmentInventory = new List<HandEquipmentItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            HandEquipmentItem handEquipment = item as HandEquipmentItem;

            if (handEquipment != null)
            {
                handEquipmentInventory.Add(handEquipment);
            }
        }

        if (handEquipmentInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (HandEquipmentItem handEquipment in handEquipmentInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(handEquipment);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }
    }

    public void LoadLegEquipmentInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<LegEquipmentItem> legEquipmentInventory = new List<LegEquipmentItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            LegEquipmentItem legEquipment = item as LegEquipmentItem;

            if (legEquipment != null)
            {
                legEquipmentInventory.Add(legEquipment);
            }
        }

        if (legEquipmentInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (LegEquipmentItem legEquipment in legEquipmentInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(legEquipment);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }
    }

    public void LoadProjectileEquipmentInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<RangedProjectileItem> projectileInventory = new List<RangedProjectileItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            RangedProjectileItem projectile = item as RangedProjectileItem;


            if (projectile != null)
            {
                projectileInventory.Add(projectile);
            }
        }

        if (projectileInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (RangedProjectileItem projectile in projectileInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(projectile);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }

    }


    public void LoadQuickSlotInventorySlots()
    {
        ClearEquipmentInventoryWindow();

        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        List<QuickSlotItem> quickSlotsInventory = new List<QuickSlotItem>();

        foreach (Item item in player.playerInventoryManager.characterInventory)
        {
            QuickSlotItem quickSlotsItem = item as QuickSlotItem;


            if (quickSlotsItem != null)
            {
                quickSlotsInventory.Add(quickSlotsItem);
            }
        }

        if (quickSlotsInventory.Count <= 0)
        {
            RefreshMenu();
            return;
        }

        bool hasFirstSlotBeenSelected = false;

        foreach (QuickSlotItem quickSlotsItem in quickSlotsInventory)
        {
            GameObject slot = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
            UI_EquipmentInventorySlot inventorySlot = slot.GetComponent<UI_EquipmentInventorySlot>();
            inventorySlot.AddItem(quickSlotsItem);

            if (!hasFirstSlotBeenSelected)
            {
                Button button = slot.GetComponent<Button>();
                button.Select();
                button.OnSelect(null);

                hasFirstSlotBeenSelected = true;
            }
        }
    }

    public void SelectEquipmentSlot(int slotIndex)
    {
        currentSelectedEquipmentType = (EquipmentType)slotIndex;
    }

    public void UnequipItem()
    {
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        Item unequippedWeapon;

        switch (PlayerUIManager.instance.playerUIEquipmentManager.currentSelectedEquipmentType)
        {
            case EquipmentType.RightWeapon01:
                unequippedWeapon = player.playerInventoryManager.weaponsInRightHand[0];

                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInRightHand[0] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }

                if (player.playerInventoryManager.rightWeaponIndex == 0)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.RightWeapon02:
                unequippedWeapon = player.playerInventoryManager.weaponsInRightHand[1];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInRightHand[1] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.rightWeaponIndex == 1)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.RightWeapon03:
                unequippedWeapon = player.playerInventoryManager.weaponsInRightHand[2];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInRightHand[2] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.rightWeaponIndex == 2)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.LeftWeapon01:
                unequippedWeapon = player.playerInventoryManager.weaponsInLeftHand[0];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInLeftHand[0] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.leftWeaponIndex == 0)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.LeftWeapon02:
                unequippedWeapon = player.playerInventoryManager.weaponsInLeftHand[1];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInLeftHand[1] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.leftWeaponIndex == 1)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.LeftWeapon03:
                unequippedWeapon = player.playerInventoryManager.weaponsInLeftHand[2];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInLeftHand[2] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.leftWeaponIndex == 2)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.Head:
                HeadEquipmentItem unequippedHeadEquipment = player.playerInventoryManager.headEquipment;
                if (unequippedHeadEquipment != null)
                {
                    player.playerInventoryManager.headEquipment = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedHeadEquipment);
                }
                player.playerEquipmentManager.LoadHeadEquipment(null);
                break;
            case EquipmentType.Body:
                BodyEquipmentItem unequippedBodyEquipment = player.playerInventoryManager.bodyEquipment;
                if (unequippedBodyEquipment != null)
                {
                    player.playerInventoryManager.bodyEquipment = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedBodyEquipment);
                }
                player.playerEquipmentManager.LoadBodyEquipment(null);
                break;
            case EquipmentType.Hands:
                HandEquipmentItem unequippedHandEquipment = player.playerInventoryManager.handEquipment;
                if (unequippedHandEquipment != null)
                {
                    player.playerInventoryManager.handEquipment = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedHandEquipment);
                }
                player.playerEquipmentManager.LoadHandEquipment(null);
                break;
            case EquipmentType.Legs:
                LegEquipmentItem unequippedLegEquipment = player.playerInventoryManager.legEquipment;
                if (unequippedLegEquipment != null)
                {
                    player.playerInventoryManager.legEquipment = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedLegEquipment);
                }
                player.playerEquipmentManager.LoadLegEquipment(null);
                break;
            case EquipmentType.MainProjectile:
                RangedProjectileItem unequippedMainProjectile = player.playerInventoryManager.mainProjectile;
                if (unequippedMainProjectile != null)
                {
                    player.playerInventoryManager.mainProjectile = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedMainProjectile);
                }
                SetMainProjectileCountText(0, false);
                player.playerEquipmentManager.LoadMainProjectileEquipment(null);
                break;
            case EquipmentType.SecondaryProjectile:
                RangedProjectileItem unequippedSecondaryProjectile = player.playerInventoryManager.secondaryProjectile;
                if (unequippedSecondaryProjectile != null)
                {
                    player.playerInventoryManager.secondaryProjectile = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedSecondaryProjectile);
                }
                SetSecondaryProjectileCountText(0, false);
                player.playerEquipmentManager.LoadSecondaryProjectileEquipment(null);
                break;
            case EquipmentType.QuickSlot01:
            case EquipmentType.QuickSlot02:
            case EquipmentType.QuickSlot03:
            case EquipmentType.QuickSlot04:
            case EquipmentType.QuickSlot05:
            case EquipmentType.QuickSlot06:
            case EquipmentType.QuickSlot07:
            case EquipmentType.QuickSlot08:
            case EquipmentType.QuickSlot09:
            case EquipmentType.QuickSlot10:
                int quickSlotIndex = (int)currentSelectedEquipmentType - (int)EquipmentType.QuickSlot01;
                QuickSlotItem unequippedQuickSlotItem = player.playerInventoryManager.quickSlotItemInventory[quickSlotIndex];
                if (unequippedQuickSlotItem != null)
                {
                    player.playerInventoryManager.quickSlotItemInventory[quickSlotIndex] = null;

                    player.playerInventoryManager.AddItemToInventory(unequippedQuickSlotItem);
                }

                SetQuickSlotCountText(currentSelectedEquipmentType, 0, false);

                if (player.playerInventoryManager.currentQuickSlotItemIndex == quickSlotIndex)
                {
                    player.playerNetworkManager.currentQuickSlotItemID.Value = -1;
                }
                player.playerInventoryManager.SetItemInQuickSlot(null, quickSlotIndex);
                break;
        }

        RefreshMenu();

        if (equipmentInventoryWindow.activeSelf)
        {
            LoadEquipmentInventorySlots();
        }

    }

    public void FromEquipmentInventoryWindowBackToEquipmentMenu()
    {
        equipmentInventoryWindow.SetActive(false);
        RefreshMenu();
        SelectLastSelectedEquipmentSlot();
    }


}
