using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatsManager : MonoBehaviour
{
    private CharacterManager character;

    [Header("Stamina Regeneration")]
    [SerializeField] private float staminaRegenerationAmount = 10;
    private float staminaRegenerationTimer = 0;
    private float staminaRegenerationTicker = 0;
    [SerializeField] private float staminaRegenerationDelay = 0.5f;

    [Header("Blocking Absorption")]
    public float blockingPhysicalAbsorption;
    public float blockingMagicalAbsorption;
    public float blockingFireAbsorption;
    public float blockingLightningAbsorption;
    public float blockingHolyAbsorption;
    public float blockingStaminaAbsorption;

    [Header("Armor Absorption")]
    public float armorPhysicalDamageAbsorption;
    public float armorMagicDamageAbsorption;
    public float armorFireDamageAbsorption;
    public float armorLightningDamageAbsorption;
    public float armorHolyDamageAbsorption;

    [Header("Armor Resistance Bonus")]
    public float armorImmunity;      // RESISTANCE TO ROT AND POISON
    public float armorRobustness;    // RESISTANCE TO BLEED AND FROST
    public float armorFocus;         // RESISTANCE TO MADNESS AND SLEEP
    public float armorVitality;      // RESISTANCE TO DEATH CURSE

    [Header("Poise")]
    public float totalPoiseDamage;              //一段时间内收到的削韧值总和
    public float offensivePoiseBonus;           //攻击时增加的韧性值
    public float basePoiseDefense;              //基础韧性值(来自装备，护符等)
    public float defaultPoiseResetTimer = 8f;   //韧性值重置时间
    public float poiseResetTimer;               //韧性值重置计时器


    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        HandlePoiseResetTimer();
    }

    public int CalculateStaminaBasedOnEnduranceLevel(int endurance)
    {
        float stamina = 0;

        //耐力计算公式
        stamina = endurance * 15;

        return Mathf.RoundToInt(stamina);
    }

    public int CalculateHealthBasedOnVitalityLevel(int vitality)
    {
        float health = 0;

        //耐力计算公式
        health = vitality * 15;

        return Mathf.RoundToInt(health);
    }

    public void StaminaRegeneration()
    {
        if (character.isDead.Value)
            return;

        if (!character.IsOwner)
            return;

        if (character.characterNetworkManager.isBlocking.Value)
            return;

        if (character.characterNetworkManager.isSprinting.Value)
            return;

        if (character.isPerformingAction)
            return;

        staminaRegenerationTimer += Time.deltaTime;
        if (staminaRegenerationTimer >= staminaRegenerationDelay)
        {
            if (character.characterNetworkManager.currentStamina.Value < character.characterNetworkManager.maxStamina.Value)
            {
                staminaRegenerationTicker += Time.deltaTime;
                if (staminaRegenerationTicker > 0.1f)
                {
                    staminaRegenerationTicker = 0;
                    character.characterNetworkManager.currentStamina.Value += staminaRegenerationAmount;
                }
            }
        }
    }

    public void ResetStaminaTimer(float previousStaminaAmount, float currentStaminaAmount)
    {
        if (currentStaminaAmount < previousStaminaAmount)
            staminaRegenerationTimer = 0;
    }

    protected virtual void HandlePoiseResetTimer()
    {
        if (poiseResetTimer > 0)
        {
            poiseResetTimer -= Time.deltaTime;
        }
        else
        {
            totalPoiseDamage = 0;
            poiseResetTimer = 0;
        }
    }
}
