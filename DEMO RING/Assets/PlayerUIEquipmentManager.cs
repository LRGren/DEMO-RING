using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerUIEquipmentManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menu;

    [Header("Equipment Slots")]
    [SerializeField] private Image rightWeaponSlot01;
    [SerializeField] private Image rightWeaponSlot02;
    [SerializeField] private Image rightWeaponSlot03;
    [SerializeField] private Image leftWeaponSlot01;
    [SerializeField] private Image leftWeaponSlot02;
    [SerializeField] private Image leftWeaponSlot03;

    [SerializeField] private Image headSlot;
    [SerializeField] private Image bodySlot;
    [SerializeField] private Image legsSlot;
    [SerializeField] private Image handSlot;

    [Header("Equipment Inventory Slots")]
    public GameObject equipmentInventoryWindow;
    public EquipmentType currentSelectedEquipmentType;
    public GameObject equipmentInventorySlotPrefab;
    public Transform equipmentInventoryContentWindow;
    public Item currentlySelectedItem;

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
                    player.playerInventoryManager.weaponsInRightHand[0] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }

                if (player.playerInventoryManager.rightWeaponIndex == 0)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.RightWeapon02:
                unequippedWeapon = player.playerInventoryManager.weaponsInRightHand[1];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInRightHand[1] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.rightWeaponIndex == 1)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.RightWeapon03:
                unequippedWeapon = player.playerInventoryManager.weaponsInRightHand[2];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInRightHand[2] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.rightWeaponIndex == 2)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.LeftWeapon01:
                unequippedWeapon = player.playerInventoryManager.weaponsInLeftHand[0];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInLeftHand[0] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.leftWeaponIndex == 0)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.LeftWeapon02:
                unequippedWeapon = player.playerInventoryManager.weaponsInLeftHand[1];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInLeftHand[1] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.leftWeaponIndex == 1)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
                }
                break;
            case EquipmentType.LeftWeapon03:
                unequippedWeapon = player.playerInventoryManager.weaponsInLeftHand[2];
                if (unequippedWeapon != null)
                {
                    player.playerInventoryManager.weaponsInLeftHand[2] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                    if (unequippedWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(unequippedWeapon);
                    }
                }
                if (player.playerInventoryManager.leftWeaponIndex == 2)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
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
