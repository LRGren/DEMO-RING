using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBossCharacterManager : AICharacterManager
{
    //分配唯一的ID
    [Header("Boss ID")]
    public string bossID = "Boss_001";
    //当Boss生成，检查Save File（A List）
    //如果没有这个ID，就分配一个新的ID
    //如果有这个ID，检查是否被击败过
    //如果被击败过，就disable掉这个Boss的生成
    //如果没有被击败过，就保持active状态
    void OnEnable()
    {
        Test01();
        if (WorldSaveGameManager.instance.currentCharacterData.bosses.Contains(bossID))
        {
            if (WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Contains(bossID))
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            WorldSaveGameManager.instance.currentCharacterData.bosses.Add(bossID);
        }
    }

    public void Test01()
    {
        if (WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Contains(bossID))
        {
            if (WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Contains(bossID))
            {
                Debug.Log("Boss has been defeated before.");
            }
            else
            {
                Debug.Log("Boss has not been defeated yet.");
            }
        }
        else
        {
            Debug.Log("Boss ID not found in the save data.");
        }
    }

    public void Test02()
    {
        WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID);
        Debug.Log("Boss marked as defeated.");
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.K))
        {
            Test02();
        }
    }
}
