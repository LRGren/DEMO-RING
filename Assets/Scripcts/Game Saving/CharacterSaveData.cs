using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    [Header("Scene Index")]
    public int sceneIndex = 1;

    [Header("Character Name")]
    public string characterName = "sereinjians";
    public bool isMale;

    [Header("Time Played")]
    public float secondsPlayed;

    [Header("Gold")]
    public int currentGold;

    [Header("World Coordinates")]
    public float xPosition;
    public float yPosition;
    public float zPosition;

    [Header("Resources")]
    public int currentHealth;
    public float currentStamina;
    public int currentFocus;

    [Header("Stats")]
    public int level;
    public int vigor;
    public int mind;
    public int endurance;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int faith;
    public int arcane;

    [Header("Flasks")]
    public int remainingHealthFlasks;
    public int remainingManaFlasks;

    [Header("Site Of Grace")]
    public SerializableDictionary<int, bool> siteOfGraceActivated;

    [Header("Bosses")]
    public SerializableDictionary<string, bool> bossesAwakened;
    public SerializableDictionary<string, bool> bossesDefeated;

    [Header("Item")]
    public SerializableDictionary<int, bool> worldItemsLooted;

    [Header("Inventory")]
    public List<SerializableWeapon> weaponInInventory;
    public List<SerializableQuickSlotItem> quickSlotItemInInventory;
    public List<SerializableRangedProjectile> rangedProjectileInInventory;
    public List<int> equipmentInInventory;
    public List<int> spellInInventory;

    [Header("Equipment")]
    public int currentHeadEquipment;
    public int currentBodyEquipment;
    public int currentLegsEquipment;
    public int currentHandEquipment;

    public int currentRightWeaponIndex;
    public int currentLeftWeaponIndex;
    public SerializableWeapon rightWeapon01;
    public SerializableWeapon rightWeapon02;
    public SerializableWeapon rightWeapon03;

    public SerializableWeapon leftWeapon01;
    public SerializableWeapon leftWeapon02;
    public SerializableWeapon leftWeapon03;

    public int currentSpellIndex;
    public int currentSpell01;
    public int currentSpell02;
    public int currentSpell03;
    public int currentSpell04;
    public int currentSpell05;
    public int currentSpell06;
    public int currentSpell07;
    public int currentSpell08;
    public int currentSpell09;
    public int currentSpell10;

    public int currentQuickSlotItemIndex;
    public SerializableQuickSlotItem quickSlotItem01;
    public SerializableQuickSlotItem quickSlotItem02;
    public SerializableQuickSlotItem quickSlotItem03;
    public SerializableQuickSlotItem quickSlotItem04;
    public SerializableQuickSlotItem quickSlotItem05;
    public SerializableQuickSlotItem quickSlotItem06;
    public SerializableQuickSlotItem quickSlotItem07;
    public SerializableQuickSlotItem quickSlotItem08;
    public SerializableQuickSlotItem quickSlotItem09;
    public SerializableQuickSlotItem quickSlotItem10;



    public SerializableRangedProjectile currentMainProjectile;
    public SerializableRangedProjectile currentSecondaryProjectile;

    [Header("Appearance")]
    public AppearanceSaveData appearanceSaveData;

    public CharacterSaveData()
    {
        siteOfGraceActivated = new SerializableDictionary<int, bool>();

        bossesAwakened = new SerializableDictionary<string, bool>();
        bossesDefeated = new SerializableDictionary<string, bool>();

        worldItemsLooted = new SerializableDictionary<int, bool>();

        weaponInInventory = new List<SerializableWeapon>();
        quickSlotItemInInventory = new List<SerializableQuickSlotItem>();
        rangedProjectileInInventory = new List<SerializableRangedProjectile>();
        equipmentInInventory = new List<int>();
        spellInInventory = new List<int>();
    }


}
