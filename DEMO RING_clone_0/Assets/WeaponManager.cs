using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] MeleeWeaponDamageCollider meleeWeaponDamageCollider;

    private void Awake()
    {
        meleeWeaponDamageCollider = GetComponentInChildren<MeleeWeaponDamageCollider>();
    }

    public void SetWeaponDamage(CharacterManager characterWeildingDamage,WeaponItem weaponItem)
    {
        meleeWeaponDamageCollider.characterCasuingDamage = characterWeildingDamage;
        meleeWeaponDamageCollider.physicalDamage = weaponItem.physicalDamage;
        meleeWeaponDamageCollider.magicalDamage = weaponItem.magicalDamage;
        meleeWeaponDamageCollider.fireDamage = weaponItem.fireDamage;
        meleeWeaponDamageCollider.holyDamage = weaponItem.holyDamage;
        meleeWeaponDamageCollider.lightningDamage = weaponItem.lightningDamage;
    }
}
