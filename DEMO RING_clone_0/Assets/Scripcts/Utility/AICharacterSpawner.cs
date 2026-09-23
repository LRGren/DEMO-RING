using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AICharacterSpawner : MonoBehaviour
{
    private AICharacterManager aiCharacrer;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject aiCharacterPrefab;
    [SerializeField] private GameObject spawnedAICharacter;

    void Start()
    {
        WorldAIManager.instance.SpawnCharacter(this);
        gameObject.SetActive(false);
    }

    public void AttemptToSpawnAICharacter()
    {
        if (spawnedAICharacter == null)
        {
            spawnedAICharacter = Instantiate(aiCharacterPrefab);
            spawnedAICharacter.transform.position = transform.position;
            spawnedAICharacter.transform.rotation = transform.rotation;

            spawnedAICharacter.GetComponent<NetworkObject>().Spawn();
            aiCharacrer = spawnedAICharacter.GetComponent<AICharacterManager>();

            if (aiCharacrer != null)
                WorldAIManager.instance.AddSpawnedCharacter(spawnedAICharacter.GetComponent<AICharacterManager>());
        }
    }

    public void ResetSpawnedCharacter()
    {
        if (spawnedAICharacter == null)
            return;

        if (aiCharacrer == null)
            return;

        spawnedAICharacter.transform.position = transform.position;
        spawnedAICharacter.transform.rotation = transform.rotation;
        aiCharacrer.aiCharacterNetworkManager.currentHealth = aiCharacrer.aiCharacterNetworkManager.maxHealth;
        aiCharacrer.currentState = aiCharacrer.idle;

        if (aiCharacrer.isDead.Value)
        {
            aiCharacrer.isDead.Value = false;
            aiCharacrer.characterAnimatorManager.PlayerTargetActionAnimation("Empty", false, false, true, true, true, true);
        }

        aiCharacrer.characterUIManager.ResetCharacterHPBar();
    }
}
