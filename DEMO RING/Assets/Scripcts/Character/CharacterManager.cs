using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    [Header("Status")]
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public CharacterNetworkManager characterNetworkManager;
    [HideInInspector] public Animator animator;
    [HideInInspector] public CharacterEffectsManager characterEffectsManager;
    [HideInInspector] public CharacterAnimatorManager characterAnimatorManager;
    [HideInInspector] public CharacterStatsManager characterStatsManager;
    [HideInInspector] public CharacterCombatManager characterCombatManager;
    [HideInInspector] public CharacterSoundFXManager characterSoundFXManager;
    [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
    [HideInInspector] public CharacterUIManager characterUIManager;

    [Header("Character Group")]
    public CharacterGroup characterGroup;

    [Header("Flags")]
    public bool isPerformingAction = false;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);

        characterController = GetComponent<CharacterController>();
        characterNetworkManager = GetComponent<CharacterNetworkManager>();
        animator = GetComponent<Animator>();
        characterEffectsManager = GetComponent<CharacterEffectsManager>();
        characterStatsManager = GetComponent<CharacterStatsManager>();
        characterAnimatorManager = GetComponent<CharacterAnimatorManager>();
        characterCombatManager = GetComponent<CharacterCombatManager>();
        characterSoundFXManager = GetComponent<CharacterSoundFXManager>();
        characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
        characterUIManager = GetComponent<CharacterUIManager>();
    }

    protected virtual void Start()
    {
        IgnoreMyOwnColliders();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        animator.SetBool("isMoving", characterNetworkManager.isMoving.Value);
        characterNetworkManager.OnIsActiveChanged(false, characterNetworkManager.isActive.Value);

        characterNetworkManager.isMoving.OnValueChanged += characterNetworkManager.OnIsMovingChanged;
        characterNetworkManager.isActive.OnValueChanged += characterNetworkManager.OnIsActiveChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        characterNetworkManager.isMoving.OnValueChanged -= characterNetworkManager.OnIsMovingChanged;
        characterNetworkManager.isActive.OnValueChanged -= characterNetworkManager.OnIsActiveChanged;
    }

    protected virtual void Update()
    {
        animator.SetBool("isGrounded", characterLocomotionManager.isGrounded);

        if (IsOwner)
        {
            //如果该角色由本地控制，那么实时更改位置，并且写入网络变量
            characterNetworkManager.networkPosition.Value = transform.position;

            characterNetworkManager.networkRotation.Value = transform.rotation;
        }
        else
        {
            //如果不是本地控制，那么应该根据主机的位置进行移动
            transform.position = Vector3.SmoothDamp(transform.position, characterNetworkManager.networkPosition.Value,
                ref characterNetworkManager.networkPositionVelocity, characterNetworkManager.networkPositionSmoothTime);

            transform.rotation = Quaternion.Slerp(transform.rotation, characterNetworkManager.networkRotation.Value,
                characterNetworkManager.networkRotationSmoothTime);
        }
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    public virtual IEnumerator ProcessDeathEvent(bool manuallySelectedDeathAnimation = false)
    {
        if (IsOwner)
        {
            characterNetworkManager.currentHealth.Value = 0;
            isDead.Value = true;

            //重置所有FLAG

            //如果在空中，选择播放其他动画

            if (!manuallySelectedDeathAnimation)
            {
                characterAnimatorManager.PlayerTargetActionAnimation("Death_01", true);
            }
        }

        yield return new WaitForSeconds(5);

        //虚化

        //消失
    }

    public virtual void ReviveCharacter()
    {

    }

    protected virtual void IgnoreMyOwnColliders()
    {
        Collider characterCollider = GetComponent<Collider>();
        Collider[] damageableColliders = GetComponentsInChildren<Collider>();
        List<Collider> ignoreColliders = new List<Collider>();

        foreach (var collider in damageableColliders)
        {
            ignoreColliders.Add(collider);
        }

        ignoreColliders.Add(characterCollider);

        foreach (var collider in ignoreColliders)
        {
            foreach (var otherCollider in ignoreColliders)
            {
                Physics.IgnoreCollision(collider, otherCollider, true);
            }
        }
    }

    public void SetIsPerformingActionFalse()
    {
        isPerformingAction = false;
    }
}
