using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBossCharacterManager : AICharacterManager
{
    //分配唯一的ID
    [Header("Boss ID")]
    public string bossID = "Boss_001";

    [SerializeField] private List<FogWallInteractable> fogWalls;

    [Space(10)]
    [SerializeField] private bool hasBeenDefeated = false;
    [SerializeField] private bool hasBeenAwakened = false;


    //[Header("Test")]
    //[SerializeField] private bool testDefeated = false;
    //当Boss生成，检查Save File（A List）
    //如果没有这个ID，就分配一个新的ID
    //如果有这个ID，检查,是否被唤醒，是否被击败过
    //如果被击败过，就disable掉这个Boss的生成
    //如果没有被击败过，就保持active状态


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            //检查是否有这个ID
            if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
            {
                //如果没有这个ID，就分配一个新的ID
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, false);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, false);
            }
            else
            {
                hasBeenDefeated = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated[bossID];
                hasBeenAwakened = WorldSaveGameManager.instance.currentCharacterData.bossesAwakened[bossID];
            }

            StartCoroutine(GetFogWallsFromWorldObjectManager());

            if (hasBeenAwakened)
            {
                foreach (var fogWall in fogWalls)
                {
                    fogWall.isActive.Value = true;
                }
            }

            if (hasBeenDefeated)
            {
                foreach (var fogWall in fogWalls)
                {
                    fogWall.isActive.Value = false;
                }
                aiCharacterNetworkManager.isActive.Value = false;
            }
        }
    }

    private IEnumerator GetFogWallsFromWorldObjectManager()
    {
        fogWalls = new List<FogWallInteractable>();

        while (fogWalls.Count == 0)
        {
            foreach (var fogWall in WorldObjectManager.instance.fogWalls)
            {
                if (fogWall.objectID == bossID)
                {
                    fogWalls.Add(fogWall);
                }
            }

            if (fogWalls.Count == 0)
                yield return new WaitForEndOfFrame();
        }
    }

    public override IEnumerator ProcessDeathEvent(bool manuallySelectedDeathAnimation = false)
    {
        if (IsOwner)
        {
            characterNetworkManager.currentHealth.Value = 0;
            isDead.Value = true;

            //重置所有FLAG

            //如果在空中，选择播放其他动画

            if (!manuallySelectedDeathAnimation)
            {
                characterAnimatorManager.PlayerTargetActionAnimation("Death_01", true);
            }


            hasBeenDefeated = true;
            if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
            {
                //如果没有这个ID，就分配一个新的ID
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, true);
            }
            else
            {
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Remove(bossID);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Remove(bossID);

                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, true);
            }

            WorldSaveGameManager.instance.SaveGame();
        }

        yield return new WaitForSeconds(5);

        //虚化

        //消失
    }
}
