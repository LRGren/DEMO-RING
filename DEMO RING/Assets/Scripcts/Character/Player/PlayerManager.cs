using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

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
            playerNetworkManager.vigor.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
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
            playerEquipmentManager.RefreshRightWeapon();
            playerEquipmentManager.RefreshLeftWeapon();
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
            playerNetworkManager.vigor.OnValueChanged -= playerNetworkManager.SetNewMaxHealthValue;
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

    private T CloneOptionalItem<T>(T template) where T : UnityEngine.Object
    {
        return template != null ? Instantiate(template) : null;
    }

    public void InitializeNewCharacter(string characterName, StartingClassDefinition startingClass)
    {
        playerInventoryManager.characterInventory.Clear();

        playerNetworkManager.characterName.Value = string.IsNullOrWhiteSpace(characterName) ? "Tarnished" : characterName;

        playerNetworkManager.level.Value = startingClass.level;
        playerNetworkManager.vigor.Value = startingClass.vigor;
        playerNetworkManager.mind.Value = startingClass.mind;
        playerNetworkManager.endurance.Value = startingClass.endurance;
        playerNetworkManager.strength.Value = startingClass.strength;
        playerNetworkManager.dexterity.Value = startingClass.dexterity;
        playerNetworkManager.intelligence.Value = startingClass.intelligence;
        playerNetworkManager.faith.Value = startingClass.faith;
        playerNetworkManager.arcane.Value = startingClass.arcane;

        //设置初始武器
        playerInventoryManager.rightWeaponIndex = 0;
        playerInventoryManager.weaponsInRightHand[0] = Instantiate(startingClass.startingRightWeapon01 ? startingClass.startingRightWeapon01 : WorldItemDatabase.instance.unarmedWeapon);
        playerInventoryManager.weaponsInRightHand[1] = Instantiate(startingClass.startingRightWeapon02 ? startingClass.startingRightWeapon02 : WorldItemDatabase.instance.unarmedWeapon);
        playerInventoryManager.weaponsInRightHand[2] = Instantiate(startingClass.startingRightWeapon03 ? startingClass.startingRightWeapon03 : WorldItemDatabase.instance.unarmedWeapon);

        playerInventoryManager.leftWeaponIndex = 0;
        playerInventoryManager.weaponsInLeftHand[0] = Instantiate(startingClass.startingLeftWeapon01 ? startingClass.startingLeftWeapon01 : WorldItemDatabase.instance.unarmedWeapon);
        playerInventoryManager.weaponsInLeftHand[1] = Instantiate(startingClass.startingLeftWeapon02 ? startingClass.startingLeftWeapon02 : WorldItemDatabase.instance.unarmedWeapon);
        playerInventoryManager.weaponsInLeftHand[2] = Instantiate(startingClass.startingLeftWeapon03 ? startingClass.startingLeftWeapon03 : WorldItemDatabase.instance.unarmedWeapon);

        //设置初始装备
        playerInventoryManager.headEquipment = CloneOptionalItem(startingClass.startingHeadEquipment);
        playerInventoryManager.bodyEquipment = CloneOptionalItem(startingClass.startingBodyEquipment);
        playerInventoryManager.handEquipment = CloneOptionalItem(startingClass.startingHandEquipment);
        playerInventoryManager.legEquipment = CloneOptionalItem(startingClass.startingLegEquipment);

        //设置初始快捷栏物品
        playerInventoryManager.currentQuickSlotItemIndex = 0;
        playerInventoryManager.quickSlotItemInventory[0] = CloneOptionalItem(startingClass.startingQuickSlotItem01);
        playerInventoryManager.quickSlotItemInventory[1] = CloneOptionalItem(startingClass.startingQuickSlotItem02);
        playerInventoryManager.quickSlotItemInventory[2] = CloneOptionalItem(startingClass.startingQuickSlotItem03);
        playerInventoryManager.quickSlotItemInventory[3] = CloneOptionalItem(startingClass.startingQuickSlotItem04);
        playerInventoryManager.quickSlotItemInventory[4] = CloneOptionalItem(startingClass.startingQuickSlotItem05);
        playerInventoryManager.quickSlotItemInventory[5] = CloneOptionalItem(startingClass.startingQuickSlotItem06);
        playerInventoryManager.quickSlotItemInventory[6] = CloneOptionalItem(startingClass.startingQuickSlotItem07);
        playerInventoryManager.quickSlotItemInventory[7] = CloneOptionalItem(startingClass.startingQuickSlotItem08);
        playerInventoryManager.quickSlotItemInventory[8] = CloneOptionalItem(startingClass.startingQuickSlotItem09);
        playerInventoryManager.quickSlotItemInventory[9] = CloneOptionalItem(startingClass.startingQuickSlotItem10);

        //设置初始远程弹药
        playerInventoryManager.mainProjectile = CloneOptionalItem(startingClass.startingMainRangedProjectile);
        playerInventoryManager.secondaryProjectile = CloneOptionalItem(startingClass.startingSecondaryRangedProjectile);

        // 设置初始弹药数量
        if (playerInventoryManager.mainProjectile != null)
        {
            playerInventoryManager.mainProjectile.SetAmmotAmount(
                Mathf.Clamp(
                    startingClass.startingMainRangedProjectileAmount,
                    0,
                    playerInventoryManager.mainProjectile.maxAmmoAmount));
        }
        if (playerInventoryManager.secondaryProjectile != null)
        {
            playerInventoryManager.secondaryProjectile.SetAmmotAmount(
                Mathf.Clamp(
                    startingClass.startingSecondaryRangedProjectileAmount,
                    0,
                    playerInventoryManager.secondaryProjectile.maxAmmoAmount));
        }

        playerNetworkManager.remainingHealthFlasks.Value =
            Mathf.Max(0, startingClass.startingHealthFlaskCount);
        playerNetworkManager.remainingManaFlasks.Value =
            Mathf.Max(0, startingClass.startingManaFlaskCount);

        //设置初始法术
        playerInventoryManager.currentSpell = CloneOptionalItem(startingClass.startingSpell01);

        playerNetworkManager.isTwoHandingWeapon.Value = false;
        playerNetworkManager.isTwoHandingRightWeapon.Value = false;
        playerNetworkManager.isTwoHandingLeftWeapon.Value = false;

        // 根据新角色的属性，主动计算资源上限
        playerNetworkManager.maxHealth.Value =
            playerStatsManager.CalculateHealthBasedOnVitalityLevel(
                playerNetworkManager.vigor.Value);

        playerNetworkManager.maxStamina.Value =
            playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(
                playerNetworkManager.endurance.Value);

        playerNetworkManager.maxFocus.Value =
            playerStatsManager.CalculateFocusBasedOnMindLevel(
                playerNetworkManager.mind.Value);

        // 先更新状态条上限
        var hud = PlayerUIManager.instance.playerUIHudManager;
        hud.SetMaxHealthValue(playerNetworkManager.maxHealth.Value);
        hud.SetMaxStaminaValue(playerNetworkManager.maxStamina.Value);
        hud.SetMaxFocusValue(playerNetworkManager.maxFocus.Value);

        // 新角色以满状态开始
        playerNetworkManager.currentHealth.Value =
            playerNetworkManager.maxHealth.Value;
        playerNetworkManager.currentStamina.Value =
            playerNetworkManager.maxStamina.Value;
        playerNetworkManager.currentFocusPoints.Value =
            playerNetworkManager.maxFocus.Value;

        // 根据初始化后的武器数组，刷新当前武器、模型和网络 ID
        playerEquipmentManager.RefreshRightWeapon();
        playerEquipmentManager.RefreshLeftWeapon();

        // 根据初始化后的护甲，刷新模型和网络 ID
        playerEquipmentManager.EquipArmor();

        // 刷新弹药装备及网络 ID
        playerEquipmentManager.LoadMainProjectileEquipment(playerInventoryManager.mainProjectile);
        playerEquipmentManager.LoadSecondaryProjectileEquipment(playerInventoryManager.secondaryProjectile);

        playerInventoryManager.currentQuickSlotItem = playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex];
        playerNetworkManager.currentQuickSlotItemID.Value = playerInventoryManager.currentQuickSlotItem != null ? playerInventoryManager.currentQuickSlotItem.itemID : -1;
        playerNetworkManager.currentSpellID.Value = playerInventoryManager.currentSpell != null ? playerInventoryManager.currentSpell.itemID : -1;
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
        currentCharacterSaveData.currentFocus = playerNetworkManager.currentFocusPoints.Value;

        currentCharacterSaveData.level = playerNetworkManager.level.Value;
        currentCharacterSaveData.vigor = playerNetworkManager.vigor.Value;
        currentCharacterSaveData.endurance = playerNetworkManager.endurance.Value;
        currentCharacterSaveData.mind = playerNetworkManager.mind.Value;
        currentCharacterSaveData.strength = playerNetworkManager.strength.Value;
        currentCharacterSaveData.dexterity = playerNetworkManager.dexterity.Value;
        currentCharacterSaveData.intelligence = playerNetworkManager.intelligence.Value;
        currentCharacterSaveData.faith = playerNetworkManager.faith.Value;
        currentCharacterSaveData.arcane = playerNetworkManager.arcane.Value;

        currentCharacterSaveData.siteOfGraceActivated = WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated;

        currentCharacterSaveData.bossesAwakened = WorldSaveGameManager.instance.currentCharacterData.bossesAwakened;
        currentCharacterSaveData.bossesDefeated = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated;

        // Equipment
        currentCharacterSaveData.currentHeadEquipment = playerNetworkManager.headEquipmentID.Value;
        currentCharacterSaveData.currentBodyEquipment = playerNetworkManager.bodyEquipmentID.Value;
        currentCharacterSaveData.currentLegsEquipment = playerNetworkManager.legEquipmentID.Value;
        currentCharacterSaveData.currentHandEquipment = playerNetworkManager.handEquipmentID.Value;

        currentCharacterSaveData.currentRightWeaponIndex = playerInventoryManager.rightWeaponIndex;
        currentCharacterSaveData.currentLeftWeaponIndex = playerInventoryManager.leftWeaponIndex;

        Func<WeaponItem, SerializableWeapon> Box = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem;
        currentCharacterSaveData.currentRightWeaponIndex = playerInventoryManager.rightWeaponIndex;
        currentCharacterSaveData.rightWeapon01 = Box(playerInventoryManager.weaponsInRightHand[0]);
        currentCharacterSaveData.rightWeapon02 = Box(playerInventoryManager.weaponsInRightHand[1]);
        currentCharacterSaveData.rightWeapon03 = Box(playerInventoryManager.weaponsInRightHand[2]);

        currentCharacterSaveData.currentLeftWeaponIndex = playerInventoryManager.leftWeaponIndex;
        currentCharacterSaveData.leftWeapon01 = Box(playerInventoryManager.weaponsInLeftHand[0]);
        currentCharacterSaveData.leftWeapon02 = Box(playerInventoryManager.weaponsInLeftHand[1]);
        currentCharacterSaveData.leftWeapon03 = Box(playerInventoryManager.weaponsInLeftHand[2]);


        currentCharacterSaveData.currentSpell = playerNetworkManager.currentSpellID.Value;

        currentCharacterSaveData.currentQuickSlotItemIndex = playerInventoryManager.currentQuickSlotItemIndex;
        currentCharacterSaveData.quickSlotItem01 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[0], this);
        currentCharacterSaveData.quickSlotItem02 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[1], this);
        currentCharacterSaveData.quickSlotItem03 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[2], this);
        currentCharacterSaveData.quickSlotItem04 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[3], this);
        currentCharacterSaveData.quickSlotItem05 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[4], this);
        currentCharacterSaveData.quickSlotItem06 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[5], this);
        currentCharacterSaveData.quickSlotItem07 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[6], this);
        currentCharacterSaveData.quickSlotItem08 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[7], this);
        currentCharacterSaveData.quickSlotItem09 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[8], this);
        currentCharacterSaveData.quickSlotItem10 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemInventory[9], this);

        // TODO: 需要保存当前的弹药数量 以及当前的弹药类型
        currentCharacterSaveData.currentMainProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.mainProjectile);

        currentCharacterSaveData.currentSecondaryProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.secondaryProjectile);

        // Spell
        if (playerInventoryManager.currentSpell != null)
            currentCharacterSaveData.currentSpell = playerInventoryManager.currentSpell.itemID;
        else
            currentCharacterSaveData.currentSpell = -1;

        currentCharacterSaveData.remainingHealthFlasks = playerNetworkManager.remainingHealthFlasks.Value;
        currentCharacterSaveData.remainingManaFlasks = playerNetworkManager.remainingManaFlasks.Value;

        // Inventory
        currentCharacterSaveData.weaponInInventory.Clear();
        currentCharacterSaveData.quickSlotItemInInventory.Clear();
        currentCharacterSaveData.rangedProjectileInInventory.Clear();
        currentCharacterSaveData.equipmentInInventory.Clear();
        for (int i = 0; i < playerInventoryManager.characterInventory.Count; i++)
        {
            Item item = playerInventoryManager.characterInventory[i];
            if (item is WeaponItem weaponItem)
            {
                currentCharacterSaveData.weaponInInventory.Add(WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(weaponItem));
            }
            else if (item is QuickSlotItem quickSlotItem)
            {
                currentCharacterSaveData.quickSlotItemInInventory.Add(WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(quickSlotItem, this));
            }
            else if (item is RangedProjectileItem rangedProjectileItem)
            {
                currentCharacterSaveData.rangedProjectileInInventory.Add(WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(rangedProjectileItem));
            }
            else if (item is EquipmentItem equipmentItem)
            {
                currentCharacterSaveData.equipmentInInventory.Add(equipmentItem.itemID);
            }
        }
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

        playerNetworkManager.level.Value = currentCharacterSaveData.level;
        playerNetworkManager.vigor.Value = currentCharacterSaveData.vigor;
        playerNetworkManager.endurance.Value = currentCharacterSaveData.endurance;
        playerNetworkManager.mind.Value = currentCharacterSaveData.mind;
        playerNetworkManager.strength.Value = currentCharacterSaveData.strength;
        playerNetworkManager.dexterity.Value = currentCharacterSaveData.dexterity;
        playerNetworkManager.intelligence.Value = currentCharacterSaveData.intelligence;
        playerNetworkManager.faith.Value = currentCharacterSaveData.faith;
        playerNetworkManager.arcane.Value = currentCharacterSaveData.arcane;

        playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnVitalityLevel(playerNetworkManager.vigor.Value);
        playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(playerNetworkManager.endurance.Value);
        playerNetworkManager.maxFocus.Value = playerStatsManager.CalculateFocusBasedOnMindLevel(playerNetworkManager.mind.Value);

        playerNetworkManager.currentHealth.Value = currentCharacterSaveData.currentHealth;
        playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;
        playerNetworkManager.currentFocusPoints.Value = Mathf.Clamp(currentCharacterSaveData.currentFocus, 0, playerNetworkManager.maxFocus.Value);

        var hud = PlayerUIManager.instance.playerUIHudManager;
        hud.SetMaxHealthValue(playerNetworkManager.maxHealth.Value);
        hud.SetMaxStaminaValue(playerNetworkManager.maxStamina.Value);
        hud.SetMaxFocusValue(playerNetworkManager.maxFocus.Value);
        hud.SetNewHealthValue(
            playerNetworkManager.currentHealth.Value,
            playerNetworkManager.currentHealth.Value);
        hud.SetNewStaminaValue(
            playerNetworkManager.currentStamina.Value,
            playerNetworkManager.currentStamina.Value);
        hud.SetNewFocusValue(
            playerNetworkManager.currentFocusPoints.Value,
            playerNetworkManager.currentFocusPoints.Value);

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
        playerInventoryManager.rightWeaponIndex = currentCharacterSaveData.currentRightWeaponIndex;
        playerInventoryManager.weaponsInRightHand[0] = currentCharacterSaveData.rightWeapon01.GetWeapon();
        playerInventoryManager.weaponsInRightHand[1] = currentCharacterSaveData.rightWeapon02.GetWeapon();
        playerInventoryManager.weaponsInRightHand[2] = currentCharacterSaveData.rightWeapon03.GetWeapon();

        // Weapons Left Hand
        playerInventoryManager.leftWeaponIndex = currentCharacterSaveData.currentLeftWeaponIndex;
        playerInventoryManager.weaponsInLeftHand[0] = currentCharacterSaveData.leftWeapon01.GetWeapon();
        playerInventoryManager.weaponsInLeftHand[1] = currentCharacterSaveData.leftWeapon02.GetWeapon();
        playerInventoryManager.weaponsInLeftHand[2] = currentCharacterSaveData.leftWeapon03.GetWeapon();

        playerInventoryManager.currentRightHandWeapon = playerInventoryManager.weaponsInRightHand[playerInventoryManager.rightWeaponIndex];
        playerInventoryManager.currentLeftHandWeapon = playerInventoryManager.weaponsInLeftHand[playerInventoryManager.leftWeaponIndex];

        playerEquipmentManager.RefreshRightWeapon();
        playerEquipmentManager.RefreshLeftWeapon();

        // Spell
        if (WorldItemDatabase.instance.GetSpellByID(currentCharacterSaveData.currentSpell) != null)
            playerNetworkManager.currentSpellID.Value = Instantiate(WorldItemDatabase.instance.GetSpellByID(currentCharacterSaveData.currentSpell)).itemID;
        else
            playerNetworkManager.currentSpellID.Value = -1;

        // Quick Slot Items

        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem01, 0);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem02, 1);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem03, 2);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem04, 3);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem05, 4);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem06, 5);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem07, 6);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem08, 7);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem09, 8);
        playerInventoryManager.RestoreQuickSlot(currentCharacterSaveData.quickSlotItem10, 9);


        RangedProjectileItem mainProjectile = currentCharacterSaveData.currentMainProjectile.GetProjectile();
        playerEquipmentManager.LoadMainProjectileEquipment(mainProjectile);

        RangedProjectileItem secondaryProjectile = currentCharacterSaveData.currentSecondaryProjectile.GetProjectile();
        playerEquipmentManager.LoadSecondaryProjectileEquipment(secondaryProjectile);

        playerEquipmentManager.EquipArmor();

        if (currentCharacterSaveData.currentQuickSlotItemIndex == -1)
            playerInventoryManager.currentQuickSlotItemIndex = 0;
        else
        {
            playerInventoryManager.currentQuickSlotItemIndex = currentCharacterSaveData.currentQuickSlotItemIndex;
            playerInventoryManager.currentQuickSlotItem = playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex];

            if (playerInventoryManager.currentQuickSlotItem != null)
                PlayerUIManager.instance.playerUIHudManager.SetQuickSlotCountText(playerInventoryManager.currentQuickSlotItem.GetAmountOfItem(this), playerInventoryManager.currentQuickSlotItem.isConsumable);
        }

        playerNetworkManager.currentSpellID.Value = currentCharacterSaveData.currentSpell;
        playerNetworkManager.currentQuickSlotItemID.Value = playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex] != null ? playerInventoryManager.quickSlotItemInventory[playerInventoryManager.currentQuickSlotItemIndex].itemID : -1;
        playerNetworkManager.remainingHealthFlasks.Value = currentCharacterSaveData.remainingHealthFlasks;
        playerNetworkManager.remainingManaFlasks.Value = currentCharacterSaveData.remainingManaFlasks;

        // Inventory
        playerInventoryManager.characterInventory.Clear();
        for (int i = 0; i < currentCharacterSaveData.weaponInInventory.Count; i++)
        {
            playerInventoryManager.characterInventory.Add(currentCharacterSaveData.weaponInInventory[i].GetWeapon());
        }
        for (int i = 0; i < currentCharacterSaveData.quickSlotItemInInventory.Count; i++)
        {
            playerInventoryManager.characterInventory.Add(currentCharacterSaveData.quickSlotItemInInventory[i].GetQuickSlotItem());
        }
        for (int i = 0; i < currentCharacterSaveData.rangedProjectileInInventory.Count; i++)
        {
            playerInventoryManager.characterInventory.Add(currentCharacterSaveData.rangedProjectileInInventory[i].GetProjectile());
        }
        for (int i = 0; i < currentCharacterSaveData.equipmentInInventory.Count; i++)
        {
            int id = currentCharacterSaveData.equipmentInInventory[i];
            Item definition = WorldItemDatabase.instance.GetItemByID(id);

            if (definition is EquipmentItem)
            {
                playerInventoryManager.characterInventory.Add(Instantiate(definition));
            }
        }

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
