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
    [HideInInspector] public PlayerInteractionManager playerInteractionManager;
    [HideInInspector] public PlayerEffectsManager playerEffectsManager;
    [HideInInspector] public PlayerBodyManager playerBodyManager;

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
        playerInteractionManager = GetComponent<PlayerInteractionManager>();
        playerEffectsManager = GetComponent<PlayerEffectsManager>();
        playerBodyManager = GetComponent<PlayerBodyManager>();
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
            playerNetworkManager.mind.OnValueChanged += playerNetworkManager.SetNewMaxFocusValue;
            playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;

            //更新状态条
            playerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
            playerNetworkManager.currentStamina.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
            playerNetworkManager.currentFocusPoints.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewFocusValue;
            playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaTimer;

            playerNetworkManager.isAiming.OnValueChanged += playerNetworkManager.OnIsAimingChanged;
        }

        if (!IsOwner)
        {
            playerNetworkManager.currentHealth.OnValueChanged += characterUIManager.OnHPChanged;
        }

        playerNetworkManager.isMale.OnValueChanged += playerNetworkManager.OnIsMaleChanged;

        //状态
        playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.CheckHP;
        playerNetworkManager.currentFocusPoints.OnValueChanged += playerNetworkManager.CheckFP;

        //武器
        playerNetworkManager.currentRightHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightHandWeaponIDChanged;
        playerNetworkManager.currentLeftHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftHandWeaponIDChanged;
        playerNetworkManager.currentWeaponBeingUsed.OnValueChanged += playerNetworkManager.OnCurrentWeaponBedingUsedIDChanged;
        playerNetworkManager.isBlocking.OnValueChanged += playerNetworkManager.OnIsBlockingChanged;

        playerNetworkManager.currentSpellID.OnValueChanged += playerNetworkManager.OnCurrentSpellIDChanged;

        //盔甲
        playerNetworkManager.headEquipmentID.OnValueChanged += playerNetworkManager.OnHeadEquipmentIDChanged;
        playerNetworkManager.bodyEquipmentID.OnValueChanged += playerNetworkManager.OnBodyEquipmentIDChanged;
        playerNetworkManager.handEquipmentID.OnValueChanged += playerNetworkManager.OnHandEquipmentIDChanged;
        playerNetworkManager.legEquipmentID.OnValueChanged += playerNetworkManager.OnLegEquipmentIDChanged;

        if (!IsOwner)
        {
            SyncRemoteArmorFromNetworkVariables();
        }

        if (IsOwner)
        {
            playerNetworkManager.currentRightHandWeaponID.Value =
                playerInventoryManager.weaponsInRightHand[playerInventoryManager.rightWeaponIndex].itemID;
            playerNetworkManager.currentLeftHandWeaponID.Value =
                playerInventoryManager.weaponsInLeftHand[playerInventoryManager.leftWeaponIndex].itemID;
        }

        //锁定
        playerNetworkManager.isLockOn.OnValueChanged += playerNetworkManager.OnIsLockOnChanged;
        playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged += playerNetworkManager.OnLockOnTargetIDChange;

        //FLAGS
        playerNetworkManager.isChargingAttack.OnValueChanged += playerNetworkManager.OnIsChargingAttackChanged;

        //Spell
        playerNetworkManager.isChargingRightSpell.OnValueChanged += playerNetworkManager.OnIsChargingRightSpellChanged;
        playerNetworkManager.isChargingLeftSpell.OnValueChanged += playerNetworkManager.OnIsChargingLeftSpellChanged;

        //QuickSlotItem
        playerNetworkManager.currentQuickSlotItemID.OnValueChanged += playerNetworkManager.OnCurrentQuickSlotItemIDChanged;
        playerNetworkManager.isChugging.OnValueChanged += playerNetworkManager.OnIsChuggingChanged;

        //Two Handed
        playerNetworkManager.isTwoHandingWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingWeaponChanged;
        playerNetworkManager.isTwoHandingRightWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingRightWeaponChanged;
        playerNetworkManager.isTwoHandingLeftWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingLeftWeaponChanged;

        //Projectile
        playerNetworkManager.mainProjectileID.OnValueChanged += playerNetworkManager.OnMainProjectileIDChanged;
        playerNetworkManager.secondaryProjectileID.OnValueChanged += playerNetworkManager.OnSecondaryProjectileIDChanged;
        playerNetworkManager.isHoldingArrow.OnValueChanged += playerNetworkManager.OnIsHoldingArrowChanged;

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
            playerNetworkManager.mind.OnValueChanged -= playerNetworkManager.SetNewMaxFocusValue;
            playerNetworkManager.endurance.OnValueChanged -= playerNetworkManager.SetNewMaxStaminaValue;

            //更新状态条
            playerNetworkManager.currentHealth.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
            playerNetworkManager.currentStamina.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
            playerNetworkManager.currentFocusPoints.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewFocusValue;
            playerNetworkManager.currentStamina.OnValueChanged -= playerStatsManager.ResetStaminaTimer;

            playerNetworkManager.isAiming.OnValueChanged -= playerNetworkManager.OnIsAimingChanged;
        }

        if (!IsOwner)
        {
            playerNetworkManager.currentHealth.OnValueChanged -= characterUIManager.OnHPChanged;
        }

        playerNetworkManager.isMale.OnValueChanged -= playerNetworkManager.OnIsMaleChanged;

        //状态
        playerNetworkManager.currentHealth.OnValueChanged -= playerNetworkManager.CheckHP;
        playerNetworkManager.currentFocusPoints.OnValueChanged -= playerNetworkManager.CheckFP;

        //武器
        playerNetworkManager.currentRightHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentRightHandWeaponIDChanged;
        playerNetworkManager.currentLeftHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentLeftHandWeaponIDChanged;
        playerNetworkManager.currentWeaponBeingUsed.OnValueChanged -= playerNetworkManager.OnCurrentWeaponBedingUsedIDChanged;
        playerNetworkManager.isBlocking.OnValueChanged -= playerNetworkManager.OnIsBlockingChanged;

        playerNetworkManager.currentSpellID.OnValueChanged -= playerNetworkManager.OnCurrentSpellIDChanged;

        //盔甲
        playerNetworkManager.headEquipmentID.OnValueChanged -= playerNetworkManager.OnHeadEquipmentIDChanged;
        playerNetworkManager.bodyEquipmentID.OnValueChanged -= playerNetworkManager.OnBodyEquipmentIDChanged;
        playerNetworkManager.handEquipmentID.OnValueChanged -= playerNetworkManager.OnHandEquipmentIDChanged;
        playerNetworkManager.legEquipmentID.OnValueChanged -= playerNetworkManager.OnLegEquipmentIDChanged;

        //锁定
        playerNetworkManager.isLockOn.OnValueChanged -= playerNetworkManager.OnIsLockOnChanged;
        playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged -= playerNetworkManager.OnLockOnTargetIDChange;

        //Spell
        playerNetworkManager.isChargingRightSpell.OnValueChanged -= playerNetworkManager.OnIsChargingRightSpellChanged;
        playerNetworkManager.isChargingLeftSpell.OnValueChanged -= playerNetworkManager.OnIsChargingLeftSpellChanged;

        //QuickSlotItem
        playerNetworkManager.currentQuickSlotItemID.OnValueChanged -= playerNetworkManager.OnCurrentQuickSlotItemIDChanged;
        playerNetworkManager.isChugging.OnValueChanged -= playerNetworkManager.OnIsChuggingChanged;

        //Two Handed
        playerNetworkManager.isTwoHandingWeapon.OnValueChanged -= playerNetworkManager.OnIsTwoHandingWeaponChanged;
        playerNetworkManager.isTwoHandingRightWeapon.OnValueChanged -= playerNetworkManager.OnIsTwoHandingRightWeaponChanged;
        playerNetworkManager.isTwoHandingLeftWeapon.OnValueChanged -= playerNetworkManager.OnIsTwoHandingLeftWeaponChanged;

        //Projectile
        playerNetworkManager.mainProjectileID.OnValueChanged -= playerNetworkManager.OnMainProjectileIDChanged;
        playerNetworkManager.secondaryProjectileID.OnValueChanged -= playerNetworkManager.OnSecondaryProjectileIDChanged;
        playerNetworkManager.isHoldingArrow.OnValueChanged -= playerNetworkManager.OnIsHoldingArrowChanged;

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

        currentCharacterSaveData.isMale = playerNetworkManager.isMale.Value;

        currentCharacterSaveData.xPosition = transform.position.x;
        currentCharacterSaveData.yPosition = transform.position.y;
        currentCharacterSaveData.zPosition = transform.position.z;

        currentCharacterSaveData.currentHealth = playerNetworkManager.currentHealth.Value;
        currentCharacterSaveData.currentStamina = playerNetworkManager.currentStamina.Value;

        currentCharacterSaveData.vitality = playerNetworkManager.vitality.Value;
        currentCharacterSaveData.endurance = playerNetworkManager.endurance.Value;
        currentCharacterSaveData.mind = playerNetworkManager.mind.Value;

        currentCharacterSaveData.siteOfGraceActivated = WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated;

        currentCharacterSaveData.bossesAwakened = WorldSaveGameManager.instance.currentCharacterData.bossesAwakened;
        currentCharacterSaveData.bossesDefeated = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated;

        // Equipment
        currentCharacterSaveData.currentHeadEquipment = playerNetworkManager.headEquipmentID.Value;
        currentCharacterSaveData.currentBodyEquipment = playerNetworkManager.bodyEquipmentID.Value;
        currentCharacterSaveData.currentLegsEquipment = playerNetworkManager.legEquipmentID.Value;
        currentCharacterSaveData.currentHandEquipment = playerNetworkManager.handEquipmentID.Value;

        currentCharacterSaveData.currentRightWeaponIndex = playerInventoryManager.rightWeaponIndex;
        currentCharacterSaveData.rightWeapon01 = playerInventoryManager.weaponsInRightHand[0].itemID;
        currentCharacterSaveData.rightWeapon02 = playerInventoryManager.weaponsInRightHand[1].itemID;
        currentCharacterSaveData.rightWeapon03 = playerInventoryManager.weaponsInRightHand[2].itemID;

        currentCharacterSaveData.currentLeftWeaponIndex = playerInventoryManager.leftWeaponIndex;
        currentCharacterSaveData.leftWeapon01 = playerInventoryManager.weaponsInLeftHand[0].itemID;
        currentCharacterSaveData.leftWeapon02 = playerInventoryManager.weaponsInLeftHand[1].itemID;
        currentCharacterSaveData.leftWeapon03 = playerInventoryManager.weaponsInLeftHand[2].itemID;

        currentCharacterSaveData.currentSpell = playerNetworkManager.currentSpellID.Value;

        currentCharacterSaveData.currentQuickSlotItemIndex = playerInventoryManager.currentQuickSlotItemIndex;
        currentCharacterSaveData.quickSlotItem01 = playerInventoryManager.quickSlotItemInventory[0] != null ? playerInventoryManager.quickSlotItemInventory[0].itemID : -1;
        currentCharacterSaveData.quickSlotItem02 = playerInventoryManager.quickSlotItemInventory[1] != null ? playerInventoryManager.quickSlotItemInventory[1].itemID : -1;
        currentCharacterSaveData.quickSlotItem03 = playerInventoryManager.quickSlotItemInventory[2] != null ? playerInventoryManager.quickSlotItemInventory[2].itemID : -1;
        currentCharacterSaveData.quickSlotItem04 = playerInventoryManager.quickSlotItemInventory[3] != null ? playerInventoryManager.quickSlotItemInventory[3].itemID : -1;
        currentCharacterSaveData.quickSlotItem05 = playerInventoryManager.quickSlotItemInventory[4] != null ? playerInventoryManager.quickSlotItemInventory[4].itemID : -1;
        currentCharacterSaveData.quickSlotItem06 = playerInventoryManager.quickSlotItemInventory[5] != null ? playerInventoryManager.quickSlotItemInventory[5].itemID : -1;
        currentCharacterSaveData.quickSlotItem07 = playerInventoryManager.quickSlotItemInventory[6] != null ? playerInventoryManager.quickSlotItemInventory[6].itemID : -1;
        currentCharacterSaveData.quickSlotItem08 = playerInventoryManager.quickSlotItemInventory[7] != null ? playerInventoryManager.quickSlotItemInventory[7].itemID : -1;
        currentCharacterSaveData.quickSlotItem09 = playerInventoryManager.quickSlotItemInventory[8] != null ? playerInventoryManager.quickSlotItemInventory[8].itemID : -1;
        currentCharacterSaveData.quickSlotItem10 = playerInventoryManager.quickSlotItemInventory[9] != null ? playerInventoryManager.quickSlotItemInventory[9].itemID : -1;

        currentCharacterSaveData.quickSlotItem01Amount = playerInventoryManager.quickSlotItemInventory[0] != null ? playerInventoryManager.quickSlotItemInventory[0].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem02Amount = playerInventoryManager.quickSlotItemInventory[1] != null ? playerInventoryManager.quickSlotItemInventory[1].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem03Amount = playerInventoryManager.quickSlotItemInventory[2] != null ? playerInventoryManager.quickSlotItemInventory[2].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem04Amount = playerInventoryManager.quickSlotItemInventory[3] != null ? playerInventoryManager.quickSlotItemInventory[3].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem05Amount = playerInventoryManager.quickSlotItemInventory[4] != null ? playerInventoryManager.quickSlotItemInventory[4].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem06Amount = playerInventoryManager.quickSlotItemInventory[5] != null ? playerInventoryManager.quickSlotItemInventory[5].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem07Amount = playerInventoryManager.quickSlotItemInventory[6] != null ? playerInventoryManager.quickSlotItemInventory[6].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem08Amount = playerInventoryManager.quickSlotItemInventory[7] != null ? playerInventoryManager.quickSlotItemInventory[7].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem09Amount = playerInventoryManager.quickSlotItemInventory[8] != null ? playerInventoryManager.quickSlotItemInventory[8].GetAmountOfItem(this) : 0;
        currentCharacterSaveData.quickSlotItem10Amount = playerInventoryManager.quickSlotItemInventory[9] != null ? playerInventoryManager.quickSlotItemInventory[9].GetAmountOfItem(this) : 0;

        currentCharacterSaveData.currentMainProjectile = playerNetworkManager.mainProjectileID.Value;
        currentCharacterSaveData.currentSecondaryProjectile = playerNetworkManager.secondaryProjectileID.Value;

        currentCharacterSaveData.currentMainAmmoAmount = playerInventoryManager.mainProjectile != null ? playerInventoryManager.mainProjectile.currentAmmoAmount : 0;
        currentCharacterSaveData.currentSecondaryAmmoAmount = playerInventoryManager.secondaryProjectile != null ? playerInventoryManager.secondaryProjectile.currentAmmoAmount : 0;

        currentCharacterSaveData.currentMainProjectile = playerNetworkManager.mainProjectileID.Value;
        currentCharacterSaveData.currentSecondaryProjectile = playerNetworkManager.secondaryProjectileID.Value;

        // Spell
        if (playerInventoryManager.currentSpell != null)
            currentCharacterSaveData.currentSpell = playerInventoryManager.currentSpell.itemID;
        else
            currentCharacterSaveData.currentSpell = -1;

        // Inventory
        currentCharacterSaveData.inventory = playerInventoryManager.characterInventory;
    }

    /// <summary>
    /// (临时添加用以测试) 将当前boss的状态保存到当前角色数据中
    /// </summary>
    public void SaveGameBossInfoToCurrentCharacterData(ref CharacterSaveData currentCharacterSaveData, string bossID, bool isAwakened, bool isDefeated)
    {
        if (!currentCharacterSaveData.bossesAwakened.ContainsKey(bossID))
        {
            currentCharacterSaveData.bossesAwakened.Add(bossID, isAwakened);
        }
        else
        {
            currentCharacterSaveData.bossesAwakened[bossID] = isAwakened;
        }

        if (!currentCharacterSaveData.bossesDefeated.ContainsKey(bossID))
        {
            currentCharacterSaveData.bossesDefeated.Add(bossID, isDefeated);
        }
        else
        {
            currentCharacterSaveData.bossesDefeated[bossID] = isDefeated;
        }
    }

    public void LoadGameFromCurrentCharacterData(ref CharacterSaveData currentCharacterSaveData)
    {
        playerNetworkManager.characterName.Value = currentCharacterSaveData.characterName;

        playerNetworkManager.isMale.Value = currentCharacterSaveData.isMale;
        playerBodyManager.ToggleBodyType(playerNetworkManager.isMale.Value);

        Vector3 myPosition = new Vector3(
            currentCharacterSaveData.xPosition,
            currentCharacterSaveData.yPosition,
            currentCharacterSaveData.zPosition);

        characterController.enabled = false;
        transform.position = myPosition;
        characterController.enabled = true;

        playerNetworkManager.vitality.Value = currentCharacterSaveData.vitality;
        playerNetworkManager.endurance.Value = currentCharacterSaveData.endurance;
        playerNetworkManager.mind.Value = currentCharacterSaveData.mind;

        playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnVitalityLevel(playerNetworkManager.vitality.Value);
        playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(playerNetworkManager.endurance.Value);
        playerNetworkManager.maxFocus.Value = playerStatsManager.CalculateFocusBasedOnMindLevel(playerNetworkManager.mind.Value);

        playerNetworkManager.currentHealth.Value = currentCharacterSaveData.currentHealth;
        playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;

        PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(playerNetworkManager.maxHealth.Value);
        PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(playerNetworkManager.maxStamina.Value);

        PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue(playerNetworkManager.currentHealth.Value, playerNetworkManager.currentHealth.Value);
        PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue(playerNetworkManager.currentStamina.Value, playerNetworkManager.maxStamina.Value);

        // Equipment
        if (WorldItemDatabase.instance.GetHeadEquipmentByID(currentCharacterSaveData.currentHeadEquipment) != null)
            playerInventoryManager.headEquipment = Instantiate(WorldItemDatabase.instance.GetHeadEquipmentByID(currentCharacterSaveData.currentHeadEquipment));
        else
            playerInventoryManager.headEquipment = null;
        if (WorldItemDatabase.instance.GetBodyEquipmentByID(currentCharacterSaveData.currentBodyEquipment) != null)
            playerInventoryManager.bodyEquipment = Instantiate(WorldItemDatabase.instance.GetBodyEquipmentByID(currentCharacterSaveData.currentBodyEquipment));
        else
            playerInventoryManager.bodyEquipment = null;
        if (WorldItemDatabase.instance.GetLegEquipmentByID(currentCharacterSaveData.currentLegsEquipment) != null)
            playerInventoryManager.legEquipment = Instantiate(WorldItemDatabase.instance.GetLegEquipmentByID(currentCharacterSaveData.currentLegsEquipment));
        else
            playerInventoryManager.legEquipment = null;
        if (WorldItemDatabase.instance.GetHandEquipmentByID(currentCharacterSaveData.currentHandEquipment) != null)
            playerInventoryManager.handEquipment = Instantiate(WorldItemDatabase.instance.GetHandEquipmentByID(currentCharacterSaveData.currentHandEquipment));
        else
            playerInventoryManager.handEquipment = null;


        // Weapons Right Hand
        if (WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.rightWeapon01) != null)
            playerInventoryManager.weaponsInRightHand[0] = Instantiate(WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.rightWeapon01));
        else
            playerInventoryManager.weaponsInRightHand[0] = Instantiate(WorldItemDatabase.instance.unarmedWeapon);
        if (WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.rightWeapon02) != null)
            playerInventoryManager.weaponsInRightHand[1] = Instantiate(WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.rightWeapon02));
        else
            playerInventoryManager.weaponsInRightHand[1] = Instantiate(WorldItemDatabase.instance.unarmedWeapon); ;
        if (WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.rightWeapon03) != null)
            playerInventoryManager.weaponsInRightHand[2] = Instantiate(WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.rightWeapon03));
        else
            playerInventoryManager.weaponsInRightHand[2] = Instantiate(WorldItemDatabase.instance.unarmedWeapon); ;


        // Weapons Left Hand
        if (WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.leftWeapon01) != null)
            playerInventoryManager.weaponsInLeftHand[0] = Instantiate(WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.leftWeapon01));
        else
            playerInventoryManager.weaponsInLeftHand[0] = Instantiate(WorldItemDatabase.instance.unarmedWeapon); ;
        if (WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.leftWeapon02) != null)
            playerInventoryManager.weaponsInLeftHand[1] = Instantiate(WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.leftWeapon02));
        else
            playerInventoryManager.weaponsInLeftHand[1] = Instantiate(WorldItemDatabase.instance.unarmedWeapon); ;
        if (WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.leftWeapon03) != null)
            playerInventoryManager.weaponsInLeftHand[2] = Instantiate(WorldItemDatabase.instance.GetWeaponByID(currentCharacterSaveData.leftWeapon03));
        else
            playerInventoryManager.weaponsInLeftHand[2] = Instantiate(WorldItemDatabase.instance.unarmedWeapon); ;


        // Spell
        if (WorldItemDatabase.instance.GetSpellByID(currentCharacterSaveData.currentSpell) != null)
            playerNetworkManager.currentSpellID.Value = Instantiate(WorldItemDatabase.instance.GetSpellByID(currentCharacterSaveData.currentSpell)).itemID;
        else
            playerNetworkManager.currentSpellID.Value = -1;

        // Quick Slot Items
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem01) != null)
        {
            playerInventoryManager.quickSlotItemInventory[0] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem01));
            playerInventoryManager.quickSlotItemInventory[0].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem01Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[0] = null;

        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem02) != null)
        {
            playerInventoryManager.quickSlotItemInventory[1] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem02));
            playerInventoryManager.quickSlotItemInventory[1].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem02Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[1] = null;

        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem03) != null)
        {
            playerInventoryManager.quickSlotItemInventory[2] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem03));
            playerInventoryManager.quickSlotItemInventory[2].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem03Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[2] = null;

        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem04) != null)
        {
            playerInventoryManager.quickSlotItemInventory[3] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem04));
            playerInventoryManager.quickSlotItemInventory[3].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem04Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[3] = null;
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem05) != null)
        {
            playerInventoryManager.quickSlotItemInventory[4] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem05));
            playerInventoryManager.quickSlotItemInventory[4].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem05Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[4] = null;
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem06) != null)
        {
            playerInventoryManager.quickSlotItemInventory[5] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem06));
            playerInventoryManager.quickSlotItemInventory[5].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem06Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[5] = null;
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem07) != null)
        {
            playerInventoryManager.quickSlotItemInventory[6] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem07));
            playerInventoryManager.quickSlotItemInventory[6].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem07Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[6] = null;
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem08) != null)
        {
            playerInventoryManager.quickSlotItemInventory[7] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem08));
            playerInventoryManager.quickSlotItemInventory[7].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem08Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[7] = null;
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem09) != null)
        {
            playerInventoryManager.quickSlotItemInventory[8] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem09));
            playerInventoryManager.quickSlotItemInventory[8].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem09Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[8] = null;
        if (WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem10) != null)
        {
            playerInventoryManager.quickSlotItemInventory[9] = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(currentCharacterSaveData.quickSlotItem10));
            playerInventoryManager.quickSlotItemInventory[9].SetAmountOfItem(this, currentCharacterSaveData.quickSlotItem10Amount);
        }
        else
            playerInventoryManager.quickSlotItemInventory[9] = null;

        RangedProjectileItem mainProjectile = WorldItemDatabase.instance.GetProjectileByID(currentCharacterSaveData.currentMainProjectile);
        if (mainProjectile != null)
        {
            mainProjectile = Instantiate(mainProjectile);
            mainProjectile.SetAmmotAmount(currentCharacterSaveData.currentMainAmmoAmount);
        }
        playerEquipmentManager.LoadMainProjectileEquipment(mainProjectile);

        RangedProjectileItem secondaryProjectile = WorldItemDatabase.instance.GetProjectileByID(currentCharacterSaveData.currentSecondaryProjectile);
        if (secondaryProjectile != null)
        {
            secondaryProjectile = Instantiate(secondaryProjectile);
            secondaryProjectile.SetAmmotAmount(currentCharacterSaveData.currentSecondaryAmmoAmount);
        }
        playerEquipmentManager.LoadSecondaryProjectileEquipment(secondaryProjectile);

        playerEquipmentManager.EquipArmor();

        if (currentCharacterSaveData.currentRightWeaponIndex == -1)
            playerInventoryManager.rightWeaponIndex = 0;
        else
            playerInventoryManager.rightWeaponIndex = currentCharacterSaveData.currentRightWeaponIndex;

        if (currentCharacterSaveData.currentLeftWeaponIndex == -1)
            playerInventoryManager.leftWeaponIndex = 0;
        else
            playerInventoryManager.leftWeaponIndex = currentCharacterSaveData.currentLeftWeaponIndex;

        if (currentCharacterSaveData.currentQuickSlotItemIndex == -1)
            playerInventoryManager.currentQuickSlotItemIndex = 0;
        else
        {
            playerInventoryManager.currentQuickSlotItemIndex = currentCharacterSaveData.currentQuickSlotItemIndex;
            playerInventoryManager.currentQuickSlotItem = playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex];

            if (playerInventoryManager.currentQuickSlotItem != null)
                PlayerUIManager.instance.playerUIHudManager.SetQuickSlotCountText(playerInventoryManager.currentQuickSlotItem.GetAmountOfItem(this), playerInventoryManager.currentQuickSlotItem.isConsumable);
        }

        playerNetworkManager.currentRightHandWeaponID.Value = playerInventoryManager.weaponsInRightHand[playerInventoryManager.rightWeaponIndex].itemID;
        playerNetworkManager.currentLeftHandWeaponID.Value = playerInventoryManager.weaponsInLeftHand[playerInventoryManager.leftWeaponIndex].itemID;
        playerNetworkManager.currentSpellID.Value = currentCharacterSaveData.currentSpell;
        playerNetworkManager.currentQuickSlotItemID.Value = playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex] != null ? playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex].itemID : -1;

        playerInventoryManager.characterInventory = currentCharacterSaveData.inventory;

    }

    public void LoadOtherPlayerCharacterWhenJoingServer()
    {
        // Body Type
        playerNetworkManager.OnIsMaleChanged(false, playerNetworkManager.isMale.Value);

        // Equipment
        playerNetworkManager.OnCurrentRightHandWeaponIDChanged(0, playerNetworkManager.currentRightHandWeaponID.Value);
        playerNetworkManager.OnCurrentLeftHandWeaponIDChanged(0, playerNetworkManager.currentLeftHandWeaponID.Value);

        // Spell
        playerNetworkManager.OnCurrentSpellIDChanged(-1, playerNetworkManager.currentSpellID.Value);

        // Quick Slot Item
        playerNetworkManager.OnCurrentQuickSlotItemIDChanged(-1, playerNetworkManager.currentQuickSlotItemID.Value);

        //Block
        playerNetworkManager.OnIsBlockingChanged(false, playerNetworkManager.isBlocking.Value);

        //Two Handed
        playerNetworkManager.OnIsTwoHandingWeaponChanged(false, playerNetworkManager.isTwoHandingWeapon.Value);
        playerNetworkManager.OnIsTwoHandingRightWeaponChanged(false, playerNetworkManager.isTwoHandingRightWeapon.Value);
        playerNetworkManager.OnIsTwoHandingLeftWeaponChanged(false, playerNetworkManager.isTwoHandingLeftWeapon.Value);

        //Projectile
        playerNetworkManager.OnMainProjectileIDChanged(0, playerNetworkManager.mainProjectileID.Value);
        playerNetworkManager.OnSecondaryProjectileIDChanged(0, playerNetworkManager.secondaryProjectileID.Value);
        playerNetworkManager.OnIsHoldingArrowChanged(false, playerNetworkManager.isHoldingArrow.Value);

        //Lock On
        if (playerNetworkManager.isLockOn.Value)
        {
            playerNetworkManager.OnLockOnTargetIDChange(0, playerNetworkManager.currentTargetNetworkObjectID.Value);
        }
    }

    private void SyncRemoteArmorFromNetworkVariables()
    {
        playerNetworkManager.OnHeadEquipmentIDChanged(-1, playerNetworkManager.headEquipmentID.Value);
        playerNetworkManager.OnBodyEquipmentIDChanged(-1, playerNetworkManager.bodyEquipmentID.Value);
        playerNetworkManager.OnHandEquipmentIDChanged(-1, playerNetworkManager.handEquipmentID.Value);
        playerNetworkManager.OnLegEquipmentIDChanged(-1, playerNetworkManager.legEquipmentID.Value);
    }

    public void DebugMenu()
    {
        if (respawnCharacter)
        {
            respawnCharacter = false;
            ReviveCharacter();
        }

        if (switchRightWeapon)
        {
            switchRightWeapon = false;
            playerEquipmentManager.SwitchRightWeapon();
        }
    }

}
