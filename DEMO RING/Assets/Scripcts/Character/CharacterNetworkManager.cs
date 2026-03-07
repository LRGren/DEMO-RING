using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterNetworkManager : NetworkBehaviour
{
    private CharacterManager character;
    
    [Header("Position")] 
    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Vector3 networkPositionVelocity;
    public float networkPositionSmoothTime = 0.1f;
    public float networkRotationSmoothTime = 0.1f;

    [Header("Animator")]
    public NetworkVariable<float> horizontalMovement = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> verticalMovement = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> moveAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Flags")]
    public NetworkVariable<bool> isSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [Header("Stats")]
    public NetworkVariable<int> endurance = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> vitality = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [Header("Resources")]
    public NetworkVariable<float> currentStamina = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public void CheckHP(int oldValue, int newValue)
    {
        if (currentHealth.Value <= 0)
        {
            StartCoroutine(character.ProcessDeathEvent());
        }

        if (currentHealth.Value >= maxHealth.Value)
        {
            currentHealth.Value = maxHealth.Value;
        }
    }
    
    [ServerRpc]
    public void NotifyTheServerOfActionAnimationServerRpc(ulong clientId,string animationName,bool applyRootMotion)
    {
        if (IsServer)
        {
            PlayActionAnimationForAllClientsClientRpc(clientId, animationName, applyRootMotion);
        }
    }

    [ClientRpc]
    public void PlayActionAnimationForAllClientsClientRpc(ulong clientId,string animationName,bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PerformActionAnimationFromServer(animationName, applyRootMotion);
        }
    }

    public void PerformActionAnimationFromServer(string animationName, bool applyRootMotion)
    {
        character.applyRootMotion = applyRootMotion;
        character.animator.CrossFade(animationName, 0.2f);
    }
}
