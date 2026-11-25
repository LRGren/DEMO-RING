using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUICharacterMenuManager : MonoBehaviour
{
    private PlayerUIHudManager playerUIHudManager;

    [Header("Menu")]
    public GameObject menu;

    void Awake()
    {
    }

    void Start()
    {
        if (playerUIHudManager == null)
            playerUIHudManager = PlayerUIManager.instance.playerUIHudManager;
    }

    public void OpenCharacterMenu()
    {
        playerUIHudManager.ToggleHUD(false);
        PlayerUIManager.instance.menuWindowIsOpen = true;
        menu.SetActive(true);
    }

    public void CloseCharacterMenu()
    {
        playerUIHudManager.ToggleHUD(true);
        PlayerUIManager.instance.menuWindowIsOpen = false;
        menu.SetActive(false);
    }

    public void CloseCharacterMenuAfterFixedUpdate()
    {
        StartCoroutine(CloseMenuCoroutine());
    }

    private IEnumerator CloseMenuCoroutine()
    {
        yield return new WaitForSeconds(0.15f);

        PlayerUIManager.instance.menuWindowIsOpen = false;
        menu.SetActive(false);
    }
}
