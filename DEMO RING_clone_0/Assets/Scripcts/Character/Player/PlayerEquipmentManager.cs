using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerEquipmentManager : CharacterEquipmentManager
{
    PlayerManager player;
    public WeaponModelInstantiationSlot rightHandSlot;
    public WeaponModelInstantiationSlot leftHandSlot;

    [SerializeField] WeaponManager rightHandWeaponManager;
    [SerializeField] WeaponManager leftHandWeaponManager;

    public GameObject rightWeaponModel;
    public GameObject leftWeaponModel;
    
    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
        
        InitialiseWeaponSlots();
    }

    protected override void Start()
    {
        base.Start();
        
        LoadWeaponsOnBothHands();
    }

    private void InitialiseWeaponSlots()
    {
        WeaponModelInstantiationSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>();

        foreach (var weapon in weaponSlots)
        {
            if (weapon.weaponSlot == WeaponModelSlot.RightHand)
            {
                rightHandSlot = weapon;
            }
            else if(weapon.weaponSlot == WeaponModelSlot.LeftHand)
            {
                leftHandSlot = weapon;
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
            rightHandSlot.UnloadWeapon();

            rightWeaponModel = Instantiate(player.playerInventoryManager.currentRightHandWeapon.weaponModel);
            rightHandSlot.LoadWeapon(rightWeaponModel);

            rightHandWeaponManager = rightWeaponModel.GetComponentInChildren<WeaponManager>();
            rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        }
    }

    public void SwitchRightWeapon()
    {
        if (!player.IsOwner)
            return;

        player.playerAnimatorManager.PlayerTargetActionAnimation("Swap_Right_Weapon_01", false, true, true, true);

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
            leftHandSlot.UnloadWeapon();

            leftWeaponModel = Instantiate(player.playerInventoryManager.currentLeftHandWeapon.weaponModel);
            leftHandSlot.LoadWeapon(leftWeaponModel);

            leftHandWeaponManager = leftWeaponModel.GetComponentInChildren<WeaponManager>();
            leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
        }
    }
    
    public void SwitchLeftWeapon()
    {

    }

    // Damage Colliders
    public void OpenDamageCollider()
    {
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            rightHandWeaponManager.meleeWeaponDamageCollider.EnableDamageCollider();
        }
        else if (player.playerNetworkManager.isUsingLeftHand.Value)
        {
            leftHandWeaponManager.meleeWeaponDamageCollider.EnableDamageCollider();
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
