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

    [Header("Teleport Buttons")]
    public List<GameObject> teleportButtons;
    public GameObject teleportButtonParent;

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

    public void OpenMainMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = true;
        mainMenu.SetActive(true);
        teleportMenu.SetActive(false);
    }

    public void CloseMainMenu()
    {
        StartCoroutine(CloseTeleportMenuCoroutine());
    }

    public void OpenTeleportMenu()
    {
        mainMenu.SetActive(false);
        teleportMenu.SetActive(true);

        ToggleTeleportMenuButtons();
    }

    public void CloseTeleportMenu()
    {
        mainMenu.SetActive(true);
        teleportMenu.SetActive(false);
    }

    public void TeleportToSiteOfGrace(int siteOfGraceID)
    {
        CloseMainMenu();

        if (WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.ContainsKey(siteOfGraceID)
         && WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated[siteOfGraceID])
        {
            // Perform the teleportation logic here
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
            SiteOfGraceInteractable targetSiteOfGrace = WorldObjectManager.instance.siteOfGraces[siteOfGraceID];
            if (targetSiteOfGrace != null && targetSiteOfGrace.teleportPosition != null)
            {
                PlayerUIManager.instance.playerUILoadingScreenManager.ActivateLoadingScreen();

                // Debug.Log($"Teleporting player to Site of Grace ID: {siteOfGraceID} at position: {targetSiteOfGrace.teleportPosition.position}");
                player.transform.position = targetSiteOfGrace.teleportPosition.position;

                PlayerUIManager.instance.playerUILoadingScreenManager.DeactivateLoadingScreen();
            }
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
        }
    }

}
