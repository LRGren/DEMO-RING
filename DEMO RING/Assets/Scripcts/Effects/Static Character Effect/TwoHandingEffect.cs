using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Static Effects/Two Handing Effect")]
public class TwoHandingEffect : StaticCharacterEffect
{
    [SerializeField] private int strengthGainedFromTwoHanding;

    public override void ProcessEffect(CharacterManager character)
    {
        base.ProcessEffect(character);

        if (character.IsOwner)
        {
            strengthGainedFromTwoHanding = Mathf.RoundToInt(character.characterNetworkManager.strength.Value * 0.5f);
            character.characterNetworkManager.strengthModifiers.Value += strengthGainedFromTwoHanding;
        }
    }

    public override void RemoveEffect(CharacterManager character)
    {
        base.RemoveEffect(character);

        if (character.IsOwner)
        {
            character.characterNetworkManager.strengthModifiers.Value -= strengthGainedFromTwoHanding;
        }
    }

}
