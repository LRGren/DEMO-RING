using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerNetworkManager : CharacterNetworkManager
{
    private PlayerManager player;

    public NetworkVariable<FixedString64Bytes> characterName = new NetworkVariable<FixedString64Bytes>("sereinjians", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Flasks")]
    public NetworkVariable<int> remainingHealthFlasks = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> remainingManaFlasks = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isChugging = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Equipment")]
    public NetworkVariable<int> currentWeaponBeingUsed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentRightHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentLeftHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentSpellID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentQuickSlotItemID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public NetworkVariable<bool> isUsingRightHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isUsingLeftHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Spells")]
    public NetworkVariable<bool> isChargingRightSpell = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isChargingLeftSpell = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    [Header("Two Hand Weapon")]
    public NetworkVariable<bool> isTwoHandingWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isTwoHandingRightWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isTwoHandingLeftWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentWeaponBeingTwoHanded = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Armor")]
    public NetworkVariable<bool> isMale = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> headEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> bodyEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> handEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> legEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Projectiles")]
    public NetworkVariable<int> mainProjectileID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> secondaryProjectileID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> hasArrowNotched = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isHoldingArrow = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isAiming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
    }

    public void SetCharacterActionHand(bool rightHandedAction)
    {
        if (rightHandedAction)
        {
            isUsingLeftHand.Value = false;
            isUsingRightHand.Value = true;
        }
        else
        {
            isUsingLeftHand.Value = true;
            isUsingRightHand.Value = false;
        }
    }

    public void SetNewMaxHealthValue(int oldVitality, int newVitality)
    {
        maxHealth.Value = player.playerStatsManager.CalculateHealthBasedOnVitalityLevel(newVitality);
        PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(maxHealth.Value);
        currentHealth.Value = maxHealth.Value;

    }

    public void SetNewMaxStaminaValue(int oldEndurance, int newEndurance)
    {
        maxStamina.Value = player.playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(newEndurance);
        PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(maxStamina.Value);
        currentStamina.Value = maxStamina.Value;
    }

    public void SetNewMaxFocusValue(int oldMind, int newMind)
    {
        maxFocus.Value = player.playerStatsManager.CalculateFocusBasedOnMindLevel(newMind);
        PlayerUIManager.instance.playerUIHudManager.SetMaxFocusValue(maxFocus.Value);
        currentFocusPoints.Value = maxFocus.Value;
    }


    // Aiming
    public void OnIsAimingChanged(bool old, bool isAiming)
    {
        if (!isAiming)
        {
            PlayerCamera.instance.cameraObject.transform.localEulerAngles = Vector3.zero;

            PlayerCamera.instance.cameraObject.fieldOfView = 60f;
            PlayerCamera.instance.cameraObject.nearClipPlane = .3f;

            PlayerCamera.instance.cameraPivotTransform.localPosition = new Vector3(0, PlayerCamera.instance.aimingFollowCameraYPositionOffset, 0);

            PlayerUIManager.instance.playerUIHudManager.crosshair.SetActive(false);
        }
        else
        {
            PlayerCamera.instance.StopCameraHeightCoroutine();

            PlayerCamera.instance.gameObject.transform.eulerAngles = Vector3.zero;
            PlayerCamera.instance.cameraPivotTransform.localEulerAngles = Vector3.zero;

            PlayerCamera.instance.cameraObject.fieldOfView = 40f;
            PlayerCamera.instance.cameraObject.nearClipPlane = 1.3f;

            PlayerCamera.instance.cameraPivotTransform.transform.localPosition = Vector3.zero;

            PlayerUIManager.instance.playerUIHudManager.crosshair.SetActive(true);
        }
    }

    // Weapon
    public void OnCurrentRightHandWeaponIDChanged(int oldWeaponID, int newWeaponID)
    {
        WeaponItem newWeapon = Instantiate(WorldItemDatabase.instance.GetWeaponByID(newWeaponID));
        player.playerInventoryManager.currentRightHandWeapon = newWeapon;
        player.playerEquipmentManager.LoadRightWeapon();

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetRightWeaponQuickSlotIcon(newWeaponID);

            PlayerUIManager.instance.playerUIHudManager.ToggleProjectileSlotsGameobject(newWeapon.weaponClass == WeaponClass.Bow);
        }
    }

    public void OnCurrentLeftHandWeaponIDChanged(int oldWeaponID, int newWeaponID)
    {
        WeaponItem newWeapon = Instantiate(WorldItemDatabase.instance.GetWeaponByID(newWeaponID));
        player.playerInventoryManager.currentLeftHandWeapon = newWeapon;
        player.playerEquipmentManager.LoadLeftWeapon();

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetLeftWeaponQuickSlotIcon(newWeaponID);
        }
    }

    public void OnCurrentWeaponBedingUsedIDChanged(int oldWeaponID, int newWeaponID)
    {
        WeaponItem newWeapon = Instantiate(WorldItemDatabase.instance.GetWeaponByID(newWeaponID));
        player.playerCombatManager.currentWeaponBedingUsed = newWeapon;

        if (!IsOwner)
            return;

        if (player.playerCombatManager.currentWeaponBedingUsed != null)
            player.playerAnimatorManager.UpdateAnimatorController(player.playerCombatManager.currentWeaponBedingUsed.weaponAnimator);
    }


    // Spell
    public void OnCurrentSpellIDChanged(int oldSpellID, int newSpellID)
    {
        if (newSpellID == -1)
        {
            player.playerInventoryManager.currentSpell = null;
            PlayerUIManager.instance.playerUIHudManager.SetSpellItemQuickSlotIcon(-1);
            return;
        }

        SpellItem newSpell = Instantiate(WorldItemDatabase.instance.GetSpellByID(newSpellID));

        if (newSpell == null)
            return;

        player.playerInventoryManager.currentSpell = newSpell;

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetSpellItemQuickSlotIcon(newSpellID);
        }
    }

    public void OnIsChargingRightSpellChanged(bool old, bool isCharging)
    {
        player.animator.SetBool("isChargingRightSpell", isChargingRightSpell.Value);
    }

    public void OnIsChargingLeftSpellChanged(bool old, bool isCharging)
    {
        player.animator.SetBool("isChargingLeftSpell", isChargingLeftSpell.Value);
    }

    // Quick Slot Item
    public void OnCurrentQuickSlotItemIDChanged(int oldQuickSlotItemID, int newQuickSlotItemID)
    {
        if (newQuickSlotItemID == -1)
        {
            player.playerInventoryManager.currentQuickSlotItem = null;
            PlayerUIManager.instance.playerUIHudManager.SetQuickSlotItemQuickSlotIcon(-1);
            return;
        }

        QuickSlotItem newQuickSlotItem = Instantiate(WorldItemDatabase.instance.GetQuickSlotItemByID(newQuickSlotItemID));

        if (newQuickSlotItem == null)
            return;

        player.playerInventoryManager.currentQuickSlotItem = newQuickSlotItem;

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetQuickSlotItemQuickSlotIcon(newQuickSlotItemID);
        }
    }
    public void OnIsChuggingChanged(bool old, bool isChugging)
    {
        player.animator.SetBool("isChuggingFlask", isChugging);
    }

    // Projectile
    public void OnMainProjectileIDChanged(int oldProjectileID, int newProjectileID)
    {
        if (newProjectileID == -1)
        {
            player.playerInventoryManager.mainProjectile = null;
            PlayerUIManager.instance.playerUIHudManager.SetMainProjectileItemQuickSlotIcon(null);
            return;
        }

        RangedProjectileItem newProjectile = Instantiate(WorldItemDatabase.instance.GetProjectileByID(newProjectileID));

        if (newProjectile == null)
            return;

        player.playerInventoryManager.mainProjectile = newProjectile;

        if (player.IsOwner)
        {
            // Debug.Log("SSS");
            PlayerUIManager.instance.playerUIHudManager.SetMainProjectileItemQuickSlotIcon(newProjectile);
        }
    }

    public void OnSecondaryProjectileIDChanged(int oldProjectileID, int newProjectileID)
    {
        if (newProjectileID == -1)
        {
            player.playerInventoryManager.secondaryProjectile = null;
            PlayerUIManager.instance.playerUIHudManager.SetSecondaryProjectileItemQuickSlotIcon(null);
            return;
        }

        RangedProjectileItem newProjectile = Instantiate(WorldItemDatabase.instance.GetProjectileByID(newProjectileID));

        if (newProjectile == null)
            return;

        player.playerInventoryManager.secondaryProjectile = newProjectile;

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetSecondaryProjectileItemQuickSlotIcon(newProjectile);
        }
    }

    public void OnIsHoldingArrowChanged(bool old, bool isHoldingArrow)
    {
        player.animator.SetBool("isHoldingArrow", isHoldingArrow);
    }

    // Blocking
    public override void OnIsBlockingChanged(bool old, bool isLockedOn)
    {
        base.OnIsBlockingChanged(old, isLockedOn);

        if (IsOwner)
        {
            if (player.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                player.playerStatsManager.blockingPhysicalAbsorption = player.playerInventoryManager.currentTwoHandedWeapon.physicalDamageAbsorption;
                player.playerStatsManager.blockingMagicalAbsorption = player.playerInventoryManager.currentTwoHandedWeapon.magicalDamageAbsorption;
                player.playerStatsManager.blockingFireAbsorption = player.playerInventoryManager.currentTwoHandedWeapon.fireDamageAbsorption;
                player.playerStatsManager.blockingHolyAbsorption = player.playerInventoryManager.currentTwoHandedWeapon.holyDamageAbsorption;
                player.playerStatsManager.blockingLightningAbsorption = player.playerInventoryManager.currentTwoHandedWeapon.lightningDamageAbsorption;

                player.playerStatsManager.blockingStaminaAbsorption = player.playerInventoryManager.currentTwoHandedWeapon.staminaAbsorption;
            }
            else
            {
                player.playerStatsManager.blockingPhysicalAbsorption = player.playerInventoryManager.currentLeftHandWeapon.physicalDamageAbsorption;
                player.playerStatsManager.blockingMagicalAbsorption = player.playerInventoryManager.currentLeftHandWeapon.magicalDamageAbsorption;
                player.playerStatsManager.blockingFireAbsorption = player.playerInventoryManager.currentLeftHandWeapon.fireDamageAbsorption;
                player.playerStatsManager.blockingHolyAbsorption = player.playerInventoryManager.currentLeftHandWeapon.holyDamageAbsorption;
                player.playerStatsManager.blockingLightningAbsorption = player.playerInventoryManager.currentLeftHandWeapon.lightningDamageAbsorption;

                player.playerStatsManager.blockingStaminaAbsorption = player.playerInventoryManager.currentLeftHandWeapon.staminaAbsorption;
            }
        }
    }

    // Two Hand Weapon
    public void OnIsTwoHandingWeaponChanged(bool old, bool isTwoHanding)
    {
        if (!isTwoHandingWeapon.Value)
        {
            if (IsOwner)
            {
                isTwoHandingRightWeapon.Value = false;
                isTwoHandingLeftWeapon.Value = false;
            }

            player.playerEquipmentManager.UnTwoHandWeapon();
            player.playerEffectsManager.RemoveStaticEffect(WorldCharacterEffectsManager.instance.twoHandingEffect.staticEffectID);
        }
        else
        {
            player.playerEffectsManager.AddStaticEffect(WorldCharacterEffectsManager.instance.twoHandingEffect);
        }

        player.animator.SetBool("isTwoHandWeapon", isTwoHandingWeapon.Value);
    }
    public void OnIsTwoHandingRightWeaponChanged(bool old, bool isTwoHanding)
    {
        if (!isTwoHandingRightWeapon.Value)
            return;

        if (IsOwner)
        {
            currentWeaponBeingTwoHanded.Value = currentRightHandWeaponID.Value;
            isTwoHandingWeapon.Value = true;
        }

        player.playerInventoryManager.currentTwoHandedWeapon = player.playerInventoryManager.currentRightHandWeapon;
        player.playerEquipmentManager.TwoHandRightWeapon();
    }
    public void OnIsTwoHandingLeftWeaponChanged(bool old, bool isTwoHanding)
    {
        if (!isTwoHandingLeftWeapon.Value)
            return;

        if (IsOwner)
        {
            currentWeaponBeingTwoHanded.Value = currentLeftHandWeaponID.Value;
            isTwoHandingWeapon.Value = true;
        }

        player.playerInventoryManager.currentTwoHandedWeapon = player.playerInventoryManager.currentLeftHandWeapon;
        player.playerEquipmentManager.TwoHandLeftWeapon();
    }

    // Armor
    public void OnHeadEquipmentIDChanged(int oldID, int newID)
    {
        if (IsOwner)
            return;

        HeadEquipmentItem equipment = WorldItemDatabase.instance.GetHeadEquipmentByID(headEquipmentID.Value);
        if (equipment != null)
        {
            player.playerEquipmentManager.LoadHeadEquipment(Instantiate(equipment));
        }
        else
        {
            player.playerEquipmentManager.LoadHeadEquipment(null);
        }

    }

    public void OnBodyEquipmentIDChanged(int oldID, int newID)
    {
        if (IsOwner)
            return;

        BodyEquipmentItem equipment = WorldItemDatabase.instance.GetBodyEquipmentByID(bodyEquipmentID.Value);
        if (equipment != null)
        {
            player.playerEquipmentManager.LoadBodyEquipment(Instantiate(equipment));
        }
        else
        {
            player.playerEquipmentManager.LoadBodyEquipment(null);
        }
    }

    public void OnHandEquipmentIDChanged(int oldID, int newID)
    {
        if (IsOwner)
            return;

        HandEquipmentItem equipment = WorldItemDatabase.instance.GetHandEquipmentByID(handEquipmentID.Value);
        if (equipment != null)
        {
            player.playerEquipmentManager.LoadHandEquipment(Instantiate(equipment));
        }
        else
        {
            player.playerEquipmentManager.LoadHandEquipment(null);
        }
    }

    public void OnLegEquipmentIDChanged(int oldID, int newID)
    {
        if (IsOwner)
            return;

        LegEquipmentItem equipment = WorldItemDatabase.instance.GetLegEquipmentByID(legEquipmentID.Value);
        if (equipment != null)
        {
            player.playerEquipmentManager.LoadLegEquipment(Instantiate(equipment));
        }
        else
        {
            player.playerEquipmentManager.LoadLegEquipment(null);
        }
    }

    // Body Type
    public void OnIsMaleChanged(bool old, bool isMale)
    {
        player.playerBodyManager.ToggleBodyType(isMale);
        player.playerEquipmentManager.EquipArmor();
    }

    // Rpc
    [ServerRpc]
    public void NotifyTheServerOfWeaponActionServerRpc(ulong clientID, int actionID, int weaponID)
    {
        if (IsServer)
        {
            NotifyTheClientsOfWeaponActionClientRpc(clientID, actionID, weaponID);
        }
    }

    [ClientRpc]
    private void NotifyTheClientsOfWeaponActionClientRpc(ulong clientID, int actionID, int weaponID)
    {
        //如果不是本地玩家执行的动作，那么其他玩家需要在客户端执行对应的动作
        if (clientID != NetworkManager.Singleton.LocalClientId)
        {
            PerformWeaponBasedAction(actionID, weaponID);
        }
    }

    private void PerformWeaponBasedAction(int actionID, int weaponID)
    {
        WeaponItemAction weaponAction = WorldActionManager.Instance.GetWeaponActionByID(actionID);

        if (weaponAction != null)
        {
            player.playerCombatManager.PerformWeaponBasedAction(weaponAction, WorldItemDatabase.instance.GetWeaponByID(weaponID));
        }
        else
        {
            Debug.LogError("CANNOT PLAY THE ACTION. No weapon action found for action ID: " + actionID);
        }
    }

    [ServerRpc]
    public void NotifyTheServerOfDrawnProjectileServerRpc(int projectileID)
    {
        if (IsServer)
        {
            NotifyTheClientsOfDrawnProjectileClientRpc(projectileID);
        }
    }

    [ClientRpc]
    private void NotifyTheClientsOfDrawnProjectileClientRpc(int projectileID)
    {
        // 如果弓有动画的话，在这里通过WeaponManager得到弓的动画并播放

        // 生成投掷物
        GameObject arrow = Instantiate(WorldItemDatabase.instance.GetProjectileByID(projectileID).drawProjectileModel, player.playerEquipmentManager.leftHandWeaponManager.transform);
        player.playerEffectsManager.activeProjectileFX = arrow;

        player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.notchArrowSFX));
    }

    [ServerRpc]
    public void NotifyTheServerOfReleasedProjectileServerRpc(ulong clientID, int projectileID, float xPosition, float yPosition, float zPosition, float yRotation)
    {
        if (IsServer)
        {
            NotifyTheClientsOfReleasedProjectileClientRpc(clientID, projectileID, xPosition, yPosition, zPosition, yRotation);
        }
    }

    [ClientRpc]
    private void NotifyTheClientsOfReleasedProjectileClientRpc(ulong clientID, int projectileID, float xPosition, float yPosition, float zPosition, float yRotation)
    {
        if (clientID != NetworkManager.Singleton.LocalClientId)
        {
            PerformReleasedProjectileRpc(projectileID, xPosition, yPosition, zPosition, yRotation);
        }
    }

    private void PerformReleasedProjectileRpc(int projectileID, float xPosition, float yPosition, float zPosition, float yRotation)
    {
        RangedProjectileItem projectileToFire = null;
        if (WorldItemDatabase.instance.GetProjectileByID(projectileID) != null)
        {
            projectileToFire = Instantiate(WorldItemDatabase.instance.GetProjectileByID(projectileID));
        }

        if (projectileToFire == null)
            return;

        Transform projectileInstantiatePoint = player.playerCombatManager.lockOnTransform;
        GameObject liveProjectileGameObject = Instantiate(projectileToFire.releaseProjectileModel, projectileInstantiatePoint);
        RangedProjectileDamageCollider liveProjectileDamageCollider = liveProjectileGameObject.GetComponent<RangedProjectileDamageCollider>();
        Rigidbody liveProjectileRigidbody = liveProjectileDamageCollider.projectileRigidbody;

        // (TODO:) 伤害计算，距离衰减
        liveProjectileDamageCollider.physicalDamage = projectileToFire.physicalDamage;
        liveProjectileDamageCollider.characterShootingProjectile = player;

        // 三种发射方式
        if (player.playerNetworkManager.isAiming.Value)
        {
            // 瞄准
            // 3. 不锁定 瞄准
            liveProjectileGameObject.transform.LookAt(new Vector3(xPosition, yPosition, zPosition));
        }
        else
        {
            // 不瞄准
            // // 1. 锁定
            if (player.playerCombatManager.currentTarget != null)
            {
                liveProjectileGameObject.transform.rotation = Quaternion.LookRotation(player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - liveProjectileGameObject.transform.position);
            }
            // 2. 不锁定 但是 不瞄准
            else
            {
                player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, yRotation, player.transform.rotation.eulerAngles.z);
                liveProjectileGameObject.transform.rotation = Quaternion.LookRotation(player.transform.forward);
            }
        }

        // 无视碰撞
        Collider[] collisions = player.GetComponentsInChildren<Collider>();
        List<Collider> collisionList = new List<Collider>(collisions);
        foreach (var col in collisions)
            collisionList.Add(col);

        foreach (var col in collisionList)
            Physics.IgnoreCollision(liveProjectileDamageCollider.damageCollider, col, true);

        // 减少箭矢数量
        //projectileToFire.currentAmmoAmount--;

        // 动量
        //liveProjectileRigidbody.mass = projectileToFire.ammoMass;

        liveProjectileRigidbody.AddForce(liveProjectileGameObject.transform.forward * projectileToFire.forwardVelocity);
        liveProjectileRigidbody.AddForce(liveProjectileGameObject.transform.up * projectileToFire.upwardVelocity);

        liveProjectileGameObject.transform.parent = null;
    }


    // Cancel All Attempted Actions (FX, Animations, etc.)
    [ClientRpc]
    public override void DestoryAllAttemptedActionsClientRpc()
    {
        // 不能调 base.DestoryAllAttemptedActionsClientRpc()，那是 [ClientRpc]，会再次发送 RPC 造成无限递归
        ClearAllAttemptedActionsFX();

        if (player.playerNetworkManager.hasArrowNotched.Value)
        {
            // 播放弓的动画

            // 播放射箭音效
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.releaseArrowSFX));
        }

        if (player.IsOwner)
        {
            player.playerNetworkManager.hasArrowNotched.Value = false;
        }
    }

    // Hide Weapons
    [ServerRpc]
    public void HideWeaponsServerRpc()
    {
        if (IsServer)
        {
            HideWeaponsClientRpc();
        }
    }

    [ClientRpc]
    private void HideWeaponsClientRpc()
    {
        if (player.playerEquipmentManager.rightWeaponModel != null)
            player.playerEquipmentManager.rightWeaponModel.SetActive(false);

        if (player.playerEquipmentManager.leftWeaponModel != null)
            player.playerEquipmentManager.leftWeaponModel.SetActive(false);
    }

    [ServerRpc]
    public void NotifyTheServerOfQuickSlotItemActionServerRpc(ulong clientID, int quickSlotItemID)
    {
        if (IsServer)
        {
            NotifyTheClientsOfQuickSlotItemActionClientRpc(clientID, quickSlotItemID);
        }
    }

    [ClientRpc]
    private void NotifyTheClientsOfQuickSlotItemActionClientRpc(ulong clientID, int quickSlotItemID)
    {
        //如果不是本地玩家执行的动作，那么其他玩家需要在客户端执行对应的动作
        if (clientID != NetworkManager.Singleton.LocalClientId)
        {
            QuickSlotItem quickSlotItem = WorldItemDatabase.instance.GetQuickSlotItemByID(quickSlotItemID);

            if (quickSlotItem != null)
                quickSlotItem.AttemptToUseItem(player);
        }
    }

}
