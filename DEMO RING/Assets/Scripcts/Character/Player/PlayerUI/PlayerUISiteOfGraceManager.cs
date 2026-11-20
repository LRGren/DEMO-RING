using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class PlayerUISiteOfGraceManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenu;
    public GameObject teleportMenu;
    public GameObject spellMenu;

    [Header("Teleport Buttons")]
    public List<GameObject> teleportButtons;
    public GameObject teleportButtonParent;

    [Header("Site of Grace Info")]
    public int currentSiteOfGraceID = -1; // Initialize with an invalid ID
    public Button teleportReturnButton;

    void Awake()
    {
        if (teleportButtons.Count > 0)
            return;

        Button[] buttons = teleportButtonParent.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            teleportButtons.Add(button.gameObject);
        }
    }

    public void UpdateCurrentSiteOfGraceID(int siteOfGraceID)
    {
        currentSiteOfGraceID = siteOfGraceID;
    }

    public void OpenMainMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = true;
        mainMenu.SetActive(true);

        teleportMenu.SetActive(false);
        spellMenu.SetActive(false);
    }

    public void CloseMainMenu()
    {
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
    }

    public void CloseSpellMenu()
    {
        mainMenu.SetActive(true);
        spellMenu.SetActive(false);
    }

}
