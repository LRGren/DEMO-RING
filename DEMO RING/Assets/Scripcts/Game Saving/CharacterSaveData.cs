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

    [Header("World Coordinates")]
    public float xPosition;
    public float yPosition;
    public float zPosition;

    [Header("Resources")]
    public int currentHealth;
    public float currentStamina;
    public int currentFocus;

    [Header("Stats")]
    public int vitality;
    public int endurance;
    public int mind;

    [Header("Site Of Grace")]
    public SerializableDictionary<int, bool> siteOfGraceActivated;

    [Header("Bosses")]
    public SerializableDictionary<string, bool> bossesAwakened;
    public SerializableDictionary<string, bool> bossesDefeated;

    [Header("Item")]
    public SerializableDictionary<int, bool> worldItemsLooted;

    [Header("Inventory")]
    public List<Item> inventory;

    [Header("Equipment")]
    public int currentHeadEquipment;
    public int currentBodyEquipment;
    public int currentLegsEquipment;
    public int currentHandEquipment;

    public int currentRightWeaponIndex;
    public int rightWeapon01;
    public int rightWeapon02;
    public int rightWeapon03;

    public int currentLeftWeaponIndex;
    public int leftWeapon01;
    public int leftWeapon02;
    public int leftWeapon03;

    public int currentSpell;

    public int currentQuickSlotItemIndex;
    public int quickSlotItem01;
    public int quickSlotItem02;
    public int quickSlotItem03;
    public int quickSlotItem04;
    public int quickSlotItem05;
    public int quickSlotItem06;
    public int quickSlotItem07;
    public int quickSlotItem08;
    public int quickSlotItem09;
    public int quickSlotItem10;

    public int quickSlotItem01Amount;
    public int quickSlotItem02Amount;
    public int quickSlotItem03Amount;
    public int quickSlotItem04Amount;
    public int quickSlotItem05Amount;
    public int quickSlotItem06Amount;
    public int quickSlotItem07Amount;
    public int quickSlotItem08Amount;
    public int quickSlotItem09Amount;
    public int quickSlotItem10Amount;

    public int currentMainProjectile;
    public int currentSecondaryProjectile;

    public int currentMainAmmoAmount;
    public int currentSecondaryAmmoAmount;

    public CharacterSaveData()
    {
        siteOfGraceActivated = new SerializableDictionary<int, bool>();

        bossesAwakened = new SerializableDictionary<string, bool>();
        bossesDefeated = new SerializableDictionary<string, bool>();

        worldItemsLooted = new SerializableDictionary<int, bool>();

        inventory = new List<Item>();
    }


}
