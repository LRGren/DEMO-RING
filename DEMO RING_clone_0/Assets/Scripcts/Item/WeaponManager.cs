using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public MeleeWeaponDamageCollider meleeWeaponDamageCollider;

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

        meleeWeaponDamageCollider.light_Attack_01_Modifier = weaponItem.light_Attack_01_Modifier;
        meleeWeaponDamageCollider.heavy_Attack_01_Modifier = weaponItem.heavy_Attack_01_Modifier;
        meleeWeaponDamageCollider.charged_Attack_01_Modifier = weaponItem.charged_Attack_01_Modifier;

    }
}
