using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class WorldAIManager : MonoBehaviour
{
    public static WorldAIManager instance;

    [Header("Spawning")]
    public bool isPerformingLoadingScreenSpawn = false;
    private Coroutine spawnCoroutine;
    private Coroutine despawnCoroutine;
    private Coroutine resetCoroutine;

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
        if (bossCharacter != null)
        {
            if (!spawnedInBossCharacters.Contains(bossCharacter))
            {
                spawnedInBossCharacters.Add(bossCharacter);
            }
        }
    }

    public AIBossCharacterManager GetBossCharacterByID(string bossID)
    {
        return spawnedInBossCharacters.FirstOrDefault(boss => boss.bossID == bossID);
    }

    public void SpawnAllCharacters()
    {
        isPerformingLoadingScreenSpawn = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnAllCharactersCoroutine());
    }

    private IEnumerator SpawnAllCharactersCoroutine()
    {
        for (int i = 0; i < aiCharacterSpawners.Count; i++)
        {
            AICharacterSpawner spawner = aiCharacterSpawners[i];
            if (spawner != null)
            {
                yield return new WaitForFixedUpdate(); // Wait for the next fixed update before spawning the next character
                spawner.AttemptToSpawnAICharacter();
                yield return null;
            }
        }

        isPerformingLoadingScreenSpawn = false;

        yield return null;
    }

    public void ForceAllAIToReevaluateTargets()
    {
        foreach (var character in spawnedInCharacters)
        {
            if (character == null || character.isDead.Value)
                continue;

            character.aiCharacterCombatManager.currentTarget = null;
            character.currentState = character.idle;
        }
    }

    public void DespawnAllCharacters()
    {
        isPerformingLoadingScreenSpawn = true;

        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }

        despawnCoroutine = StartCoroutine(DespawnAllCharactersCoroutine());
    }

    private IEnumerator DespawnAllCharactersCoroutine()
    {
        foreach (var character in spawnedInCharacters)
        {
            if (character != null)
            {
                yield return new WaitForFixedUpdate(); // Wait for the next fixed update before despawning the next character
                character.GetComponent<NetworkObject>().Despawn();
                yield return null; // Wait for the next frame before despawning the next character
            }
        }

        spawnedInCharacters.Clear();
        spawnedInBossCharacters.Clear();

        yield return null;
    }

    public void DisableAllCharacters()
    {

    }

    public void ResetAllCharacters()
    {
        isPerformingLoadingScreenSpawn = true;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        resetCoroutine = StartCoroutine(ResetAllCharactersCoroutine());
    }

    private IEnumerator ResetAllCharactersCoroutine()
    {
        isPerformingLoadingScreenSpawn = true;

        for (int i = 0; i < aiCharacterSpawners.Count; i++)
        {
            AICharacterSpawner spawner = aiCharacterSpawners[i];
            if (spawner != null)
            {
                yield return new WaitForFixedUpdate(); // Wait for the next fixed update before spawning the next character
                spawner.ResetSpawnedCharacter();
                yield return null;
            }
        }

        isPerformingLoadingScreenSpawn = false;

        yield return null;
    }

}
