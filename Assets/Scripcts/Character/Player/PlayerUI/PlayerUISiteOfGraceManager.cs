using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerUISiteOfGraceManager : MonoBehaviour
{
    private PlayerManager player;
    private PlayerUIHudManager playerUIHudManager;

    [Header("Main Menu")]
    public GameObject mainMenu;
    public GameObject teleportMenu;
    public GameObject spellMenu;

    [Header("LevelUp Menu")]
    public PlayerAtrribute selectedPlayerAtrribute;
    public GameObject levelUpMenu;
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI goldCostText;
    public TextMeshProUGUI goldLeftText;
    private bool canUpgrade = false;
    private int deltaLevel = 0;
    private int currentCost = 0;
    private PlayerAtrributeDraft currentPlayerAtrributeDraft;
    private PlayerAtrributeDraft temporaryPlayerAtrributeDraft;
    public TextMeshProUGUI vigorValueText;
    public TextMeshProUGUI mindValueText;
    public TextMeshProUGUI enduranceValueText;
    public TextMeshProUGUI strengthValueText;
    public TextMeshProUGUI dexterityValueText;
    public TextMeshProUGUI intelligenceValueText;
    public TextMeshProUGUI faithValueText;
    public TextMeshProUGUI arcaneValueText;

    [Header("Teleport Buttons")]
    public List<GameObject> teleportButtons;
    public GameObject teleportButtonParent;

    [Header("Site of Grace Info")]
    public int currentSiteOfGraceID = -1; // Initialize with an invalid ID
    public Button teleportReturnButton;

    [Header("Spell Menu")]
    public PlayerUISpellManager playerUISpellManager;

    void Awake()
    {
        if (teleportButtons.Count > 0)
            return;

        playerUISpellManager = GetComponent<PlayerUISpellManager>();
        Button[] buttons = teleportButtonParent.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            teleportButtons.Add(button.gameObject);
        }
    }

    void Start()
    {
        if (playerUIHudManager == null)
            playerUIHudManager = PlayerUIManager.instance.playerUIHudManager;
    }

    public void UpdateCurrentSiteOfGraceID(int siteOfGraceID)
    {
        currentSiteOfGraceID = siteOfGraceID;
    }

    public void OpenMainMenu()
    {
        playerUIHudManager.ToggleHUD(false);

        PlayerUIManager.instance.menuWindowIsOpen = true;
        mainMenu.SetActive(true);

        teleportMenu.SetActive(false);
        spellMenu.SetActive(false);
    }

    public void CloseMainMenu()
    {
        playerUIHudManager.ToggleHUD(true);

        if (!mainMenu.activeSelf)
            return;

        StartCoroutine(CloseTeleportMenuCoroutine());

        if (WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.ContainsKey(currentSiteOfGraceID)
         && WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated[currentSiteOfGraceID])
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
            SiteOfGraceInteractable targetSiteOfGrace = WorldObjectManager.instance.siteOfGraces.Find(sog => sog.siteOfGraceID == currentSiteOfGraceID);

            targetSiteOfGrace.StandUpFromResting(player);
        }
    }

    public void OpenTeleportMenu()
    {
        mainMenu.SetActive(false);
        teleportMenu.SetActive(true);

        teleportReturnButton.Select();

        ToggleTeleportMenuButtons();
    }

    public void CloseTeleportMenu()
    {
        mainMenu.SetActive(true);
        teleportMenu.SetActive(false);
    }

    public void TeleportToSiteOfGrace(int siteOfGraceID)
    {
        if (!WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.ContainsKey(siteOfGraceID))
            return;

        if (!WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated[siteOfGraceID])
            return;

        // Perform the teleportation logic here
        PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        SiteOfGraceInteractable targetSiteOfGrace = WorldObjectManager.instance.siteOfGraces.Find(sog => sog.siteOfGraceID == siteOfGraceID);
        if (targetSiteOfGrace != null && targetSiteOfGrace.teleportPosition != null)
        {
            StartCoroutine(CloseTeleportMenuCoroutine());

            UpdateCurrentSiteOfGraceID(siteOfGraceID);

            PlayerUIManager.instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            // Debug.Log($"Teleporting player to Site of Grace ID: {siteOfGraceID} at position: {targetSiteOfGrace.teleportPosition.position}");
            player.transform.position = targetSiteOfGrace.teleportPosition.position;

            PlayerUIManager.instance.playerUILoadingScreenManager.DeactivateLoadingScreen();

            targetSiteOfGrace.StandUpFromResting(player);
        }
    }

    private IEnumerator CloseTeleportMenuCoroutine()
    {
        yield return new WaitForSeconds(0.15f);

        teleportMenu.SetActive(false);
        mainMenu.SetActive(false);

        PlayerUIManager.instance.menuWindowIsOpen = false;
    }

    private void ToggleTeleportMenuButtons()
    {
        foreach (var siteOfGrace in WorldObjectManager.instance.siteOfGraces)
        {
            if (WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.ContainsKey(siteOfGrace.siteOfGraceID)
             && WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated[siteOfGrace.siteOfGraceID])
            {
                // Enable the button for this Site of Grace
                // Assuming you have a method to enable buttons based on Site of Grace ID
                teleportButtons[siteOfGrace.siteOfGraceID].SetActive(WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated[siteOfGrace.siteOfGraceID]);
            }
            else
            {
                // Disable the button for this Site of Grace
                teleportButtons[siteOfGrace.siteOfGraceID].SetActive(false);
            }
        }

        if (currentSiteOfGraceID >= 0 && currentSiteOfGraceID < teleportButtons.Count)
        {
            // Disable the button for the current Site of Grace
            teleportButtons[currentSiteOfGraceID].SetActive(false);
        }
    }

    public void OpenSpellMenu()
    {
        mainMenu.SetActive(false);
        spellMenu.SetActive(true);

        playerUISpellManager.RefreshSpellInventory();
    }

    public void CloseSpellMenu()
    {
        mainMenu.SetActive(true);
        spellMenu.SetActive(false);
    }

    public void OpenLevelUpMenu()
    {
        mainMenu.SetActive(false);
        levelUpMenu.SetActive(true);

        InitializeCurrentPlayerAtrributeDraft();
        vigorValueText.GetComponentInParent<Button>().Select();
    }

    public void CloseLevelUpMenu()
    {
        mainMenu.SetActive(true);
        levelUpMenu.SetActive(false);
    }

    // Level Up Menu Methods
    private void InitializeCurrentPlayerAtrributeDraft()
    {
        if (player == null)
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        deltaLevel = 0;
        canUpgrade = true;

        currentPlayerAtrributeDraft = new PlayerAtrributeDraft(
            player.playerNetworkManager.level.Value,
            player.playerNetworkManager.vigor.Value,
            player.playerNetworkManager.mind.Value,
            player.playerNetworkManager.endurance.Value,
            player.playerNetworkManager.strength.Value,
            player.playerNetworkManager.dexterity.Value,
            player.playerNetworkManager.intelligence.Value,
            player.playerNetworkManager.faith.Value,
            player.playerNetworkManager.arcane.Value
            );

        temporaryPlayerAtrributeDraft = new PlayerAtrributeDraft(currentPlayerAtrributeDraft);
        vigorValueText.text = "< " + currentPlayerAtrributeDraft.vigor.ToString() + " >";
        mindValueText.text = "< " + currentPlayerAtrributeDraft.mind.ToString() + " >";
        enduranceValueText.text = "< " + currentPlayerAtrributeDraft.endurance.ToString() + " >";
        strengthValueText.text = "< " + currentPlayerAtrributeDraft.strength.ToString() + " >";
        dexterityValueText.text = "< " + currentPlayerAtrributeDraft.dexterity.ToString() + " >";
        intelligenceValueText.text = "< " + currentPlayerAtrributeDraft.intelligence.ToString() + " >";
        faithValueText.text = "< " + currentPlayerAtrributeDraft.faith.ToString() + " >";
        arcaneValueText.text = "< " + currentPlayerAtrributeDraft.arcane.ToString() + " >";

        CalculateAndDisplayGoldCost();
        DisplayCurrentLevel(true);
    }

    /// <summary>
    /// Calculates the gold cost for upgrading a player attribute based on the current level.
    /// </summary>
    /// <param name="Lvl">The current level of the player.</param>
    private int function_UpgradeGoldCost(int Lvl)
    {
        float x = Mathf.Max(((Lvl + 81) - 92) * 0.02f, 0f);
        int goldCost = Mathf.FloorToInt(((x + 0.1f) * Mathf.Pow(Lvl + 81, 2)) + 1);
        return goldCost;
    }

    private void CalculateAndDisplayGoldCost()
    {
        int totalGoldCost = 0;

        // Calculate the total gold cost based on the deltaLevel
        for (int i = 0; i < deltaLevel; i++)
        {
            int currentLevel = currentPlayerAtrributeDraft.level + i;
            totalGoldCost += function_UpgradeGoldCost(currentLevel);
        }
        currentCost = totalGoldCost;

        int goldLeft = player.playerNetworkManager.currentGold.Value - currentCost;
        canUpgrade = goldLeft >= 0;

        goldCostText.text = currentCost.ToString();
        goldLeftText.text = goldLeft.ToString();

        Color goldTextColor = canUpgrade ? Color.white : Color.red;
        goldCostText.color = goldTextColor;
        goldLeftText.color = goldTextColor;
    }

    private void DisplayCurrentLevel(bool completed = false)
    {
        if (completed)
        {
            levelValueText.text = currentPlayerAtrributeDraft.level.ToString();
        }
        else
        {
            levelValueText.text = currentPlayerAtrributeDraft.level + " + " + deltaLevel;
        }
    }

    public void SelectPlayerAttribute(BaseEventData baseEvent)
    {
        int direction = 0;
        var data = baseEvent as AxisEventData;
        if (data == null || (data.moveDir != MoveDirection.Left && data.moveDir != MoveDirection.Right))
            return;

        direction = data.moveDir == MoveDirection.Left ? -1 : 1;

        switch (selectedPlayerAtrribute)
        {
            case PlayerAtrribute.Vigor:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.vigor, direction, vigorValueText, currentPlayerAtrributeDraft.vigor);
                break;
            case PlayerAtrribute.Mind:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.mind, direction, mindValueText, currentPlayerAtrributeDraft.mind);
                break;
            case PlayerAtrribute.Endurance:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.endurance, direction, enduranceValueText, currentPlayerAtrributeDraft.endurance);
                break;
            case PlayerAtrribute.Strength:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.strength, direction, strengthValueText, currentPlayerAtrributeDraft.strength);
                break;
            case PlayerAtrribute.Dexterity:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.dexterity, direction, dexterityValueText, currentPlayerAtrributeDraft.dexterity);
                break;
            case PlayerAtrribute.Intelligence:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.intelligence, direction, intelligenceValueText, currentPlayerAtrributeDraft.intelligence);
                break;
            case PlayerAtrribute.Faith:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.faith, direction, faithValueText, currentPlayerAtrributeDraft.faith);
                break;
            case PlayerAtrribute.Arcane:
                AttemptToUpgrade(ref temporaryPlayerAtrributeDraft.arcane, direction, arcaneValueText, currentPlayerAtrributeDraft.arcane);
                break;
            default: break;
        }
    }

    private void AttemptToUpgrade(ref int attributeValue, int direction, TextMeshProUGUI textComponent, int currentValue)
    {
        if (direction > 0 && !canUpgrade)
            return;

        var targetValue = attributeValue + direction;
        if (targetValue >= currentValue && targetValue <= 99)
        {
            attributeValue = targetValue;
            textComponent.text = "< " + attributeValue.ToString() + " >";
            deltaLevel += direction;
            CalculateAndDisplayGoldCost();
            DisplayCurrentLevel();
        }
    }

    public void ApplyLevelUp()
    {
        if (!levelUpMenu.activeInHierarchy || deltaLevel <= 0)
        {
            CancelLevelUp();
            return;
        }

        if (player == null)
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

        CalculateAndDisplayGoldCost();
        if (!canUpgrade)
        {
            CancelLevelUp();
            return;
        }

        player.playerNetworkManager.currentGold.Value -= currentCost;
        currentCost = 0;

        player.playerNetworkManager.level.Value = temporaryPlayerAtrributeDraft.level + deltaLevel;
        player.playerNetworkManager.vigor.Value = temporaryPlayerAtrributeDraft.vigor;
        player.playerNetworkManager.mind.Value = temporaryPlayerAtrributeDraft.mind;
        player.playerNetworkManager.endurance.Value = temporaryPlayerAtrributeDraft.endurance;
        player.playerNetworkManager.strength.Value = temporaryPlayerAtrributeDraft.strength;
        player.playerNetworkManager.dexterity.Value = temporaryPlayerAtrributeDraft.dexterity;
        player.playerNetworkManager.intelligence.Value = temporaryPlayerAtrributeDraft.intelligence;
        player.playerNetworkManager.faith.Value = temporaryPlayerAtrributeDraft.faith;
        player.playerNetworkManager.arcane.Value = temporaryPlayerAtrributeDraft.arcane;

        deltaLevel = 0;
        // ApplyLevelUp() 末尾，写回等级和属性之后
        WorldSaveGameManager.instance.SaveGame();
        InitializeCurrentPlayerAtrributeDraft();
    }

    public void CancelLevelUp()
    {
        InitializeCurrentPlayerAtrributeDraft();
    }
}
