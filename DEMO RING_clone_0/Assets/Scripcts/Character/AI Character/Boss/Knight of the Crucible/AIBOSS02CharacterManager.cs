using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBOSS02CharacterManager : AIBossCharacterManager
{
    public AIBOSS02SoundFXManager aiBOSS02SoundFXManager;

    override protected void Awake()
    {
        base.Awake();

        aiBOSS02SoundFXManager = GetComponent<AIBOSS02SoundFXManager>();
    }
}
