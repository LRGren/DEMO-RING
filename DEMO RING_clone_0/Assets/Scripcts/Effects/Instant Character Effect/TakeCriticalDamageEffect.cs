using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Critical Damage")]
public class TakeCriticalDamageEffect : TakeDamageEffect
{
    public override void ProcessEffect(CharacterManager character)
    {
        if (character.characterNetworkManager.isInvulnerable.Value)
        {
            //Debug.Log("Character is Invulnerable, No Damage Taken");
            return;
        }

        //如果角色死了 无需继续计算
        if (character.isDead.Value)
            return;

        //计算伤害
        CalculteDamage(character);

        character.characterCombatManager.pendingCriticalAttackDamage = finalDamageDealt;

        //如果是 AI 将敌人设置为发动攻击的人
    }

    protected override void CalculteDamage(CharacterManager character)
    {

        if (!character.IsOwner)
            return;

        if (characterCausingDamage != null)
        {
            //确认对方是否有伤害修饰符
        }

        //属性减伤

        //装备减伤

        //将所有伤害加起来
        finalDamageDealt = Mathf.RoundToInt(physicalDamage + magicalDamage + fireDamage + lightningDamage + holyDamage);
        if (finalDamageDealt <= 0)
        {
            finalDamageDealt = 1;
        }

        //Debug.Log("Cause Damage");
        //对于critical damage， 我们会在动画中的特定帧调用，所以我们不在这里直接扣血，而是通过动画事件来扣血
        //character.characterNetworkManager.currentHealth.Value -= finalDamageDealt;

        //计算削韧值
        character.characterStatsManager.totalPoiseDamage -= poiseDamage;
        character.characterCombatManager.previousPoiseDamageTaken = poiseDamage;
        //Debug.Log("Poise Damage Taken: " + poiseDamage + " Total Poise Damage: " + character.characterStatsManager.totalPoiseDamage);

        float remainingPoise = character.characterStatsManager.basePoiseDefense + character.characterStatsManager.offensivePoiseBonus + character.characterStatsManager.totalPoiseDamage;

        if (remainingPoise <= 0)
        {
            poiseIsBroken = true;
            character.characterStatsManager.totalPoiseDamage = 0;
        }

        character.characterStatsManager.poiseResetTimer = character.characterStatsManager.defaultPoiseResetTimer;
    }

}
