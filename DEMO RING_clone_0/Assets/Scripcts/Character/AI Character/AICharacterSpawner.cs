using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AICharacterSpawner : MonoBehaviour
{
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
        }
    }
}
