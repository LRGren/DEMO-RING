using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SpellInventorySlot : MonoBehaviour
{
    private PlayerUISpellManager spellManager;

    public int slotIndex;
    public bool isEquippedSlot = false;

    public Image itemIcon;
    public Image highlightIcon;
    public SpellItem currentItem;
    public bool canEquip = true;

    public void Awake()
    {
        highlightIcon.enabled = false;

        spellManager = PlayerUIManager.instance.playerUISpellManager;
    }

    public void SetItem(SpellItem item)
    {
        currentItem = item;
        if (item != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemIcon.enabled = true;
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
    }

    public void SelectSlot()
    {
        if (isEquippedSlot)
            spellManager.currentSelectedSlotIndex = slotIndex;
        highlightIcon.enabled = true;
    }

    public void DeselectSlot()
    {
        highlightIcon.enabled = false;
    }

    public void EquipSpell()
    {
        if (!canEquip)
            return;

        SpellItem selectedSpell = currentItem;

        if (selectedSpell != null)
        {
            spellManager.EquipSpell(selectedSpell);
        }
    }

    public void SelectSpellInventorySlot()
    {
        spellManager.SelectToCurrentSpellInAllSpells(currentItem);
    }
}
