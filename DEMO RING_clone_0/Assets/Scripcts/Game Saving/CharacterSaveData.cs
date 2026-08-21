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

    [Header("Stats")]
    public int vitality;
    public int endurance;

    [Header("Site Of Grace")]
    public SerializableDictionary<int, bool> siteOfGraceActivated;

    [Header("Bosses")]
    public SerializableDictionary<string, bool> bossesAwakened;
    public SerializableDictionary<string, bool> bossesDefeated;

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


    public CharacterSaveData()
    {
        siteOfGraceActivated = new SerializableDictionary<int, bool>();

        bossesAwakened = new SerializableDictionary<string, bool>();
        bossesDefeated = new SerializableDictionary<string, bool>();
    }


}
