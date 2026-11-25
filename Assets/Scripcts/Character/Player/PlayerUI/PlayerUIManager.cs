using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [Header("Network JOIN")]
    [SerializeField] private bool startAsClient;

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

    private void Update()
    {
        if (startAsClient)
        {
            startAsClient = false;
            StartCoroutine(SwitchToClient());
        }
    }

    private IEnumerator SwitchToClient()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.IsClient && !network.IsHost)
            yield break;

        if (!network.TryGetComponent<UnityTransport>(out var transport))
        {
            Debug.LogError("Unity Transport is missing.");
            yield break;
        }

        if (network.IsListening)
            network.Shutdown();

        while (network.ShutdownInProgress)
            yield return null;

        network.NetworkConfig.NetworkTransport = transport;
        network.StartClient();
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
