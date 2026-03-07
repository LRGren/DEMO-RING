using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetworkManager : CharacterNetworkManager
{
    private PlayerManager player;
    
    public NetworkVariable<FixedString64Bytes> characterName = new NetworkVariable<FixedString64Bytes>("sereinjians", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected override void Awake()
    {
        base.Awake();
        
        player = GetComponent<PlayerManager>();
    }

    public void SetNewMaxHealthValue(int oldVitality, int newVitality)
    {
        maxHealth.Value = player.playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(newVitality);
        PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(maxHealth.Value);
        currentHealth.Value = maxHealth.Value;
        
    }
    
    public void SetNewMaxStaminaValue(int oldEndurance, int newEndurance)
    {
        maxStamina.Value = player.playerStatsManager.CalculateHealthBasedOnVitalityLevel(newEndurance);
        PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(maxStamina.Value);
        currentStamina.Value = maxStamina.Value;
    }
    
}
