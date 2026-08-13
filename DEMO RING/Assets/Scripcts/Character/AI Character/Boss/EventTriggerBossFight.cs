using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerBossFight : MonoBehaviour
{
    [SerializeField] private string bossID = "Boss_001";

    private void OnTriggerEnter(Collider other)
    {
        AIBossCharacterManager bossCharacter = WorldAIManager.instance.GetBossCharacterByID(bossID);

        if (WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.ContainsKey(bossID) && WorldSaveGameManager.instance.currentCharacterData.bossesDefeated[bossID])
        {
            // Boss has already been defeated, do not trigger the fight
            return;
        }

        if (bossCharacter != null)
        {
            bossCharacter.AwakenBoss();
        }
    }
}
