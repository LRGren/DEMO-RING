using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponModelInstantiationSlot : MonoBehaviour
{
    //这个槽位是什么 （左手 右手 背上 双手共持）
    public WeaponModelSlot weaponSlot;
    public GameObject currentWeapon;

    public void UnloadWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
    }

    public void PlaceWeaponIntoSlot(GameObject weaponModel)
    {
        currentWeapon = weaponModel;
        currentWeapon.transform.parent = gameObject.transform;

        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.transform.localScale = Vector3.one;
    }

    public void PlaceWeaponModelInUnequipedSlot(GameObject weaponModel, WeaponClass weaponClass, PlayerManager player)
    {
        // TO DO, MOVE WEAPON ON BACK CLOSER OR MORE OUTWARD DEPENDING ON CHEST EQUIPMENT (SO IT DOESNT APPEAR TO FLOAT)

        currentWeapon = weaponModel;
        weaponModel.transform.parent = transform;

        switch (weaponClass)
        {
            case WeaponClass.StraightSword:
                weaponModel.transform.localPosition = new Vector3(0.064f, 0f, -0.06f);
                weaponModel.transform.localRotation = Quaternion.Euler(194, 90, -0.22f);
                break;
            case WeaponClass.MediumShield:
                weaponModel.transform.localPosition = new Vector3(0.219f, -0.036f, 0.017f);
                weaponModel.transform.localRotation = Quaternion.Euler(-12.423f, -44.594f, 147.319f);
                break;
            default:
                break;
        }
    }
}
