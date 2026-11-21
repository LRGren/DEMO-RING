using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerUISpellManager : MonoBehaviour
{
    [Header("Spell UI Elements")]
    public int currentSelectedSlotIndex = 0;
    private List<SpellItem> hasInitializedSpells = new List<SpellItem>();

    [Header("Equipped Spells")]
    public List<GameObject> equippedSpells = new List<GameObject>();
    public GameObject equippedSpellParent;

    [Header("Spell Inventory")]
    public GameObject spellInventorySlotPrefab;
    private List<GameObject> allSpells = new List<GameObject>();
    public GameObject spellInventoryParent;

    void Awake()
    {
    }

    private void ConfigureSpellListNavigation()
    {
        var grid = spellInventoryParent.GetComponent<GridLayoutGroup>();
        int columns = grid.constraintCount;

        for (int i = 0; i < allSpells.Count; i++)
        {
            var button = allSpells[i].GetComponent<Button>();
            int column = i % columns;

            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = column > 0 ? GetSpellButton(i - 1) : null;
            navigation.selectOnRight = column < columns - 1 ? GetSpellButton(i + 1) : null;
            navigation.selectOnUp = GetSpellButton(i - columns);
            navigation.selectOnDown = GetSpellButton(i + columns);
            button.navigation = navigation;
        }
    }

    private Button GetSpellButton(int index)
    {
        if (index < 0 || index >= allSpells.Count)
            return null;

        return allSpells[index].GetComponent<Button>();
    }

    private void InitializeSpellInventory()
    {
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        for (int i = 0; i < player.playerInventoryManager.spellInventory.Length; i++)
        {
            if (i < player.playerInventoryManager.currentMaxSpellCapacity)
            {
                SpellItem spellItem = player.playerInventoryManager.spellInventory[i];
                if (spellItem != null)
                    hasInitializedSpells.Add(spellItem);

                equippedSpells[i].SetActive(true);
                equippedSpells[i].GetComponent<UI_SpellInventorySlot>().SetItem(spellItem);
            }
            else
            {
                equippedSpells[i].SetActive(false);
            }
        }

        foreach (var obtainedSpell in player.playerInventoryManager.characterInventory)
        {
            if (obtainedSpell is SpellItem spellItem)
                if (!hasInitializedSpells.Find(spell => spell.itemID == spellItem.itemID))
                    hasInitializedSpells.Add(spellItem);
        }

        hasInitializedSpells.Sort((x, y) => x.itemID.CompareTo(y.itemID));
        foreach (var spell in hasInitializedSpells)
        {
            GameObject spellInventorySlot = Instantiate(spellInventorySlotPrefab, spellInventoryParent.transform);
            var uiSpellInventorySlot = spellInventorySlot.GetComponent<UI_SpellInventorySlot>();
            uiSpellInventorySlot.SetItem(spell);

            allSpells.Add(spellInventorySlot);
        }

        ConfigureSpellListNavigation();
    }

    public void RefreshSpellInventory()
    {
        foreach (var spell in allSpells)
        {
            Destroy(spell);
        }
        allSpells.Clear();
        hasInitializedSpells.Clear();
        InitializeSpellInventory();
    }

    public void EquipSpell(SpellItem spell)
    {
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        if (currentSelectedSlotIndex < player.playerInventoryManager.currentMaxSpellCapacity)
        {
            if (spell != null)
            {
                player.playerInventoryManager.SetSpellInInventory(spell, currentSelectedSlotIndex);
                equippedSpells[currentSelectedSlotIndex].GetComponent<Button>().Select();
                RefreshSpellInventory();
            }
        }
    }

    public void SelectToCurrentSpellInAllSpells(SpellItem spell)
    {
        Button firstButton = null;
        Button matchingButton = null;

        foreach (var slotObject in allSpells)
        {
            var slot = slotObject.GetComponent<UI_SpellInventorySlot>();
            var button = slotObject.GetComponent<Button>();

            bool isSameSpell = spell != null && slot.currentItem != null
                && slot.currentItem.itemID == spell.itemID;

            slot.canEquip = !isSameSpell;

            if (firstButton == null)
                firstButton = button;

            if (isSameSpell)
                matchingButton = button;
        }

        var buttonToSelect = matchingButton != null ? matchingButton : firstButton;
        if (buttonToSelect != null)
            buttonToSelect.Select();
    }

    public bool hasEquippedSpell(SpellItem spell)
    {
        foreach (var equippedSpell in equippedSpells)
        {
            var uiSpellInventorySlot = equippedSpell.GetComponent<UI_SpellInventorySlot>();
            if (uiSpellInventorySlot.currentItem == spell)
                return true;
        }
        return false;
    }

    public void BackInputBranch()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            if (selected.GetComponent<UI_SpellInventorySlot>() && !selected.GetComponent<UI_SpellInventorySlot>().isEquippedSlot)
                equippedSpells[currentSelectedSlotIndex].GetComponent<Button>().Select();
            else if (selected.GetComponent<UI_SpellInventorySlot>() && selected.GetComponent<UI_SpellInventorySlot>().isEquippedSlot)
                PlayerUIManager.instance.playerUISiteOfGraceManager.CloseSpellMenu();
        }
    }

    public void UnequipSpell()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            if (selected.GetComponent<UI_SpellInventorySlot>() && selected.GetComponent<UI_SpellInventorySlot>().isEquippedSlot)
            {
                PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
                player.playerInventoryManager.SetSpellInInventory(null, currentSelectedSlotIndex);
                equippedSpells[currentSelectedSlotIndex].GetComponent<Button>().Select();
                RefreshSpellInventory();
            }
        }
    }

}
