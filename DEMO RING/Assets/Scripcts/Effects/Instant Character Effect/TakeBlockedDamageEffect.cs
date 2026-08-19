using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Blocked Damage")]
public class TakeBlockedDamageEffect : InstantCharacterEffect
{
    [Header("Character Causing Damage")]
    CharacterManager characterCausingDamage;

    [Header("Damage")]
    public float physicalDamage = 0;
    public float magicalDamage = 0;
    public float fireDamage = 0;
    public float lightningDamage = 0;
    public float holyDamage = 0;

    [Header("Final Damage")]
    private int finalDamageDealt = 0;

    [Header("Poise")]
    public float poiseDamage = 0;//削韧
    public bool poiseIsBroken = false;

    [Header("Stamina Cost")]
    public float staminaCost = 0;
    public float finalStaminaCost = 0;

    //TODO:BUILD UP EFFECTS

    [Header("Animation")]
    public bool playDamageAnimation = true;
    public bool manuallySelectDamageAnimation = false;
    public string damageAnimation = "";

    [Header("Sound FX")]
    public bool willPlaySoundFX = true;
    public AudioClip elementalDamageSoundFX;

    [Header("Direction Damage Taken From")]
    public float angleHitFrom;//决定受击后的方向
    public Vector3 contactPoint;//生成BLOOD FX的粒子效果的位置



    public override void ProcessEffect(CharacterManager character)
    {
        if (character.characterNetworkManager.isInvulnerable.Value)
        {
            //Debug.Log("Character is Invulnerable, No Damage Taken");
            return;
        }

        base.ProcessEffect(character);

        //如果角色死了 无需继续计算
        if (character.isDead.Value)
            return;

        //计算伤害
        CalculateDamage(character);
        CalculateStaminaCost(character);

        //Debug.Log("Final Damage Dealt: " + finalDamageDealt);

        //确认受击方向
        PlayDirectionalBasedDamageAnimation(character);
        //受击动画
        //确认累计效果 如 毒
        //SOUND FX
        PlayDamageSFX(character);
        //VFX 溅血效果
        PlayDamageVFX(character);

        CheckForPoiseBreak(character);

        //如果是 AI 将敌人设置为发动攻击的人
    }

    private void CalculateDamage(CharacterManager character)
    {
        if (!character.IsOwner)
            return;

        if (characterCausingDamage != null)
        {
            //确认对方是否有伤害修饰符
        }

        //属性减伤

        //装备减伤

        //防御减伤
        physicalDamage -= physicalDamage * character.characterStatsManager.blockingPhysicalAbsorption / 100;
        magicalDamage -= magicalDamage * character.characterStatsManager.blockingMagicalAbsorption / 100;
        fireDamage -= fireDamage * character.characterStatsManager.blockingFireAbsorption / 100;
        lightningDamage -= lightningDamage * character.characterStatsManager.blockingLightningAbsorption / 100;
        holyDamage -= holyDamage * character.characterStatsManager.blockingHolyAbsorption / 100;

        //将所有伤害加起来
        finalDamageDealt = Mathf.RoundToInt(physicalDamage + magicalDamage + fireDamage + lightningDamage + holyDamage);
        if (finalDamageDealt <= 0)
        {
            finalDamageDealt = 1;
        }

        //Debug.Log("Cause Damage");
        character.characterNetworkManager.currentHealth.Value -= finalDamageDealt;

        //计算削韧值
    }

    private void CalculateStaminaCost(CharacterManager character)
    {
        finalStaminaCost = staminaCost - staminaCost * character.characterStatsManager.blockingStaminaAbsorption / 100;

        if (finalStaminaCost <= 0)
        {
            finalStaminaCost = 1;
        }

        character.characterNetworkManager.currentStamina.Value -= finalStaminaCost;
    }

    private void CheckForPoiseBreak(CharacterManager character)
    {
        if (!character.IsOwner)
            return;

        if (character.characterNetworkManager.currentStamina.Value <= 0)
        {
            PlayerManager player = character as PlayerManager;
            if (player != null && player.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                character.characterAnimatorManager.PlayerTargetActionAnimation("Guard_Break_01", true);
                //Play SFX
            }
            else
            {
                character.characterAnimatorManager.PlayerTargetActionAnimation("Guard_Break_01", true);
                //Play SFX
            }


            character.characterNetworkManager.isBlocking.Value = false;
        }
    }

    private void PlayDamageVFX(CharacterManager character)
    {
        //火焰伤害特效
        //雷电伤害特效
        //等等

        //根据武器类型和武器特效来决定效果
    }

    private void PlayDamageSFX(CharacterManager character)
    {
        //根据武器类型和武器特效来决定效果
        character.characterSoundFXManager.PlayBlockSFX();
    }

    private void PlayDirectionalBasedDamageAnimation(CharacterManager character)
    {
        if (!character.IsOwner)
            return;

        if (character.isDead.Value)
            return;

        //TODO: 以后根据双手武器和单手武器来决定受击动画
        DamageIntensity damageIntensity = WorldUtilityManager.instance.GetDamageIntensityBasedOnPoiseDamage(poiseDamage);

        PlayerManager player = character as PlayerManager;
        if (player != null && player.playerNetworkManager.isTwoHandingWeapon.Value)
        {
            switch (damageIntensity)
            {
                case DamageIntensity.Ping:
                    damageAnimation = "TH_Block_Ping_01";
                    break;
                case DamageIntensity.Light:
                    damageAnimation = "TH_Block_Light_01";
                    break;
                case DamageIntensity.Medium:
                    damageAnimation = "TH_Block_Medium_01";
                    break;
                case DamageIntensity.Heavy:
                    damageAnimation = "TH_Block_Heavy_01";
                    break;
                case DamageIntensity.Colossal:
                    damageAnimation = "TH_Block_Colossal_01";
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (damageIntensity)
            {
                case DamageIntensity.Ping:
                    damageAnimation = "Block_Ping_01";
                    break;
                case DamageIntensity.Light:
                    damageAnimation = "Block_Light_01";
                    break;
                case DamageIntensity.Medium:
                    damageAnimation = "Block_Medium_01";
                    break;
                case DamageIntensity.Heavy:
                    damageAnimation = "Block_Heavy_01";
                    break;
                case DamageIntensity.Colossal:
                    damageAnimation = "Block_Colossal_01";
                    break;
                default:
                    break;
            }
        }

        character.characterAnimatorManager.PlayerTargetActionAnimation(damageAnimation, true);
    }


}
