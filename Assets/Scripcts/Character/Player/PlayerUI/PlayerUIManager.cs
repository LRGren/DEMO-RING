using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [HideInInspector] public PlayerUIHudManager playerUIHudManager;
    [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;
    [HideInInspector] public PlayerUICharacterMenuManager playerUICharacterMenuManager;
    [HideInInspector] public PlayerUIEquipmentManager playerUIEquipmentManager;
    [HideInInspector] public PlayerUISiteOfGraceManager playerUISiteOfGraceManager;
    [HideInInspector] public PlayerUILoadingScreenManager playerUILoadingScreenManager;
    [HideInInspector] public PlayerUISpellManager playerUISpellManager;

    [Header("UI Flags")]
    public bool menuWindowIsOpen = false;
    public bool popUpWindowIsOpen = false;

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

        playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
        playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
        playerUICharacterMenuManager = GetComponentInChildren<PlayerUICharacterMenuManager>();
        playerUIEquipmentManager = GetComponentInChildren<PlayerUIEquipmentManager>();
        playerUISiteOfGraceManager = GetComponentInChildren<PlayerUISiteOfGraceManager>();
        playerUILoadingScreenManager = GetComponentInChildren<PlayerUILoadingScreenManager>();
        playerUISpellManager = GetComponentInChildren<PlayerUISpellManager>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CloseAllWindows()
    {
        if (menuWindowIsOpen)
        {
            playerUICharacterMenuManager.CloseCharacterMenu();
            playerUIEquipmentManager.CloseEquipmentMenu();
            playerUISiteOfGraceManager.CloseMainMenu();
        }
    }

}
