using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerManager : CharacterManager
{
    [Header("DEBUG MENU")]
    [SerializeField] private bool respawnCharacter = false;
    [SerializeField] private bool switchRightWeapon = false;
    
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
    [HideInInspector] public PlayerNetworkManager playerNetworkManager;
    [HideInInspector] public PlayerStatsManager playerStatsManager;
    [HideInInspector] public PlayerInventoryManager playerInventoryManager;
    [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
    [HideInInspector] public PlayerCombatManager playerCombatManager;

    protected override void Awake()
    {
        base.Awake();
        
        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
        playerNetworkManager = GetComponent<PlayerNetworkManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
    }

    protected override void Update()
    {
        base.Update();

        if (!IsOwner)
            return;
        
        playerLocomotionManager.HandleAllMovement();
            
        playerStatsManager.StaminaRegeneration();
        
        DebugMenu();
    }

    protected override void LateUpdate()
    {
        if (!IsOwner)
            return;
        
        base.LateUpdate();
        
        PlayerCamera.instance.HandleAllCameraActions();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnecterCallback;

        if (IsOwner)
        {
            //如果是本地，开始是设定player camera的目标是自己
            PlayerCamera.instance.player = this;
            PlayerInputManager.instance.player = this;
            WorldSaveGameManager.instance.player = this;

            //更新状态条最大值
            playerNetworkManager.vitality.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
            playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;
            
            //更新状态条
            playerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
            playerNetworkManager.currentStamina.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
            playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaTimer;
            
        }

        //状态
        playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.CheckHP;

        //装备
        playerNetworkManager.currentRightHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightHandWeaponIDChanged;
        playerNetworkManager.currentLeftHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftHandWeaponIDChanged;
        playerNetworkManager.currentWeaponBeingUsed.OnValueChanged += playerNetworkManager.OnCurrentWeaponBedingUsedIDChanged;

        //锁定
        playerNetworkManager.isLockOn.OnValueChanged+=playerNetworkManager.OnIsLockOnChanged;
        playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged += playerNetworkManager.OnLockOnTargetIDChange;

        //FLAGS
        playerNetworkManager.isChargingAttack.OnValueChanged+=playerNetworkManager.OnIsChargingAttackChanged;

        //如果不是房主，本地玩家需要重新设置状态条最大值和当前值 因为房主的数值会同步过来
        //否则会导致Player Network Manager的数据不会更新
        //如果是房主，当前角色数据会在游戏开始时加载,所以不需要在这里加载
        if (IsOwner && !IsServer)
        {
            LoadGameFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnecterCallback;

        if (IsOwner)
        {
            //更新状态条最大值
            playerNetworkManager.vitality.OnValueChanged -= playerNetworkManager.SetNewMaxHealthValue;
            playerNetworkManager.endurance.OnValueChanged -= playerNetworkManager.SetNewMaxStaminaValue;

            //更新状态条
            playerNetworkManager.currentHealth.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
            playerNetworkManager.currentStamina.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
            playerNetworkManager.currentStamina.OnValueChanged -= playerStatsManager.ResetStaminaTimer;

        }

        //状态
        playerNetworkManager.currentHealth.OnValueChanged -= playerNetworkManager.CheckHP;

        //装备
        playerNetworkManager.currentRightHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentRightHandWeaponIDChanged;
        playerNetworkManager.currentLeftHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentLeftHandWeaponIDChanged;
        playerNetworkManager.currentWeaponBeingUsed.OnValueChanged -= playerNetworkManager.OnCurrentWeaponBedingUsedIDChanged;

        //锁定
        playerNetworkManager.isLockOn.OnValueChanged -= playerNetworkManager.OnIsLockOnChanged;
        playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged -= playerNetworkManager.OnLockOnTargetIDChange;
        //FLAGS
        playerNetworkManager.isChargingAttack.OnValueChanged -= playerNetworkManager.OnIsChargingAttackChanged;
    }

    private void OnClientConnecterCallback(ulong clientId)
    {
        WorldGameSessionManager.instance.AddPlayerToActivePlayerList(this);

        //如果我们是服务器，表示我们是主机，不会迟于其他玩家加入，所以在他们加入的时候会进行加载
        //如果我们是客户端，表示我们是加入别人的玩家，我们需要在加入的时候加载其他玩家的数据

        if (!IsServer && IsOwner)
        {
            foreach (var player in WorldGameSessionManager.instance.players)
            {
                if (player != this)
                {
                    player.LoadOtherPlayerCharacterWhenJoingServer();
                }
            }
        }
    }

    public override IEnumerator ProcessDeathEvent(bool manuallySelectedDeathAnimation = false)
    {
        if (IsOwner)
        {
            PlayerUIManager.instance.playerUIPopUpManager.SendYouDiedPopUp();
        }
        
        return base.ProcessDeathEvent(manuallySelectedDeathAnimation);
        
        //查看所有玩家是否都已死亡 复活所有玩家
    }

    public override void ReviveCharacter()
    {
        base.ReviveCharacter();

        if (IsOwner)
        {
            //记得将死亡状态改为false 否则角色会一直处于死亡状态 无法进行其他操作
            isDead.Value = false;

            playerNetworkManager.currentHealth.Value = playerNetworkManager.maxHealth.Value;
            playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;
            
            //重生效果 如 减少血上限

            playerAnimatorManager.PlayerTargetActionAnimation("Empty", false);
        }
    }

    public void SaveGameToCurrentCharacterData(ref CharacterSaveData currentCharacterSaveData)
    {
        currentCharacterSaveData.sceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        currentCharacterSaveData.characterName = playerNetworkManager.characterName.Value.ToString();
        
        currentCharacterSaveData.xPosition = transform.position.x;
        currentCharacterSaveData.yPosition = transform.position.y;
        currentCharacterSaveData.zPosition = transform.position.z;

        currentCharacterSaveData.currentHealth = playerNetworkManager.currentHealth.Value;
        currentCharacterSaveData.currentStamina = playerNetworkManager.currentStamina.Value;
        
        currentCharacterSaveData.vitality = playerNetworkManager.vitality.Value;
        currentCharacterSaveData.endurance =  playerNetworkManager.endurance.Value;
    }

    public void LoadGameFromCurrentCharacterData(ref CharacterSaveData currentCharacterSaveData)
    {
        playerNetworkManager.characterName.Value = currentCharacterSaveData.characterName;

        Vector3 myPosition = new Vector3(
            currentCharacterSaveData.xPosition, 
            currentCharacterSaveData.yPosition,
            currentCharacterSaveData.zPosition);
        
        transform.position = myPosition;
        
        playerNetworkManager.vitality.Value = currentCharacterSaveData.vitality;
        playerNetworkManager.endurance.Value = currentCharacterSaveData.endurance;

        playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnVitalityLevel(playerNetworkManager.vitality.Value);
        playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(playerNetworkManager.endurance.Value);
        
        playerNetworkManager.currentHealth.Value = currentCharacterSaveData.currentHealth;
        playerNetworkManager.currentStamina.Value = currentCharacterSaveData.currentStamina;
        
        PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(playerNetworkManager.maxHealth.Value);
        PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(playerNetworkManager.maxStamina.Value);

        //自己加的
        PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue(playerNetworkManager.currentHealth.Value, playerNetworkManager.currentHealth.Value);
        PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue(playerNetworkManager.currentStamina.Value, playerNetworkManager.currentStamina.Value);
    }

    public void LoadOtherPlayerCharacterWhenJoingServer()
    {
        playerNetworkManager.OnCurrentRightHandWeaponIDChanged(0, playerNetworkManager.currentRightHandWeaponID.Value);
        playerNetworkManager.OnCurrentLeftHandWeaponIDChanged(0, playerNetworkManager.currentLeftHandWeaponID.Value);

        //Lock On
        if (playerNetworkManager.isLockOn.Value)
        {
            playerNetworkManager.OnLockOnTargetIDChange(0, playerNetworkManager.currentTargetNetworkObjectID.Value);
        }
    }

    public void DebugMenu()
    {
        if (respawnCharacter)
        {
            respawnCharacter = false;
            ReviveCharacter();
        }

        if(switchRightWeapon)
        {
            switchRightWeapon = false;
            playerEquipmentManager.SwitchRightWeapon();
        }
    }
}
