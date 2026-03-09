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

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public virtual void ProcessInstantEffect(InstantCharacterEffect effect)
    {
        effect.ProcessEffect(character);
    }
    
}
