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

    [Header("Bosses")]
    public SerializableDictionary<string, bool> bossesAwakened;
    public SerializableDictionary<string, bool> bossesDefeated;

    public CharacterSaveData()
    {
        bossesAwakened = new SerializableDictionary<string, bool>();
        bossesDefeated = new SerializableDictionary<string, bool>();
    }


}
