using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    //ANIMATION CONTROLLE 覆盖（基于武器需要有不同的攻击模组）
    [Header("Weapon Animator Override Controller")]
    public AnimatorOverrideController weaponAnimator;

    [Header("Weapon Type")]
    public WeaponType weaponType;

    [Header("Weapon Class")]
    public WeaponClass weaponClass;

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

    [Header("Weapon Blocking Absorption")]
    public float physicalDamageAbsorption = 100;
    public float magicalDamageAbsorption = 0;
    public float fireDamageAbsorption = 0;
    public float holyDamageAbsorption = 0;
    public float lightningDamageAbsorption = 0;
    public float staminaAbsorption = 0; // 格挡时减免的耐力消耗百分比(0-100)，越高越省耐力

    [Header("Attack Modifiers")]
    //武器修饰符
    //轻攻击修饰
    public float light_Attack_01_Modifier = 0.9f;
    public float light_Attack_02_Modifier = 1.2f;

    //重攻击修饰
    public float heavy_Attack_01_Modifier = 1.4f;
    public float heavy_Attack_02_Modifier = 1.6f;
    public float charged_Attack_01_Modifier = 2.2f;
    public float charged_Attack_02_Modifier = 2.5f;

    public float run_Attack_01_Modifier = 1.0f;
    public float roll_Attack_01_Modifier = 1.0f;
    public float backstep_Attack_01_Modifier = 1.0f;
    //暴击伤害修饰 等等

    [Header("Stamina Cost Modifiers")]
    public int basicStaminaCost = 0;
    //跑功耐力消耗

    //轻攻击耐力消耗修饰
    public float lightAttackStaminaModifier = 0.9f;
    //重攻击耐力消耗修饰
    public float heavyAttackStaminaModifier = 1.2f;
    public float chargedAttackStaminaModifier = 1.5f;
    public float runningAttackStaminaModifier = 1.0f;
    public float rollingAttackStaminaModifier = 1.0f;
    public float backstepAttackStaminaModifier = 1.0f;

    //基于物品的动作 （RB,RT,LB,LT）
    [Header("Weapon Actions")]
    public WeaponItemAction oh_RB_Action;//oh for one hand
    public WeaponItemAction oh_RT_Action;//oh for one hand
    public WeaponItemAction oh_LB_Action;//oh for one hand

    //ASH OF WAR

    //格挡音效

    //武器音效
    [Header("Weapon SFX")]
    public AudioClip[] wooshes;
    public AudioClip[] blocks;
}
