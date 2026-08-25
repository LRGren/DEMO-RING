using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AICharacterNetworkManager : CharacterNetworkManager
{
    AICharacterManager aiCharacterManager;

    protected override void Awake()
    {
        base.Awake();

        aiCharacterManager = GetComponent<AICharacterManager>();
    }

    public override void OnIsDeadChanged(bool old, bool newStatus)
    {
        base.OnIsDeadChanged(old, newStatus);

        if (newStatus)
        {
            aiCharacterManager.aiCharacterInventoryManager.DropItem();
        }
    }
}
