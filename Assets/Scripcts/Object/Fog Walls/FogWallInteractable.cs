using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FogWallInteractable : Interactable
{
    public string bossID = "Boss_001";

    [Header("Sound FX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip fogWallSoundFX;

    [Header("Fog Walls")]
    [SerializeField] private GameObject[] fogWalls;

    [Header("Collision")]
    [SerializeField] private Collider fogWallColliders;

    [Header("Active")]
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected override void Awake()
    {
        base.Awake();

        audioSource = GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);

        //1.面向雾门（垂直于雾门平面的方向）
        Vector3 targetDirection = transform.forward;

        if (Vector3.Dot(player.transform.position - transform.position, transform.forward) > 0)
            targetDirection = -transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        player.transform.rotation = targetRotation;

        //2.关闭雾门碰撞
        AllowPlayerToPassThroughFogWallServerRpc(player.NetworkObjectId);

        //3.播放前进动画
        player.characterAnimatorManager.PlayerTargetActionAnimation("Pass_Through_Fog_Wall_01", true);

        //4.重新开启雾门碰撞
    }

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
        interactableCollider.enabled = isActive.Value;

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

    [ServerRpc(RequireOwnership = false)]
    public void AllowPlayerToPassThroughFogWallServerRpc(ulong clientId)
    {
        AllowPlayerToPassThroughFogWallClientRpc(clientId);
    }

    [ClientRpc]
    public void AllowPlayerToPassThroughFogWallClientRpc(ulong clientId)
    {
        PlayerManager player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientId].GetComponent<PlayerManager>();

        audioSource.PlayOneShot(fogWallSoundFX);

        if (player != null)
        {
            StartCoroutine(AllowPlayerToPassThroughFogWall(player));
        }
    }

    private IEnumerator AllowPlayerToPassThroughFogWall(PlayerManager player)
    {
        Physics.IgnoreCollision(player.characterController, fogWallColliders, true);
        yield return new WaitForSeconds(2f);
        Physics.IgnoreCollision(player.characterController, fogWallColliders, false);
    }
}
