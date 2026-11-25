using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager instance;

    [Header("Menus")]
    [SerializeField] Button pressStartButton;
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
    public Button FirstCUSTOMIZATIONButton;
    [SerializeField] GameObject characterCreation_CharacterCustomizationMenu;

    [Header("Character Customization")]
    [Header("Gender")]
    public TextMeshProUGUI genderText;
    [Header("Hair Style")]
    public TextMeshProUGUI hairStyleText;
    [Header("Head Style")]
    public TextMeshProUGUI headStyleText;
    [Header("Eyebrow Style")]
    public TextMeshProUGUI eyebrowStyleText;
    [Header("Facial Hair Style")]
    public TextMeshProUGUI facialHairStyleText;

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
    public void StartNetwork()
    {
        titleScreenMainMenu.SetActive(true);
        pressStartButton.gameObject.SetActive(false);
        mainMenuNewGameButton.Select();
    }

    public void StartNewGame()
    {
        if (!TryGetPreviewPlayer() || !selectedStartingClass)
            return;

        var appearance = previewPlayer.playerBodyManager.GetAppearanceData();
        bool isMale = previewPlayer.playerBodyManager.IsMaleBody;

        var saveManager = WorldSaveGameManager.instance;
        if (!saveManager.TryStartSinglePlayer())
            return;

        var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (!playerObject || !playerObject.TryGetComponent<PlayerManager>(out var player))
        {
            Debug.LogError("The local player did not spawn.");
            return;
        }

        player.playerBodyManager.ToggleBodyType(isMale);
        player.playerBodyManager.LoadAppearance(appearance);
        player.InitializeNewCharacter(characterName, selectedStartingClass);

        saveManager.AttemptToCreateNewGame();
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
        if (!TryGetPreviewPlayer() || startingClasses.Count == 0)
            return;

        titleScreenMainMenu.SetActive(false);
        titleScreenCharacterCreationMenu.SetActive(true);

        SelectStartingClass(startingClasses[0].className);
        previewPlayer.playerBodyManager.ResetAppearance();
        RefreshAppearanceTexts();
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

        if (!TryGetPreviewPlayer() || !selectedStartingClass)
            return;

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

    public void OpenCharacterCustomizationMenu()
    {
        if (!TryGetPreviewPlayer())
            return;

        characterCreation_CharacterCustomizationMenu.SetActive(true);
        FirstCUSTOMIZATIONButton.Select();
        RefreshAppearanceTexts();
    }

    public void CloseCharacterCustomizationMenu()
    {
        characterCreation_CharacterCustomizationMenu.SetActive(false);
        NAMEButton.Select();
    }


    // ------------------- Character Customization ------------------- //
    public void ToggleEquipmentPreview(bool isOn)
    {
        if (TryGetPreviewPlayer())
            previewPlayer.playerBodyManager.ToggleEquipmentPreview(isOn);
    }

    public void SetText(TextMeshProUGUI textMeshPro, string text)
    {
        if (textMeshPro != null)
            textMeshPro.text = text;
    }

    public void SetGender(BaseEventData baseEvent)
    {
        if (!TryGetAppearanceMove(baseEvent, out int direction))
            return;

        previewPlayer.playerBodyManager.ToggleBodyType(!previewPlayer.playerBodyManager.IsMaleBody);
        RefreshAppearanceTexts();
    }

    public void SetHairStyle(string hairID)
    {
        if (TryGetPreviewPlayer() && previewPlayer.playerBodyManager.TrySetHairStyle(hairID))
            RefreshAppearanceTexts();
    }

    public void SelectHairStyle(BaseEventData baseEvent)
    {
        if (!TryGetAppearanceMove(baseEvent, out int direction))
            return;

        var body = previewPlayer.playerBodyManager;
        var hairList = body.hairStyleCatalog != null ? body.hairStyleCatalog.hairStyleOptions : null;
        if (hairList == null || hairList.Count == 0)
        {
            SetHairStyle(PlayerBodyManager.NoHairID);
            return;
        }

        int currentIndex = hairList.FindIndex(h => h != null && h.hairID == body.CurrentHairID);
        int newIndex = GetNextStyleIndex(currentIndex, hairList.Count, direction);
        if (hairList[newIndex] != null)
            SetHairStyle(hairList[newIndex].hairID);
    }

    public void SetHeadStyle(string headID)
    {
        if (TryGetPreviewPlayer() && previewPlayer.playerBodyManager.TrySetHeadStyle(headID))
            RefreshAppearanceTexts();
    }

    public void SelectHeadStyle(BaseEventData baseEvent)
    {
        if (!TryGetAppearanceMove(baseEvent, out int direction))
            return;

        var body = previewPlayer.playerBodyManager;
        var headList = body.headStyleCatalog != null ? body.headStyleCatalog.headStyleOptions : null;
        if (headList == null || headList.Count == 0)
            return;

        int currentIndex = headList.FindIndex(h => h != null && h.headID == body.CurrentHeadID);
        int newIndex = GetNextStyleIndex(currentIndex, headList.Count, direction);
        if (headList[newIndex] != null)
            SetHeadStyle(headList[newIndex].headID);
    }

    public void SetEyebrowStyle(string eyebrowID)
    {
        if (TryGetPreviewPlayer() && previewPlayer.playerBodyManager.TrySetEyebrowStyle(eyebrowID))
            RefreshAppearanceTexts();
    }

    public void SelectEyebrowStyle(BaseEventData baseEvent)
    {
        if (!TryGetAppearanceMove(baseEvent, out int direction))
            return;

        var body = previewPlayer.playerBodyManager;
        var catalog = body.CurrentEyebrowStyleCatalog;
        var eyebrowList = catalog != null ? catalog.eyebrowStyleOptions : null;
        if (eyebrowList == null || eyebrowList.Count == 0)
        {
            SetEyebrowStyle("none");
            return;
        }

        int currentIndex = eyebrowList.FindIndex(e => e != null && e.eyebrowID == body.CurrentEyebrowID);
        int newIndex = GetNextStyleIndex(currentIndex, eyebrowList.Count, direction);
        if (eyebrowList[newIndex] != null)
            SetEyebrowStyle(eyebrowList[newIndex].eyebrowID);
    }

    public void SetFacialHairStyle(string facialHairID)
    {
        if (TryGetPreviewPlayer() && previewPlayer.playerBodyManager.IsMaleBody &&
            previewPlayer.playerBodyManager.TrySetFacialHairStyle(facialHairID))
            RefreshAppearanceTexts();
    }

    public void SelectFacialHairStyle(BaseEventData baseEvent)
    {
        if (!TryGetAppearanceMove(baseEvent, out int direction))
            return;

        var body = previewPlayer.playerBodyManager;
        if (!body.IsMaleBody)
            return;

        var facialHairList = body.facialHairStyleCatalog != null ? body.facialHairStyleCatalog.facialHairStyleOptions : null;
        if (facialHairList == null || facialHairList.Count == 0)
        {
            SetFacialHairStyle(PlayerBodyManager.NoFacialHairID);
            return;
        }

        int currentIndex = facialHairList.FindIndex(f => f != null && f.facialHairID == body.CurrentFacialHairID);
        int newIndex = GetNextStyleIndex(currentIndex, facialHairList.Count, direction);
        if (facialHairList[newIndex] != null)
            SetFacialHairStyle(facialHairList[newIndex].facialHairID);
    }

    public void RefreshAppearanceTexts()
    {
        if (!TryGetPreviewPlayer())
            return;

        var body = previewPlayer.playerBodyManager;
        SetText(genderText, body.IsMaleBody ? "Gender < Male >" : "Gender < Female >");

        string hairName = body.CurrentHairID == PlayerBodyManager.NoHairID ? "No_Hair" : body.CurrentHairID;
        if (body.hairStyleCatalog != null && body.hairStyleCatalog.TryGetOption(body.CurrentHairID, out HairStyleOption hairOption))
            hairName = hairOption.hairName;
        SetText(hairStyleText, " HAIR < " + hairName + " >");

        string headName = body.CurrentHeadID;
        if (body.headStyleCatalog != null && body.headStyleCatalog.TryGetOption(body.CurrentHeadID, out HeadStyleOption headOption))
            headName = headOption.headName;
        SetText(headStyleText, " FACE < " + headName + " >");

        string eyebrowName = body.CurrentEyebrowID == "none" ? "No_Eyebrows" : body.CurrentEyebrowID;
        var eyebrowCatalog = body.CurrentEyebrowStyleCatalog;
        if (eyebrowCatalog != null && eyebrowCatalog.TryGetOption(body.CurrentEyebrowID, out EyebrowStyleOption eyebrowOption))
            eyebrowName = eyebrowOption.eyebrowName;
        SetText(eyebrowStyleText, " EYEBROWS < " + eyebrowName + " >");

        string facialHairName = body.CurrentFacialHairID == PlayerBodyManager.NoFacialHairID ? "No_Beard" : body.CurrentFacialHairID;
        if (body.facialHairStyleCatalog != null && body.facialHairStyleCatalog.TryGetOption(body.CurrentFacialHairID, out FacialHairStyleOption facialHairOption))
            facialHairName = facialHairOption.facialHairName;
        SetText(facialHairStyleText, body.IsMaleBody ? " BEARD < " + facialHairName + " >" : " BEARD < Unavailable >");
    }

    private bool TryGetPreviewPlayer()
    {
        if (previewPlayer)
            return previewPlayer.playerBodyManager != null;

        var network = NetworkManager.Singleton;
        if (!network)
            return false;

        var localPlayer = network.LocalClient?.PlayerObject;
        if (localPlayer)
        {
            previewPlayer = localPlayer.GetComponent<PlayerManager>();
        }
        else
        {
            var rig = FindFirstObjectByType<TitleScreenPlayerPreviewRotator>();
            if (!rig)
                return false;

            var model = Instantiate(
                network.NetworkConfig.PlayerPrefab,
                rig.transform.position,
                rig.transform.rotation,
                rig.transform);

            previewPlayer = model.GetComponent<PlayerManager>();
            previewPlayer.playerLocomotionManager.enabled = false;
            previewPlayer.characterController.enabled = false;
        }

        return previewPlayer && previewPlayer.playerBodyManager;
    }

    private bool TryGetAppearanceMove(BaseEventData baseEvent, out int direction)
    {
        direction = 0;
        var data = baseEvent as AxisEventData;
        if (data == null || (data.moveDir != MoveDirection.Left && data.moveDir != MoveDirection.Right))
            return false;

        direction = data.moveDir == MoveDirection.Left ? -1 : 1;
        return TryGetPreviewPlayer();
    }

    private int GetNextStyleIndex(int currentIndex, int count, int direction)
    {
        // 没有匹配到当前 ID 时，先回到第一项。
        return currentIndex < 0 ? 0 : (currentIndex + direction + count) % count;
    }

}
