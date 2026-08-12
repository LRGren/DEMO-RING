using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AIBossCharacterManager : AICharacterManager
{
    //分配唯一的ID
    [Header("Boss ID")]
    public string bossID = "Boss_001";

    [SerializeField] private List<FogWallInteractable> fogWalls;

    [Space(10)]
    [Header("Boss Status")]
    public NetworkVariable<bool> bossFightIsActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> hasBeenAwakened = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> hasBeenDefeated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private string sleepAnimation = "Sleep_01";
    [SerializeField] private string awakenAnimation = "Awaken_01";

    [Header("State")]
    [SerializeField] private BossSleepState bossSleepState;

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

        bossFightIsActive.OnValueChanged += OnBossFightIsActiveChanged;
        OnBossFightIsActiveChanged(false, bossFightIsActive.Value);

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
                hasBeenDefeated.Value = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated[bossID];
                hasBeenAwakened.Value = WorldSaveGameManager.instance.currentCharacterData.bossesAwakened[bossID];
            }

            StartCoroutine(GetFogWallsFromWorldObjectManager());

            if (hasBeenAwakened.Value)
            {
                foreach (var fogWall in fogWalls)
                {
                    fogWall.isActive.Value = true;
                }
            }
            else
            {
                foreach (var fogWall in fogWalls)
                {
                    fogWall.isActive.Value = false;
                }
            }

            if (hasBeenDefeated.Value)
            {
                foreach (var fogWall in fogWalls)
                {
                    fogWall.isActive.Value = false;
                }
                aiCharacterNetworkManager.isActive.Value = false;
            }
        }

        if (IsOwner)
        {
            bossSleepState = Instantiate(bossSleepState);

            currentState = bossSleepState;
        }

        if (!hasBeenAwakened.Value)
        {
            characterAnimatorManager.PlayerTargetActionAnimation(sleepAnimation, true);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        bossFightIsActive.OnValueChanged -= OnBossFightIsActiveChanged;
    }

    public void OnBossFightIsActiveChanged(bool oldValue, bool newValue)
    {
        if (!bossFightIsActive.Value)
            return;

        GameObject bossHPBar =
        Instantiate(PlayerUIManager.instance.playerUIHudManager.bossHPBarObject,
        PlayerUIManager.instance.playerUIHudManager.bossHPBarParent);

        bossHPBar.GetComponentInChildren<UI_Boss_HP_Bar>().EnableBossHPBar(this);
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

            bossFightIsActive.Value = false;

            //重置所有FLAG

            //如果在空中，选择播放其他动画

            if (!manuallySelectedDeathAnimation)
            {
                characterAnimatorManager.PlayerTargetActionAnimation("Death_01", true);
            }


            hasBeenDefeated.Value = true;
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

    public void AwakenBoss()
    {
        if (IsOwner)
        {
            if (!hasBeenAwakened.Value)
            {
                characterAnimatorManager.PlayerTargetActionAnimation(awakenAnimation, true);
            }

            bossFightIsActive.Value = true;
            hasBeenAwakened.Value = true;
            currentState = idle;

            if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
            {
                //如果没有这个ID，就分配一个新的ID
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, false);
            }
            else
            {
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Remove(bossID);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Remove(bossID);

                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, false);
            }

            foreach (var fogWall in fogWalls)
            {
                fogWall.isActive.Value = true;
            }
        }
    }
}
