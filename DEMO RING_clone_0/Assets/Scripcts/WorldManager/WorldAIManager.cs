using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class WorldAIManager : MonoBehaviour
{
    public static WorldAIManager instance;

    [Header("Characters")]
    [SerializeField] private List<AICharacterSpawner> aiCharacterSpawners = new List<AICharacterSpawner>();
    public List<GameObject> spawnedInCharacters;

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

    public void SpawnAllCharacters(AICharacterSpawner spawner)
    {
        aiCharacterSpawners.Add(spawner);
        spawner.AttemptToSpawnAICharacter();
    }

    private void DespawnAllCharacters()
    {
        foreach (var character in spawnedInCharacters)
        {
            if (character != null)
            {
                character.GetComponent<NetworkObject>().Despawn();
                Destroy(character);
            }
        }
        spawnedInCharacters.Clear();
    }


    private void DisableAllCharacters()
    {

    }

}
