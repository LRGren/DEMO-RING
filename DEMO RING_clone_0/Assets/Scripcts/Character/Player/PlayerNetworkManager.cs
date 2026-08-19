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

    [Header("Equipment")]
    public NetworkVariable<int> currentWeaponBeingUsed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentRightHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentLeftHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isUsingRightHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isUsingLeftHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Two Hand Weapon")]
    public NetworkVariable<bool> isTwoHandingWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isTwoHandingRightWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isTwoHandingLeftWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentWeaponBeingTwoHanded = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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


    // Weapon
    public void OnCurrentRightHandWeaponIDChanged(int oldWeaponID, int newWeaponID)
    {
        WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newWeaponID));
        player.playerInventoryManager.currentRightHandWeapon = newWeapon;
        player.playerEquipmentManager.LoadRightWeapon();

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetRightWeaponQuickSlot(newWeaponID);
        }
    }

    public void OnCurrentLeftHandWeaponIDChanged(int oldWeaponID, int newWeaponID)
    {
        WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newWeaponID));
        player.playerInventoryManager.currentLeftHandWeapon = newWeapon;
        player.playerEquipmentManager.LoadLeftWeapon();

        if (player.IsOwner)
        {
            PlayerUIManager.instance.playerUIHudManager.SetLeftWeaponQuickSlot(newWeaponID);
        }
    }

    public void OnCurrentWeaponBedingUsedIDChanged(int oldWeaponID, int newWeaponID)
    {
        WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newWeaponID));
        player.playerCombatManager.currentWeaponBedingUsed = newWeapon;

        if (!IsOwner)
            return;

        if (player.playerCombatManager.currentWeaponBedingUsed != null)
            player.playerAnimatorManager.UpdateAnimatorController(player.playerCombatManager.currentWeaponBedingUsed.weaponAnimator);
    }


    // Blocking
    public override void OnIsBlockingChanged(bool old, bool isLockedOn)
    {
        base.OnIsBlockingChanged(old, isLockedOn);

        if (IsOwner)
        {
            player.playerStatsManager.blockingPhysicalAbsorption = player.playerInventoryManager.currentLeftHandWeapon.physicalDamageAbsorption;
            player.playerStatsManager.blockingMagicalAbsorption = player.playerInventoryManager.currentLeftHandWeapon.magicalDamageAbsorption;
            player.playerStatsManager.blockingFireAbsorption = player.playerInventoryManager.currentLeftHandWeapon.fireDamageAbsorption;
            player.playerStatsManager.blockingHolyAbsorption = player.playerInventoryManager.currentLeftHandWeapon.holyDamageAbsorption;
            player.playerStatsManager.blockingLightningAbsorption = player.playerInventoryManager.currentLeftHandWeapon.lightningDamageAbsorption;

            player.playerStatsManager.blockingStaminaCost = player.playerInventoryManager.currentLeftHandWeapon.staminaCostToBlock;
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
            player.playerCombatManager.PerformWeaponBasedAction(weaponAction, WorldItemDatabase.Instance.GetWeaponByID(weaponID));
        }
        else
        {
            Debug.LogError("CANNOT PLAY THE ACTION. No weapon action found for action ID: " + actionID);
        }
    }


}
