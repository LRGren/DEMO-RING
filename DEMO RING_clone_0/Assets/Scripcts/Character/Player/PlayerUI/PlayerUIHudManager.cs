using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHudManager : MonoBehaviour
{
    [Header("Stat Bar")]
    [SerializeField] private UI_StatBar healthBar;
    [SerializeField] private UI_StatBar staminaBar;

    [Header("Quick Slots")]
    [SerializeField] private Image rightWeaponQuickSlotUI;
    [SerializeField] private Image leftWeaponQuickSlotUI;

    public void RefreshHUD()
    {
        healthBar.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(true);
        
        staminaBar.gameObject.SetActive(false);
        staminaBar.gameObject.SetActive(true);
    }
    
    public void SetNewHealthValue(int oldValue, int newValue)
    {
        healthBar.SetStat(newValue);
    }

    public void SetMaxHealthValue(int maxHealth)
    {
        healthBar.SetMaxStat(maxHealth);
    }
    
    public void SetNewStaminaValue(float oldValue, float newValue)
    {
        staminaBar.SetStat(Mathf.RoundToInt(newValue));
    }

    public void SetMaxStaminaValue(int maxStamina)
    {
        staminaBar.SetMaxStat(maxStamina);
    }

    public void SetRightWeaponQuickSlot(int weaponID)
    {
        WeaponItem weapon = WorldItemDatabase.Instance.GetWeaponByID(weaponID);

        if(weapon == null)
        {
            Debug.Log("Weapon not found in database for ID: " + weaponID);
            rightWeaponQuickSlotUI.enabled = false;
            rightWeaponQuickSlotUI.sprite = null;
        }

        if (weapon.itemIcon == null)
        {
            Debug.Log("Weapon icon not found for weapon: " + weapon.itemName);
            rightWeaponQuickSlotUI.enabled = false;
            rightWeaponQuickSlotUI.sprite = null;
        }

        rightWeaponQuickSlotUI.sprite = weapon.itemIcon;
        rightWeaponQuickSlotUI.enabled = true;
    }

    public void SetLeftWeaponQuickSlot(int weaponID)
    {
        WeaponItem weapon = WorldItemDatabase.Instance.GetWeaponByID(weaponID);

        if (weapon == null)
        {
            Debug.Log("Weapon not found in database for ID: " + weaponID);
            leftWeaponQuickSlotUI.enabled = false;
            leftWeaponQuickSlotUI.sprite = null;
        }

        if (weapon.itemIcon == null)
        {
            Debug.Log("Weapon icon not found for weapon: " + weapon.itemName);
            leftWeaponQuickSlotUI.enabled = false;
            leftWeaponQuickSlotUI.sprite = null;
        }

        leftWeaponQuickSlotUI.sprite = weapon.itemIcon;
        leftWeaponQuickSlotUI.enabled = true;
    }
}
