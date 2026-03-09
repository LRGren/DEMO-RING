using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : CharacterStatsManager
{
    private PlayerManager player;
    
    protected override void Awake()
    {
        base.Awake();
        
        player = GetComponent<PlayerManager>();
    }

    protected override void Start()
    {
        base.Start();

        //创建角色时 没有计算过 要先行计算第一次
        CalculateHealthBasedOnVitalityLevel(player.playerNetworkManager.vitality.Value);
        CalculateStaminaBasedOnEnduranceLevel(player.playerNetworkManager.endurance.Value);
    }
}
