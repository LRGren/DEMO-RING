using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundFXManager : CharacterSoundFXManager
{
    PlayerManager player;

    void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    public override void PlayBlockSFX()
    {
        PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerCombatManager.currentWeaponBedingUsed.blocks));
    }
}
