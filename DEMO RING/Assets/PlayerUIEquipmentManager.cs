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

    public void OpenEquipmentMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = true;
        menu.SetActive(true);

        RefreshWeaponSlotIcons();
    }

    public void CloseEquipmentMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = false;
        menu.SetActive(false);
    }

    public void RefreshWeaponSlotIcons()
    {
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        if (player == null)
        {
            Debug.LogError("PlayerManager not found for the local player.");
            return;
        }

        WeaponItem rightHandWeapon01 = player.playerInventoryManager.weaponsInRightHand[0];
        if (rightHandWeapon01 != null && rightHandWeapon01.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
        {
            rightWeaponSlot01.enabled = true;
            rightWeaponSlot01.sprite = rightHandWeapon01.itemIcon;
        }
        else
        {
            rightWeaponSlot01.enabled = false;
        }

        WeaponItem rightHandWeapon02 = player.playerInventoryManager.weaponsInRightHand[1];
        if (rightHandWeapon02 != null && rightHandWeapon02.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
        {
            rightWeaponSlot02.enabled = true;
            rightWeaponSlot02.sprite = rightHandWeapon02.itemIcon;
        }
        else
        {
            rightWeaponSlot02.enabled = false;
        }

        WeaponItem rightHandWeapon03 = player.playerInventoryManager.weaponsInRightHand[2];
        if (rightHandWeapon03 != null && rightHandWeapon03.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
        {
            rightWeaponSlot03.enabled = true;
            rightWeaponSlot03.sprite = rightHandWeapon03.itemIcon;
        }
        else
        {
            rightWeaponSlot03.enabled = false;
        }

        WeaponItem leftHandWeapon01 = player.playerInventoryManager.weaponsInLeftHand[0];
        if (leftHandWeapon01 != null && leftHandWeapon01.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
        {
            leftWeaponSlot01.enabled = true;
            leftWeaponSlot01.sprite = leftHandWeapon01.itemIcon;
        }
        else
        {
            leftWeaponSlot01.enabled = false;
        }

        WeaponItem leftHandWeapon02 = player.playerInventoryManager.weaponsInLeftHand[1];
        if (leftHandWeapon02 != null && leftHandWeapon02.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
        {
            leftWeaponSlot02.enabled = true;
            leftWeaponSlot02.sprite = leftHandWeapon02.itemIcon;
        }
        else
        {
            leftWeaponSlot02.enabled = false;
        }

        WeaponItem leftHandWeapon03 = player.playerInventoryManager.weaponsInLeftHand[2];
        if (leftHandWeapon03 != null && leftHandWeapon03.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
        {
            leftWeaponSlot03.enabled = true;
            leftWeaponSlot03.sprite = leftHandWeapon03.itemIcon;
        }
        else
        {
            leftWeaponSlot03.enabled = false;
        }
    }

}
