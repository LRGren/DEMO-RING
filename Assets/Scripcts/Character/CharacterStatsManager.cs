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
        endurance = System.Math.Clamp(endurance, 1, 99);

        if (endurance <= 15)
        {
            // 1 - 15: 基础递增 (+1.0 ~ +1.1 精力/点)
            return 80 + (int)System.Math.Round((endurance - 1) * 1.07f);
        }
        else if (endurance <= 30)
        {
            // 16 - 30: 高收益区 (+1.3 ~ +2.0 精力/点)
            return 95 + (int)System.Math.Round((endurance - 15) * 2.33f);
        }
        else if (endurance <= 50)
        {
            // 31 - 50: 软上限过渡 (+1.0 精力/点)
            return 130 + (int)System.Math.Round((endurance - 30) * 1.0f);
        }
        else
        {
            // 51 - 99: 硬上限极低收益 (平均每 2~3 级才 +1 点精力)
            return 150 + (int)System.Math.Round((endurance - 50) * 0.408f);
        }
    }

    public int CalculateFocusBasedOnMindLevel(int mind)
    {
        mind = System.Math.Clamp(mind, 1, 99);

        if (mind <= 15)
        {
            // 1 - 15: Low scaling (+3 to +4 FP per point)
            return 50 + (int)System.Math.Round((mind - 1) * 3.214f);
        }
        else if (mind <= 35)
        {
            // 16 - 35: High scaling (+5 to +6 FP per point)
            return 95 + (int)System.Math.Round((mind - 15) * 5.25f);
        }
        else if (mind <= 50)
        {
            // 36 - 50: Peak scaling (+6 to +7 FP per point, Cap at 300)
            return 200 + (int)System.Math.Round((mind - 35) * 6.667f);
        }
        else if (mind <= 60)
        {
            // 51 - 60: Soft Cap 1 (+5 FP per point)
            return 300 + (mind - 50) * 5;
        }
        else
        {
            // 61 - 99: Soft Cap 2 / Hard Cap (+2 to +3 FP per point)
            return 350 + (int)System.Math.Round((mind - 60) * 2.564f);
        }
    }

    public int CalculateHealthBasedOnVitalityLevel(int vitality)
    {
        vitality = System.Math.Clamp(vitality, 1, 99);

        if (vitality <= 25)
        {
            // 1 - 25: 低收益期 (+20 ~ +26 HP/点)
            return 300 + (int)System.Math.Round((vitality - 1) * 20.83f);
        }
        else if (vitality <= 40)
        {
            // 26 - 40: 高收益黄金区 (+35 ~ +48 HP/点)
            return 800 + (int)System.Math.Round((vitality - 25) * 43.33f);
        }
        else if (vitality <= 60)
        {
            // 41 - 60: 第一软上限衰减 (+15 ~ +26 HP/点)
            return 1450 + (int)System.Math.Round((vitality - 40) * 22.5f);
        }
        else
        {
            // 61 - 99: 硬上限极低收益 (+1 ~ +3 HP/点)
            return 1900 + (int)System.Math.Round((vitality - 60) * 5.128f);
        }
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
