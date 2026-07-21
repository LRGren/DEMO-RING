using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjectSpawner : MonoBehaviour
{
    [Header("Object I.D")]
    public string objectID;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject networkObjectPrefab;
    [SerializeField] private GameObject spawnedNetworkObject;

    void Start()
    {
        WorldObjectManager.instance.SpawnObject(this);
        gameObject.SetActive(false);
    }

    public void AttemptToSpawnNetworkObject()
    {
        if (spawnedNetworkObject == null)
        {
            spawnedNetworkObject = Instantiate(networkObjectPrefab);
            spawnedNetworkObject.transform.position = transform.position;
            spawnedNetworkObject.transform.rotation = transform.rotation;

            spawnedNetworkObject.GetComponent<Object>().objectID = objectID;
            spawnedNetworkObject.GetComponent<NetworkObject>().Spawn();
        }
    }
}
