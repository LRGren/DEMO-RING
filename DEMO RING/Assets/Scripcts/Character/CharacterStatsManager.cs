using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatsManager : MonoBehaviour
{
    private CharacterManager character;
    
    [Header("Stamina Regeneration")]
    [SerializeField] private float staminaRegenerationAmount = 2;
    private float staminaRegenerationTimer = 0;
    private float staminaRegenerationTicker = 0;
    [SerializeField] private float staminaRegenerationDelay = 2;

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    protected virtual void Start()
    {
        
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
        if (!character.IsOwner)
            return;

        if (character.characterNetworkManager.isSprinting.Value)
            return;
        
        if(character.isPerformingAction)
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
        if(currentStaminaAmount < previousStaminaAmount)
            staminaRegenerationTimer = 0;
    }
}
