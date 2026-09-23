using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ranged Projectile")]
public class RangedProjectileItem : Item
{
    [Header("Projectile Class")]
    public ProjectileClass projectileClass;

    [Header("Velocity")]
    public float forwardVelocity = 2200f;
    public float upwardVelocity = 0f;
    public float ammoMass = 0.01f;

    [Header("Capacity")]
    public int maxAmmoAmount = 999;
    public int currentAmmoAmount = 1;

    [Header("Damage")]
    public int physicalDamage = 0;
    public int magicalDamage = 0;
    public int fireDamage = 0;
    public int holyDamage = 0;
    public int lightningDamage = 0;
    public int criticalDamage = 100;

    [Header("Model")]
    public GameObject drawProjectileModel;
    public GameObject releaseProjectileModel;

    public virtual void SetAmmotAmount(int amount)
    {
        currentAmmoAmount = amount;
    }

}
