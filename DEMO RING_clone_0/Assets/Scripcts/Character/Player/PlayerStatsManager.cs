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

    public void CaculateTotalCharacterAborption()
    {
        armorPhysicalDamageAbsorption = 0;
        armorMagicDamageAbsorption = 0;
        armorFireDamageAbsorption = 0;
        armorLightningDamageAbsorption = 0;
        armorHolyDamageAbsorption = 0;

        armorImmunity = 0;
        armorRobustness = 0;
        armorFocus = 0;
        armorVitality = 0;

        basePoiseDefense = 0;

        CalculateSinleAborption(player.playerInventoryManager.headEquipment);
        CalculateSinleAborption(player.playerInventoryManager.bodyEquipment);
        CalculateSinleAborption(player.playerInventoryManager.handEquipment);
        CalculateSinleAborption(player.playerInventoryManager.legEquipment);
    }

    private void CalculateSinleAborption(ArmorItem equipmentItem)
    {
        if (equipmentItem == null)
            return;

        armorPhysicalDamageAbsorption += equipmentItem.physicalDamageAbsorption;
        armorMagicDamageAbsorption += equipmentItem.magicDamageAbsorption;
        armorFireDamageAbsorption += equipmentItem.fireDamageAbsorption;
        armorLightningDamageAbsorption += equipmentItem.lightningDamageAbsorption;
        armorHolyDamageAbsorption += equipmentItem.holyDamageAbsorption;

        armorImmunity += equipmentItem.immunity;
        armorRobustness += equipmentItem.robustness;
        armorFocus += equipmentItem.focus;
        armorVitality += equipmentItem.vitality;

        basePoiseDefense += equipmentItem.poise;
    }

}
