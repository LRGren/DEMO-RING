using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class WorldAIManager : MonoBehaviour
{
    public static WorldAIManager instance;

    [Header("Debug")]
    public bool respawn = false;
    public bool despawn = false;

    [Header("Characters")]
    public GameObject[] aiCharacters;
    public List<GameObject> spawnedInCharacters;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            // Server-specific initialization
            StartCoroutine(WaitForSceneToLoadThenSpawnCharacters());
        }
    }

    private void Update()
    {
        //Debug
        if(respawn)
        {
            respawn = false;
            if(spawnedInCharacters.Count > 0)
            {
                DespawnAllCharacters();
            }
            SpawnAllCharacters();
        }

        if (despawn)
        {
            despawn = false;
            DespawnAllCharacters();
        }
    }

    private IEnumerator WaitForSceneToLoadThenSpawnCharacters()
    {
        //如果不是在加载场景，直接返回
        if (!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        SpawnAllCharacters();
    }

    private void SpawnAllCharacters()
    {
        foreach (var character in aiCharacters) { 
            GameObject characterInstance = Instantiate(character);
            characterInstance.GetComponent<NetworkObject>().Spawn();
            spawnedInCharacters.Add(characterInstance);
        }
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
