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

    [Header("Static Effects")]
    [SerializeField] private List<StaticCharacterEffect> staticEffects;

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

    public void AddStaticEffect(StaticCharacterEffect effect)
    {
        staticEffects.Add(effect);

        effect.ProcessEffect(character);

        for (int i = staticEffects.Count - 1; i >= 0; i--)
        {
            if (staticEffects[i] == null)
            {
                staticEffects.RemoveAt(i);
            }
        }
    }

    public void RemoveStaticEffect(int effectID)
    {
        for (int i = 0; i < staticEffects.Count; i++)
        {
            if (staticEffects[i].staticEffectID == effectID)
            {
                staticEffects[i].RemoveEffect(character);
                staticEffects.RemoveAt(i);
            }
        }

        for (int i = staticEffects.Count - 1; i >= 0; i--)
        {
            if (staticEffects[i] == null)
            {
                staticEffects.RemoveAt(i);
            }
        }
    }
}
