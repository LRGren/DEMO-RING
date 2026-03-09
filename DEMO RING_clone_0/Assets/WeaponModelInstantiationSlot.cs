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

    public void LoadWeapon(GameObject weaponModel)
    {
        currentWeapon = weaponModel;
        currentWeapon.transform.parent = gameObject.transform;
        
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.transform.localScale = Vector3.one;
    }

}
