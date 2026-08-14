using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Character_Save_Slot : MonoBehaviour
{
    SaveFileDataWriter saveFileWriter;
    
    [Header("Game Slot")]
    public CharacterSlot characterSlot;
    
    [Header("Character Info")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI timePlayed;

    private void OnEnable()
    {
        LoadGameSlots();
    }

    private void LoadGameSlots()
    {
        saveFileWriter = new SaveFileDataWriter();
        saveFileWriter.saveDataDirectoryPath =  Application.persistentDataPath;
        
        saveFileWriter.saveFileName =
            WorldSaveGameManager.instance.DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
        
        switch (characterSlot)
        {
            case CharacterSlot.CharacterSlot_01 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot01.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_02 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot02.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_03 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot03.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_04 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot04.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_05 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot05.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_06 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot06.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_07 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot07.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_08 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot08.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_09 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot09.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case CharacterSlot.CharacterSlot_10 :
                if (saveFileWriter.CheckToSeeIfFileExists())
                {
                    characterName.text = WorldSaveGameManager.instance.characterSlot10.characterName;
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
        }
    }

    public void LoadGameFromCharacterSlot()
    {
        WorldSaveGameManager.instance.currentCharacterSlotBeingUsed = characterSlot;
        WorldSaveGameManager.instance.LoadGame();
    }

    public void SelectCharacterSlot()
    {
        TitleScreenManager.instance.SelectCharacterSlot(characterSlot);
    }
    
}
