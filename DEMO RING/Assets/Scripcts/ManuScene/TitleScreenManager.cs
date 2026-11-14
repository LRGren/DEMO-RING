using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager instance;

    [Header("Menus")]
    [SerializeField] GameObject titleScreenMainMenu;
    [SerializeField] GameObject titleScreenLoadMenu;
    [SerializeField] GameObject titleScreenCharacterCreationMenu;

    [Header("Character Creation")]
    private string characterName = "Tarnished";
    public PlayerManager previewPlayer;
    private StartingClassDefinition selectedStartingClass;
    public StartingClass currentSelectedStartingClass;
    public List<StartingClassDefinition> startingClasses = new List<StartingClassDefinition>();
    public List<GameObject> ValueTexts;

    public Button NAMEButton;
    public Button FirstCLASSButton;
    [SerializeField] GameObject characterCreation_ClassSelectionMenu;

    [Header("Buttons")]
    [SerializeField] Button loadMenuReturnButton;
    [SerializeField] Button mainMenuLoadGameButton;
    [SerializeField] Button mainMenuNewGameButton;
    [SerializeField] Button noCharacterSlotsOkayButton;
    [SerializeField] Button deleteCharacterSlotPopUpConfirmButton;

    [Header("Pop Ups")]
    [SerializeField] GameObject noCharacterSlotsPopUp;
    [SerializeField] GameObject deleteCharacterSlotPopUp;

    [Header("Character Slots")]
    public CharacterSlot currentSelectedSlot = CharacterSlot.NO_SLOT;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 以房主的身份开启服务器
    /// </summary>
    public void StartNetworkAsHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartNewGame()
    {
        if (previewPlayer == null)
            previewPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        previewPlayer.InitializeNewCharacter(characterName, selectedStartingClass);

        WorldSaveGameManager.instance.AttemptToCreateNewGame();
    }

    public void AttemptToCreateNewCharacter()
    {
        if (WorldSaveGameManager.instance.HasEmptyCharacterSlot())
        {
            OpenCharacterCreationMenu();
        }
        else
        {
            DisplayNoFreeCharacterSlotsPopUp();
        }
    }

    public void OpenCharacterCreationMenu()
    {
        titleScreenMainMenu.SetActive(false);
        titleScreenCharacterCreationMenu.SetActive(true);

        if (previewPlayer == null)

            previewPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        SelectStartingClass(startingClasses[0].className);
        previewPlayer.playerEquipmentManager.InitialAllSlotInPreview(selectedStartingClass);
    }

    public void CloseCharacterCreationMenu()
    {
        titleScreenMainMenu.SetActive(true);
        titleScreenCharacterCreationMenu.SetActive(false);

        mainMenuNewGameButton.Select();
    }

    public void OpenLoadGameMenu()
    {
        titleScreenMainMenu.SetActive(false);
        titleScreenLoadMenu.SetActive(true);

        loadMenuReturnButton.Select();
    }

    public void CloseLoadGameMenu()
    {
        titleScreenMainMenu.SetActive(true);
        titleScreenLoadMenu.SetActive(false);

        mainMenuLoadGameButton.Select();
    }

    public void DisplayNoFreeCharacterSlotsPopUp()
    {
        noCharacterSlotsPopUp.SetActive(true);
        noCharacterSlotsOkayButton.Select();
    }

    public void CloseNoFreeCharacterSlotsPopUp()
    {
        noCharacterSlotsPopUp.SetActive(false);
        mainMenuNewGameButton.Select();
    }

    public void SelectCharacterSlot(CharacterSlot slot)
    {
        currentSelectedSlot = slot;
    }

    public void SelectNoSlot()
    {
        currentSelectedSlot = CharacterSlot.NO_SLOT;
    }

    public void AttemptToDeleteCharacterSlot()
    {
        if (currentSelectedSlot != CharacterSlot.NO_SLOT)
        {
            deleteCharacterSlotPopUp.SetActive(true);
            deleteCharacterSlotPopUpConfirmButton.Select();
        }
    }

    public void DeleteCharacterSlot()
    {
        deleteCharacterSlotPopUp.SetActive(false);
        WorldSaveGameManager.instance.DeleteGame(currentSelectedSlot);

        titleScreenLoadMenu.SetActive(false);
        titleScreenLoadMenu.SetActive(true);

        loadMenuReturnButton.Select();
    }

    public void CloseDeleteCharacterSlot()
    {
        deleteCharacterSlotPopUp.SetActive(false);
        loadMenuReturnButton.Select();
    }

    public void OpenClassSelectionMenu()
    {
        FirstCLASSButton.Select();
        characterCreation_ClassSelectionMenu.SetActive(true);
    }

    public void CloseClassSelectionMenu()
    {
        NAMEButton.Select();
        characterCreation_ClassSelectionMenu.SetActive(false);
    }

    public void SelectStartingClass(string startingClass)
    {
        currentSelectedStartingClass = (StartingClass)Enum.Parse(typeof(StartingClass), startingClass);
        selectedStartingClass = startingClasses.Find(x => x.className == startingClass);

        if (previewPlayer == null)
            previewPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        previewPlayer.playerEquipmentManager.InitialAllSlotInPreview(selectedStartingClass);

        for (int i = 0; i < ValueTexts.Count; i++)
        {
            if (i == 0)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.level.ToString();
            else if (i == 1)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.vigor.ToString();
            else if (i == 2)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.mind.ToString();
            else if (i == 3)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.endurance.ToString();
            else if (i == 4)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.strength.ToString();
            else if (i == 5)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.dexterity.ToString();
            else if (i == 6)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.intelligence.ToString();
            else if (i == 7)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.faith.ToString();
            else if (i == 8)
                ValueTexts[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStartingClass.arcane.ToString();
        }
    }
}
