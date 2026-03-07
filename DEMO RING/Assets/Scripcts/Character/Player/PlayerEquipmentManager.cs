using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentManager : CharacterEquipmentManager
{
    PlayerManager player;
    public WeaponModelInstantiationSlot rightHandSlot;
    public WeaponModelInstantiationSlot leftHandSlot;

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

    public void LoadRightWeapon()
    {
        if (player.playerInventoryManager.currentRightHandWeapon != null)
        {
            rightWeaponModel = Instantiate(player.playerInventoryManager.currentRightHandWeapon.weaponModel);
            rightHandSlot.LoadWeapon(rightWeaponModel);
        }
    }

    public void LoadLeftWeapon()
    {
        if (player.playerInventoryManager.currentLeftHandWeapon != null)
        {
            leftWeaponModel = Instantiate(player.playerInventoryManager.currentLeftHandWeapon.weaponModel);
            leftHandSlot.LoadWeapon(leftWeaponModel);
        }
    }
    
}
