using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectsManager : MonoBehaviour
{
    //INSTANT EFFECTS 受伤 治疗
    
    //TIMED EFFECTS 中毒 聚集的效果
    
    //STATIC EFFECTS 装备 固定效果

    private CharacterManager character;

    [Header("VFX")]
    [SerializeField] private GameObject bloodSplatterVFX;

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public virtual void ProcessInstantEffect(InstantCharacterEffect effect)
    {
        effect.ProcessEffect(character);
    }
    
    public void PlayBloodSplatterVFX(Vector3 contactPoint)
    {
        if (bloodSplatterVFX != null)
        {
            //如果有血迹特效预设，则实例化它
            GameObject bloodSplatter = Instantiate(bloodSplatterVFX, contactPoint, Quaternion.identity);
        }
        else
        {
            //如果没有血迹特效预设，则尝试从世界角色效果管理器中获取并实例化它
            GameObject bloodSplatter = Instantiate(WorldCharacterEffectsManager.instance.bloodSplatterVFX, contactPoint, Quaternion.identity);
        }
    }
}
