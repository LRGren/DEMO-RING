using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Boss_HP_Bar : UI_StatBar
{
    [SerializeField] private AIBossCharacterManager bossCharacterManager;

    public void EnableBossHPBar(AIBossCharacterManager boss)
    {
        bossCharacterManager = boss;

        bossCharacterManager.aiCharacterNetworkManager.currentHealth.OnValueChanged += OnBossHPChanged;

        SetMaxStat(bossCharacterManager.aiCharacterNetworkManager.maxHealth.Value);
        SetStat(bossCharacterManager.aiCharacterNetworkManager.currentHealth.Value);
        GetComponentInChildren<TextMeshProUGUI>().text = bossCharacterManager.characterName;
    }

    void OnDestroy()
    {
        if (bossCharacterManager != null)
        {
            bossCharacterManager.aiCharacterNetworkManager.currentHealth.OnValueChanged -= OnBossHPChanged;
        }
    }

    private void OnBossHPChanged(int oldHP, int newHP)
    {
        SetStat(newHP);

        if (newHP <= 0)
        {
            RemoveHPBar(2.5f);
        }
    }

    public void RemoveHPBar(float time)
    {
        Destroy(gameObject, time);
    }
}
