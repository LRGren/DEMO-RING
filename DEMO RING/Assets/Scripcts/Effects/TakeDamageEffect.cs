using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Damage")]
public class TakeDamageEffect : InstantCharacterEffect
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
        base.ProcessEffect(character);
        
        //如果角色死了 无需继续计算
        if(character.isDead.Value)
            return;
        
        //是否无敌
        
        //计算伤害
        CalculteDamage(character);
        //确认受击方向
        PlayDirectionalBasedDamageAnimation(character);
        //受击动画
        //确认累计效果 如 毒
        //SOUND FX
        PlayDamageSFX(character);
        //VFX 溅血效果
        PlayDamageVFX(character);
        
        //如果是 AI 将敌人设置为发动攻击的人
    }

    private void CalculteDamage(CharacterManager character)
    {
        if(!character.IsOwner)
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
        
        Debug.Log("Cause Damage");
        character.characterNetworkManager.currentHealth.Value -= finalDamageDealt;
        
        //计算削韧值
    }

    private void PlayDamageVFX(CharacterManager character)
    {
        //火焰伤害特效
        //雷电伤害特效
        //等等

        character.characterEffectsManager.PlayBloodSplatterVFX(contactPoint);
    }

    private void PlayDamageSFX(CharacterManager character)
    {
        AudioClip physicalSFX = WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.physicalDamageSFX);

        character.characterSoundFXManager.PlaySoundFX(physicalSFX);
    }

    private void PlayDirectionalBasedDamageAnimation(CharacterManager character)
    {
        if(!character.IsOwner)
            return;

        //失衡
        poiseIsBroken = true;

        if(angleHitFrom >= 145 && angleHitFrom <= 180)
        {
            //front
            damageAnimation = character.characterAnimatorManager.hit_Forward_Medium_01;
        }
        else if(angleHitFrom <= -145 && angleHitFrom > 180)
        {
            //front
            damageAnimation = character.characterAnimatorManager.hit_Forward_Medium_01;
        }
        else if(angleHitFrom >= -45 && angleHitFrom <= 45)
        {
            //back
            damageAnimation = character.characterAnimatorManager.hit_Back_Medium_01;
        }
        else if(angleHitFrom >= -144 && angleHitFrom <= -45)
        {
            //left
            damageAnimation = character.characterAnimatorManager.hit_Left_Medium_01;
        }
        else if (angleHitFrom >= 45 && angleHitFrom <= 144)
        {
            //right
            damageAnimation = character.characterAnimatorManager.hit_Right_Medium_01;
        }
        
        if(poiseIsBroken)
        {
            character.characterAnimatorManager.PlayerTargetActionAnimation(damageAnimation, true);
        }
    }

}