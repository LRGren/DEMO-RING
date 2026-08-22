using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UI_EquipmentInventorySlot : MonoBehaviour
{
    public Image itemIcon;
    public Image highlightIcon;
    public Item currentItem;

    public void AddItem(Item newItem)
    {
        if (newItem == null)
        {
            itemIcon.enabled = false;
            return;
        }

        itemIcon.enabled = true;
        itemIcon.sprite = newItem.itemIcon;
        currentItem = newItem;
    }

    public void SelectSlot()
    {
        highlightIcon.enabled = true;
    }

    public void DeselectSlot()
    {
        highlightIcon.enabled = false;
    }

    public void EquipItem()
    {
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        Item equipedItem;

        switch (PlayerUIManager.instance.playerUIEquipmentManager.currentSelectedEquipmentType)
        {
            case EquipmentType.RightWeapon01:
                equipedItem = player.playerInventoryManager.weaponsInRightHand[0];

                if (equipedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    player.playerInventoryManager.AddItemToInventory(equipedItem);
                }

                player.playerInventoryManager.weaponsInRightHand[0] = currentItem as WeaponItem;

                player.playerInventoryManager.RemoveItemFromInventory(currentItem);

                if (player.playerInventoryManager.rightWeaponIndex == 0)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = currentItem.itemID;
                }
                break;
            case EquipmentType.RightWeapon02:
                equipedItem = player.playerInventoryManager.weaponsInRightHand[1];
                if (equipedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    player.playerInventoryManager.AddItemToInventory(equipedItem);
                }

                player.playerInventoryManager.weaponsInRightHand[1] = currentItem as WeaponItem;

                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                if (player.playerInventoryManager.rightWeaponIndex == 1)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = currentItem.itemID;
                }
                break;
            case EquipmentType.RightWeapon03:
                equipedItem = player.playerInventoryManager.weaponsInRightHand[2];
                if (equipedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    player.playerInventoryManager.AddItemToInventory(equipedItem);
                }
                player.playerInventoryManager.weaponsInRightHand[2] = currentItem as WeaponItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                if (player.playerInventoryManager.rightWeaponIndex == 2)
                {
                    player.playerNetworkManager.currentRightHandWeaponID.Value = currentItem.itemID;
                }
                break;
            case EquipmentType.LeftWeapon01:
                equipedItem = player.playerInventoryManager.weaponsInLeftHand[0];
                if (equipedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    player.playerInventoryManager.AddItemToInventory(equipedItem);
                }
                player.playerInventoryManager.weaponsInLeftHand[0] = currentItem as WeaponItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                if (player.playerInventoryManager.leftWeaponIndex == 0)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = currentItem.itemID;
                }

                break;
            case EquipmentType.LeftWeapon02:
                equipedItem = player.playerInventoryManager.weaponsInLeftHand[1];
                if (equipedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    player.playerInventoryManager.AddItemToInventory(equipedItem);
                }
                player.playerInventoryManager.weaponsInLeftHand[1] = currentItem as WeaponItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                if (player.playerInventoryManager.leftWeaponIndex == 1)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = currentItem.itemID;
                }
                break;
            case EquipmentType.LeftWeapon03:
                equipedItem = player.playerInventoryManager.weaponsInLeftHand[2];
                if (equipedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    player.playerInventoryManager.AddItemToInventory(equipedItem);
                }
                player.playerInventoryManager.weaponsInLeftHand[2] = currentItem as WeaponItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                if (player.playerInventoryManager.leftWeaponIndex == 2)
                {
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = currentItem.itemID;
                }
                break;

            case EquipmentType.Head:
                HeadEquipmentItem currentHeadEquipment = player.playerInventoryManager.headEquipment;
                if (currentHeadEquipment != null)
                {
                    player.playerInventoryManager.AddItemToInventory(currentHeadEquipment);
                }

                player.playerInventoryManager.headEquipment = currentItem as HeadEquipmentItem;

                player.playerInventoryManager.RemoveItemFromInventory(currentItem);

                player.playerEquipmentManager.LoadHeadEquipment(player.playerInventoryManager.headEquipment);
                break;

            case EquipmentType.Body:
                BodyEquipmentItem currentBodyEquipment = player.playerInventoryManager.bodyEquipment;
                if (currentBodyEquipment != null)
                {
                    player.playerInventoryManager.AddItemToInventory(currentBodyEquipment);
                }
                player.playerInventoryManager.bodyEquipment = currentItem as BodyEquipmentItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                player.playerEquipmentManager.LoadBodyEquipment(player.playerInventoryManager.bodyEquipment);
                break;
            case EquipmentType.Legs:
                LegEquipmentItem currentLegEquipment = player.playerInventoryManager.legEquipment;
                if (currentLegEquipment != null)
                {
                    player.playerInventoryManager.AddItemToInventory(currentLegEquipment);
                }
                player.playerInventoryManager.legEquipment = currentItem as LegEquipmentItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                player.playerEquipmentManager.LoadLegEquipment(player.playerInventoryManager.legEquipment);
                break;
            case EquipmentType.Hands:
                HandEquipmentItem currentHandEquipment = player.playerInventoryManager.handEquipment;
                if (currentHandEquipment != null)
                {
                    player.playerInventoryManager.AddItemToInventory(currentHandEquipment);
                }
                player.playerInventoryManager.handEquipment = currentItem as HandEquipmentItem;
                player.playerInventoryManager.RemoveItemFromInventory(currentItem);
                player.playerEquipmentManager.LoadHandEquipment(player.playerInventoryManager.handEquipment);
                break;

        }

        PlayerUIManager.instance.playerUIEquipmentManager.RefreshMenu();
        PlayerUIManager.instance.playerUIEquipmentManager.CloseEquipmentInventoryWindow();


        PlayerUIManager.instance.playerUIEquipmentManager.SelectLastSelectedEquipmentSlot();
    }

}
