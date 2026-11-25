using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [Header("Network JOIN")]
    [SerializeField] private bool startAsClient;
    public bool IsParrelSyncClone { get; private set; }
    private StartingClassDefinition clientStartingClass;
    private string clientCharacterName;
    private AppearanceSaveData clientAppearance;
    private bool clientIsMale;

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
#if UNITY_EDITOR
        string directory = System.IO.Path.GetDirectoryName(Application.dataPath);
        IsParrelSyncClone = System.IO.File.Exists(System.IO.Path.Combine(directory, ".clone"));
#endif
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

    public void PrepareClientCharacter(string characterName, StartingClassDefinition startingClass,
        AppearanceSaveData appearance = null, bool isMale = false)
    {
        clientCharacterName = characterName;
        clientStartingClass = startingClass;
        clientAppearance = appearance;
        clientIsMale = isMale;
        WorldSaveGameManager.instance.currentCharacterData = new CharacterSaveData();
        WorldSaveGameManager.instance.currentCharacterSlotBeingUsed = CharacterSlot.NO_SLOT;
    }

    public bool ApplyPreparedClientCharacter(PlayerManager player)
    {
        if (!IsParrelSyncClone || !clientStartingClass ||
            WorldSaveGameManager.instance.currentCharacterSlotBeingUsed != CharacterSlot.NO_SLOT)
            return false;

        if (clientAppearance != null)
        {
            player.playerBodyManager.ToggleBodyType(clientIsMale);
            player.playerBodyManager.LoadAppearance(clientAppearance);
        }

        player.InitializeNewCharacter(clientCharacterName, clientStartingClass);
        return true;
    }

    private IEnumerator SwitchToClient()
    {
        var network = NetworkManager.Singleton;
        if (!network || network.IsClient && !network.IsHost)
            yield break;

        if (IsParrelSyncClone && !clientStartingClass &&
            WorldSaveGameManager.instance.currentCharacterData == null)
        {
            Debug.LogWarning("Press Start and prepare a character before joining.");
            yield break;
        }

        if (network.IsListening)
            network.Shutdown();

        while (network.ShutdownInProgress)
            yield return null;

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
