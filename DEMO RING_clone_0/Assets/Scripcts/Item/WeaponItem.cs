using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    //ANIMATION CONTROLLE 覆盖（基于武器需要有不同的攻击模组）

    [Header("Weapon Model")]
    public GameObject weaponModel;
    
    [Header("Weapon Requirements")]
    public int strengthREQ = 0;
    public int dexterityREQ = 0;
    public int intelligenceREQ = 0;
    public int faithREQ = 0;

    [Header("Weapon Base Damage")]
    public int physicalDamage = 0;
    public int magicalDamage = 0;
    public int fireDamage = 0;
    public int holyDamage = 0;
    public int lightningDamage = 0;
    
    //武器格挡强度

    [Header("Weapon Poise")]
    public float poiseDamage = 10;
    //出手硬直
    
    //武器修饰符
    //轻攻击修饰
    //重攻击修饰
    //暴击伤害修饰 等等
    
    [Header("Stamina Cost")]
    public int basicStaminaCost = 0;
    //跑功耐力消耗
    //轻攻击耐力消耗修饰
    //重攻击耐力小号修饰
    
    //基于物品的动作 （RB,RT,LB,LT）
    
    //ASH OF WAR
    
    //格挡音效

}
