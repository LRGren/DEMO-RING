using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerBossFight : MonoBehaviour
{
    [SerializeField] private string bossID = "Boss_001";

    private void OnTriggerEnter(Collider other)
    {
        AIBossCharacterManager bossCharacter = WorldAIManager.instance.GetBossCharacterByID(bossID);
        if (bossCharacter != null)
        {
            bossCharacter.AwakenBoss();
        }
    }
}
