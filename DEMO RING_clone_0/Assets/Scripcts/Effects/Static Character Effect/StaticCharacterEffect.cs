using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCharacterEffect : ScriptableObject
{
    [Header("Effect I.D")]
    public int staticEffectID;

    public virtual void ProcessEffect(CharacterManager character)
    {
        //在这里实现静态效果的处理逻辑
    }

    public virtual void RemoveEffect(CharacterManager character)
    {
        //在这里实现静态效果的移除逻辑
    }

}


