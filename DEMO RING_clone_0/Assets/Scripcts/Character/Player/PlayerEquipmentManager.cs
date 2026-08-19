using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerEquipmentManager : CharacterEquipmentManager
{
    PlayerManager player;

    [Header("Weapon Slots")]
    public WeaponModelInstantiationSlot rightHandWeaponSlot;
    public WeaponModelInstantiationSlot leftHandWeaponSlot;
    public WeaponModelInstantiationSlot leftHandShieldSlot;
    public WeaponModelInstantiationSlot backSlot;

    [Header("Weapon Managers")]
    [SerializeField] WeaponManager rightHandWeaponManager;
    [SerializeField] WeaponManager leftHandWeaponManager;

    [Header("Weapon Models")]
    public GameObject rightWeaponModel;
    public GameObject leftWeaponModel;

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();

        InitialiseWeaponSlots();
    }

    private void InitialiseWeaponSlots()
    {
        WeaponModelInstantiationSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>();

        foreach (var weapon in weaponSlots)
        {
            if (weapon.weaponSlot == WeaponModelSlot.RightHandSlot)
            {
                rightHandWeaponSlot = weapon;
            }
            else if (weapon.weaponSlot == WeaponModelSlot.LeftHandWeaponSlot)
            {
                leftHandWeaponSlot = weapon;
            }
            else if (weapon.weaponSlot == WeaponModelSlot.LeftHandShieldSlot)
            {
                leftHandShieldSlot = weapon;
            }
            else if (weapon.weaponSlot == WeaponModelSlot.BackSlot)
            {
                backSlot = weapon;
            }
        }
    }

    public void LoadWeaponsOnBothHands()
    {
        LoadRightWeapon();
        LoadLeftWeapon();
    }

    // Right Weapon
    public void LoadRightWeapon()
    {
        if (player.playerInventoryManager.currentRightHandWeapon != null)
        {
            rightHandWeaponSlot.UnloadWeapon();

            rightWeaponModel = Instantiate(player.playerInventoryManager.currentRightHandWeapon.weaponModel);
            rightHandWeaponSlot.PlaceWeaponIntoSlot(rightWeaponModel);

            rightHandWeaponManager = rightWeaponModel.GetComponentInChildren<WeaponManager>();
            rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);

            player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);
        }
    }

    public void SwitchRightWeapon()
    {
        if (!player.IsOwner)
            return;

        player.playerAnimatorManager.PlayerTargetActionAnimation("Swap_Right_Weapon_01", false, false, true, true);

        //确认是否有其他武器，如果有，切换武器
        //如果没有，切换到空手
        WeaponItem selectedWeapon = null;

        player.playerInventoryManager.rightWeaponIndex++;

        if (player.playerInventoryManager.rightWeaponIndex < 0 || player.playerInventoryManager.rightWeaponIndex > 2)
        {
            player.playerInventoryManager.rightWeaponIndex = 0;
            int weaponCount = 0;
            WeaponItem firstWeapon = null;
            int firstWeaponPosition = 0;

            for (int i = 0; i < player.playerInventoryManager.weaponsInRightHand.Length; i++)
            {
                if (player.playerInventoryManager.weaponsInRightHand[i].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    weaponCount++;
                    if (firstWeapon == null)
                    {
                        firstWeapon = player.playerInventoryManager.weaponsInRightHand[i];
                        firstWeaponPosition = i;
                    }
                }
            }

            if (weaponCount <= 1)
            {
                player.playerInventoryManager.rightWeaponIndex = -1;
                selectedWeapon = WorldItemDatabase.Instance.unarmedWeapon;
                player.playerNetworkManager.currentRightHandWeaponID.Value = selectedWeapon.itemID;
            }
            else
            {
                player.playerInventoryManager.rightWeaponIndex = firstWeaponPosition;
                player.playerNetworkManager.currentRightHandWeaponID.Value = firstWeapon.itemID;
            }


            return;
        }

        foreach (WeaponItem weaponItem in player.playerInventoryManager.weaponsInRightHand)
        {
            //Debug.Log(player.playerInventoryManager.rightWeaponIndex + " and " + player.playerInventoryManager.weaponsInRightHand.Length);
            if (player.playerInventoryManager.weaponsInRightHand[player.playerInventoryManager.rightWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
            {
                selectedWeapon = player.playerInventoryManager.weaponsInRightHand[player.playerInventoryManager.rightWeaponIndex];

                //需要分配武器ID到网络上使得客户端能够正确加载武器模型
                player.playerNetworkManager.currentRightHandWeaponID.Value = player.playerInventoryManager.weaponsInRightHand[player.playerInventoryManager.rightWeaponIndex].itemID;

                return;
            }
        }

        if (selectedWeapon == null && player.playerInventoryManager.rightWeaponIndex <= 2)
        {
            SwitchRightWeapon();
        }
    }

    // Left Weapon 
    public void LoadLeftWeapon()
    {
        if (player.playerInventoryManager.currentLeftHandWeapon != null)
        {
            if (leftHandWeaponSlot.currentWeapon != null)
                leftHandWeaponSlot.UnloadWeapon();

            if (leftHandShieldSlot.currentWeapon != null)
                leftHandShieldSlot.UnloadWeapon();

            leftWeaponModel = Instantiate(player.playerInventoryManager.currentLeftHandWeapon.weaponModel);

            switch (player.playerInventoryManager.currentLeftHandWeapon.weaponType)
            {
                case WeaponType.Weapon:
                    leftHandWeaponSlot.PlaceWeaponIntoSlot(leftWeaponModel);
                    break;
                case WeaponType.Shield:
                    leftHandShieldSlot.PlaceWeaponIntoSlot(leftWeaponModel);
                    break;
                default:
                    Debug.LogError("Unknown weapon type for left hand weapon.");
                    break;
            }

            leftHandWeaponManager = leftWeaponModel.GetComponentInChildren<WeaponManager>();
            leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
        }
    }

    public void SwitchLeftWeapon()
    {
        if (!player.IsOwner)
            return;

        player.playerAnimatorManager.PlayerTargetActionAnimation("Swap_Left_Weapon_01", false, false, true, true);

        //确认是否有其他武器，如果有，切换武器
        //如果没有，切换到空手
        WeaponItem selectedWeapon = null;

        player.playerInventoryManager.leftWeaponIndex++;

        if (player.playerInventoryManager.leftWeaponIndex < 0 || player.playerInventoryManager.leftWeaponIndex > 2)
        {
            player.playerInventoryManager.leftWeaponIndex = 0;
            int weaponCount = 0;
            WeaponItem firstWeapon = null;
            int firstWeaponPosition = 0;

            for (int i = 0; i < player.playerInventoryManager.weaponsInLeftHand.Length; i++)
            {
                if (player.playerInventoryManager.weaponsInLeftHand[i].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    weaponCount++;
                    if (firstWeapon == null)
                    {
                        firstWeapon = player.playerInventoryManager.weaponsInLeftHand[i];
                        firstWeaponPosition = i;
                    }
                }
            }

            if (weaponCount <= 1)
            {
                player.playerInventoryManager.leftWeaponIndex = -1;
                selectedWeapon = WorldItemDatabase.Instance.unarmedWeapon;
                player.playerNetworkManager.currentLeftHandWeaponID.Value = selectedWeapon.itemID;
            }
            else
            {
                player.playerInventoryManager.leftWeaponIndex = firstWeaponPosition;
                player.playerNetworkManager.currentLeftHandWeaponID.Value = firstWeapon.itemID;
            }


            return;
        }

        foreach (WeaponItem weaponItem in player.playerInventoryManager.weaponsInLeftHand)
        {
            //Debug.Log(player.playerInventoryManager.leftWeaponIndex + " and " + player.playerInventoryManager.weaponsInLeftHand.Length);
            if (player.playerInventoryManager.weaponsInLeftHand[player.playerInventoryManager.leftWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
            {
                selectedWeapon = player.playerInventoryManager.weaponsInLeftHand[player.playerInventoryManager.leftWeaponIndex];

                //需要分配武器ID到网络上使得客户端能够正确加载武器模型
                player.playerNetworkManager.currentLeftHandWeaponID.Value = player.playerInventoryManager.weaponsInLeftHand[player.playerInventoryManager.leftWeaponIndex].itemID;

                return;
            }
        }

        if (selectedWeapon == null && player.playerInventoryManager.leftWeaponIndex <= 2)
        {
            SwitchLeftWeapon();
        }
    }

    // Two Hand Weapon
    public void UnTwoHandWeapon()
    {
        //更新动画
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);

        //去除力量加成

        //恢复非双持武器
        if (player.playerInventoryManager.currentLeftHandWeapon.weaponType == WeaponType.Weapon)
        {
            leftHandWeaponSlot.PlaceWeaponIntoSlot(leftWeaponModel);
        }
        else if (player.playerInventoryManager.currentLeftHandWeapon.weaponType == WeaponType.Shield)
        {
            leftHandShieldSlot.PlaceWeaponIntoSlot(leftWeaponModel);
        }

        rightHandWeaponSlot.PlaceWeaponIntoSlot(rightWeaponModel);

        //damage collider更新
        rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
    }

    public void TwoHandRightWeapon()
    {
        //检查是否可以双持
        if (player.playerInventoryManager.currentRightHandWeapon == WorldItemDatabase.Instance.unarmedWeapon)
        {

            //如果RETURNING 或者 NOT TWO HANDING，直接RESET BOOL
            if (player.IsOwner)
            {
                player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                player.playerNetworkManager.isTwoHandingRightWeapon.Value = false;
            }

            return;
        }

        //更新动画
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);

        //将non two hand weapon放在back slot
        player.playerEquipmentManager.backSlot.PlaceWeaponModelInUnequipedSlot(leftWeaponModel, player.playerInventoryManager.currentLeftHandWeapon.weaponClass, player);

        //将two hand weapon放在right hand slot
        rightHandWeaponSlot.PlaceWeaponIntoSlot(rightWeaponModel);

        rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
    }

    public void TwoHandLeftWeapon()
    {
        //检查是否可以双持
        if (player.playerInventoryManager.currentLeftHandWeapon == WorldItemDatabase.Instance.unarmedWeapon)
        {

            //如果RETURNING 或者 NOT TWO HANDING，直接RESET BOOL
            if (player.IsOwner)
            {
                player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                player.playerNetworkManager.isTwoHandingLeftWeapon.Value = false;
            }

            return;
        }

        //更新动画
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentLeftHandWeapon.weaponAnimator);

        //将non two hand weapon放在back slot
        player.playerEquipmentManager.backSlot.PlaceWeaponModelInUnequipedSlot(rightWeaponModel, player.playerInventoryManager.currentRightHandWeapon.weaponClass, player);

        //将two hand weapon放在right hand slot
        rightHandWeaponSlot.PlaceWeaponIntoSlot(leftWeaponModel);

        rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
    }

    // Damage Colliders
    public void OpenDamageCollider()
    {
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            rightHandWeaponManager.meleeWeaponDamageCollider.EnableDamageCollider();
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentRightHandWeapon.wooshes));
        }
        else if (player.playerNetworkManager.isUsingLeftHand.Value)
        {
            leftHandWeaponManager.meleeWeaponDamageCollider.EnableDamageCollider();
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentLeftHandWeapon.wooshes));
        }

        //双手共持
    }

    public void CloseDamageCollider()
    {
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            rightHandWeaponManager.meleeWeaponDamageCollider.DisableDamageCollider();
        }
        else if (player.playerNetworkManager.isUsingLeftHand.Value)
        {
            leftHandWeaponManager.meleeWeaponDamageCollider.DisableDamageCollider();
        }
        //双手共持
    }
}
