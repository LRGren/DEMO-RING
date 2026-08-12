using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FogWallInteractable : Object
{
    [Header("Fog Walls")]
    [SerializeField] private GameObject[] fogWalls;

    [Header("Active")]
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        OnIsActiveChanged(false, isActive.Value);
        isActive.OnValueChanged += OnIsActiveChanged;

        WorldObjectManager.instance.AddFogWallToList(this);
    }

    public override void OnNetworkDespawn()
    {
        isActive.OnValueChanged -= OnIsActiveChanged;

        WorldObjectManager.instance.RemoveFogWallFromList(this);
    }

    private void OnIsActiveChanged(bool previousValue, bool newValue)
    {
        if (isActive.Value)
        {
            foreach (GameObject fogWall in fogWalls)
            {
                fogWall.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject fogWall in fogWalls)
            {
                fogWall.SetActive(false);
            }
        }
    }
}
