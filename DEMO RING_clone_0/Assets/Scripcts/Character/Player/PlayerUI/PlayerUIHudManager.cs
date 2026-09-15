using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHudManager : MonoBehaviour
{
    [Header("Canvas Groups")]
    public CanvasGroup[] canvasGroups;

    [Header("Stat Bar")]
    [SerializeField] private UI_StatBar healthBar;
    [SerializeField] private UI_StatBar staminaBar;
    [SerializeField] private UI_StatBar focusBar;

    [Header("Quick Slots")]
    [SerializeField] private Image rightWeaponQuickSlotUI;
    [SerializeField] private Image leftWeaponQuickSlotUI;
    [SerializeField] private Image spellQuickSlotUI;
    [SerializeField] private Image quickSlotItemUI;

    [Header("Boss HP Bar")]
    public Transform bossHPBarParent;
    public GameObject bossHPBarObject;

    [Header("Crosshair")]
    public GameObject crosshair;

    public void ToggleHUD(bool status)
    {
        if (status)
        {
            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = 1f;
            }
        }
        else
        {
            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }

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

    public void SetNewFocusValue(int oldValue, int newValue)
    {
        focusBar.SetStat(newValue);
    }

    public void SetMaxFocusValue(int maxFocus)
    {
        focusBar.SetMaxStat(maxFocus);
    }

    public void SetRightWeaponQuickSlotIcon(int weaponID)
    {
        WeaponItem weapon = WorldItemDatabase.instance.GetWeaponByID(weaponID);

        if (weapon == null)
        {
            //Debug.Log("Weapon not found in database for ID: " + weaponID);
            rightWeaponQuickSlotUI.enabled = false;
            rightWeaponQuickSlotUI.sprite = null;
            return;
        }

        if (weapon.itemIcon == null)
        {
            //Debug.Log("Weapon icon not found for weapon: " + weapon.itemName);
            rightWeaponQuickSlotUI.enabled = false;
            rightWeaponQuickSlotUI.sprite = null;
            return;
        }

        rightWeaponQuickSlotUI.sprite = weapon.itemIcon;
        rightWeaponQuickSlotUI.enabled = true;
    }

    public void SetLeftWeaponQuickSlotIcon(int weaponID)
    {
        WeaponItem weapon = WorldItemDatabase.instance.GetWeaponByID(weaponID);

        if (weapon == null)
        {
            //Debug.Log("Weapon not found in database for ID: " + weaponID);
            leftWeaponQuickSlotUI.enabled = false;
            leftWeaponQuickSlotUI.sprite = null;
            return;
        }

        if (weapon.itemIcon == null)
        {
            //Debug.Log("Weapon icon not found for weapon: " + weapon.itemName);
            leftWeaponQuickSlotUI.enabled = false;
            leftWeaponQuickSlotUI.sprite = null;
            return;
        }

        leftWeaponQuickSlotUI.sprite = weapon.itemIcon;
        leftWeaponQuickSlotUI.enabled = true;
    }

    public void SetSpellItemQuickSlotIcon(int spellID)
    {
        SpellItem spell = WorldItemDatabase.instance.GetSpellByID(spellID);

        if (spell == null)
        {
            //Debug.Log("Spell not found in database for ID: " + spellID);
            spellQuickSlotUI.enabled = false;
            spellQuickSlotUI.sprite = null;
            return;
        }

        if (spell.itemIcon == null)
        {
            //Debug.Log("Spell icon not found for spell: " + spell.itemName);
            spellQuickSlotUI.enabled = false;
            spellQuickSlotUI.sprite = null;
            return;
        }

        spellQuickSlotUI.sprite = spell.itemIcon;
        spellQuickSlotUI.enabled = true;
    }

    public void SetQuickSlotItemQuickSlotIcon(int quickSlotItemID)
    {
        QuickSlotItem quickSlotItem = WorldItemDatabase.instance.GetQuickSlotItemByID(quickSlotItemID);

        if (quickSlotItem == null)
        {
            //Debug.Log("Quick Slot Item not found in database for ID: " + quickSlotItemID);
            quickSlotItemUI.enabled = false;
            quickSlotItemUI.sprite = null;
            return;
        }

        if (quickSlotItem.itemIcon == null)
        {
            //Debug.Log("Quick Slot Item icon not found for item: " + quickSlotItem.itemName);
            quickSlotItemUI.enabled = false;
            quickSlotItemUI.sprite = null;
            return;
        }

        quickSlotItemUI.sprite = quickSlotItem.itemIcon;
        quickSlotItemUI.enabled = true;
    }

}
