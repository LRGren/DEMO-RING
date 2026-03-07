using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : CharacterManager
{
    [Header("DEBUG MENU")]
    [SerializeField] private bool respawnCharacter = false;
    
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
    [HideInInspector] public PlayerNetworkManager playerNetworkManager;
    [HideInInspector] public PlayerStatsManager playerStatsManager;
    [HideInInspector] public PlayerInventoryManager playerInventoryManager;
    
    protected override void Awake()
    {
        base.Awake();
        
        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
        playerNetworkManager = GetComponent<PlayerNetworkManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
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

        playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.CheckHP;
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
            playerNetworkManager.currentHealth.Value = playerNetworkManager.maxHealth.Value;
            playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;
            
            //重生效果 如 减少血上限

            playerAnimatorManager.PlayerTargetAnimation("Empty", false);
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
    }

    public void DebugMenu()
    {
        if (respawnCharacter)
        {
            respawnCharacter = false;
            ReviveCharacter();
        }
    }
}
