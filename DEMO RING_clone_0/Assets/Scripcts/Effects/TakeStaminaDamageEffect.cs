using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Stamina Damage")]
public class TakeStaminaDamageEffect : InstantCharacterEffect
{
    public float staminaDamage;
    
    public override void ProcessEffect(CharacterManager character)
    {
        CalculateStaminaDamage(character);
    }

    private void CalculateStaminaDamage(CharacterManager character)
    {
        //考虑其他EFFECTS的影响
        //在使用前改变它的值
        //播放SFX或VFX
        if (character.IsOwner)
        {
            character.characterNetworkManager.currentStamina.Value -= staminaDamage;
        }
    }
    
}
