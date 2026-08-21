using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUICharacterMenuManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menu;

    public void OpenCharacterMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = true;
        menu.SetActive(true);
    }

    public void CloseCharacterMenu()
    {
        PlayerUIManager.instance.menuWindowIsOpen = false;
        menu.SetActive(false);
    }

    public void CloseCharacterMenuAfterFixedUpdate()
    {
        StartCoroutine(CloseMenuCoroutine());
    }

    private IEnumerator CloseMenuCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        PlayerUIManager.instance.menuWindowIsOpen = false;
        menu.SetActive(false);
    }
}
