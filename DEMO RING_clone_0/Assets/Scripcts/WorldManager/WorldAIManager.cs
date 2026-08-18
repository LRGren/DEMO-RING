using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class WorldAIManager : MonoBehaviour
{
    public static WorldAIManager instance;

    [Header("Characters")]
    [SerializeField] private List<AICharacterSpawner> aiCharacterSpawners;
    public List<AICharacterManager> spawnedInCharacters;
    public List<AIBossCharacterManager> spawnedInBossCharacters;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnCharacter(AICharacterSpawner spawner)
    {
        aiCharacterSpawners.Add(spawner);
        spawner.AttemptToSpawnAICharacter();
    }

    public void AddSpawnedCharacter(AICharacterManager character)
    {
        if (!spawnedInCharacters.Contains(character))
        {
            spawnedInCharacters.Add(character);
        }

        AIBossCharacterManager bossCharacter = character as AIBossCharacterManager;
        if (bossCharacter != null && !spawnedInBossCharacters.Contains(bossCharacter))
        {
            spawnedInBossCharacters.Add(bossCharacter);
        }
    }

    public AIBossCharacterManager GetBossCharacterByID(string bossID)
    {
        return spawnedInBossCharacters.FirstOrDefault(boss => boss.bossID == bossID);
    }

    public void RestAllCharacters()
    {
        DespawnAllCharacters();

        foreach (var spawner in aiCharacterSpawners)
        {
            spawner.AttemptToSpawnAICharacter();
        }
    }

    private void DespawnAllCharacters()
    {
        foreach (var character in spawnedInCharacters)
        {
            if (character != null)
            {
                character.GetComponent<NetworkObject>().Despawn();
            }
        }

        spawnedInCharacters.Clear();

        foreach (var spawner in aiCharacterSpawners)
        {
            spawner.ResetSpawnedCharacter();
        }
    }


    private void DisableAllCharacters()
    {

    }

}
