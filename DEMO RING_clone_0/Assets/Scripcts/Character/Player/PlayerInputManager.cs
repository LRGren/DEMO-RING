using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
    [HideInInspector] public PlayerManager player;
    
    //从玩家输入中获取数据
    //按照数据移动

    private PlayerControls playerControls;
    
    [Header("Camera Movement Inputs")]
    [SerializeField] private Vector2 cameraInput;
    public float cameraVerticalInput;
    public float cameraHorizontalInput;

    [Header("Lock On Inputs")]
    [SerializeField] private bool lock_On_Input = false;
    [SerializeField] private bool lockOn_Left_Input = false;
    [SerializeField] private bool lockOn_Right_Input = false;
    private Coroutine lockOnCoroutine;

    [Header("Player Movement Inputs")]
    [SerializeField] private Vector2 movementInput;
    public float verticalInput;
    public float horizontalInput;
    public float moveAmount;
    
    [Header("Player Dodge Inputs")]
    [SerializeField] private bool dodge_Input = false;
    [SerializeField] private bool sprint_Input = false;
    [SerializeField] private bool jump_Input = false;

    [Header("Bumper Inputs")]
    [SerializeField] private bool RB_Input = false;

    [Header("Trigger Inputs")]
    [SerializeField] private bool RT_Input = false;
    [SerializeField] private bool Hold_RT_Input = false;

    [Header("D-pad Inputs")]
    [SerializeField]private bool switch_Right_Weapons_Input = false;
    [SerializeField]private bool switch_Leftt_Weapons_Input = false;


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

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneChange;
        
        instance.enabled = false;

        if(playerControls != null)
        {
            playerControls.Disable();
        }
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        //加载场景的时候应该启用player controls
        if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
        {
            instance.enabled = true;
            if(playerControls != null)
            {
                playerControls.Enable();
            }
        }
        //否则应该实在菜单界面，应该禁用
        else
        {
            instance.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            //Camera Movement
            playerControls.PlayerCamera.Movement.performed += i => cameraInput = i.ReadValue<Vector2>();

            //Base Movement
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true;
            playerControls.PlayerActions.Jump.performed += i => jump_Input = true;

            playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false;

            //Bumpers
            playerControls.PlayerActions.RB.performed += i => RB_Input = true;

            //Triggers
            playerControls.PlayerActions.RT.started += i => RT_Input = true;
            playerControls.PlayerActions.HoldRT.performed += i => Hold_RT_Input = true;
            playerControls.PlayerActions.HoldRT.canceled += i => Hold_RT_Input = false;

            //Lock On
            playerControls.PlayerActions.LockOn.performed += i => lock_On_Input = true;
            playerControls.PlayerActions.SeekLeftLockOnTarget.performed += i => lockOn_Left_Input = true;
            playerControls.PlayerActions.SeekRightLockOnTarget.performed += i => lockOn_Right_Input = true;

            //D-Pad
            playerControls.PlayerActions.SwitchRightWeapon.performed += i => switch_Right_Weapons_Input = true;
            playerControls.PlayerActions.SwitchLeftWeapon.performed += i => switch_Leftt_Weapons_Input = true;
        }

        playerControls.Enable();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    public void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            playerControls.Enable();
        }
        else
        {
            playerControls.Disable();
        }
    }
    
    private void Update()
    {
        HandleAllInputs();
    }

    private void HandleAllInputs()
    {
        if(player == null)
            return;

        HandlePlayerMovementInput();
        HandleCameraMovementInput();

        HandleLockOnInput();
        HandleLockOnSwitchTargetInput();

        HandleDodgeInput();
        HandleSprintInput();
        HandleJumpInput();

        HandleRBInput();
        HandleRTInput();
        HandleHoldRTInput();

        HandleSwitchRightWeaponsInput();
        HandleSwitchLeftWeaponsInput();
    }

    private void HandleLockOnInput()
    {
        //时刻检测锁定状态，如果有目标死亡，取消锁定
        if (player.playerNetworkManager.isLockOn.Value)
        {
            //如果没有目标，取消锁定
            if (player.playerCombatManager.currentTarget == null)
                return;

            if (player.playerCombatManager.currentTarget.isDead.Value)
            {
                player.playerNetworkManager.isLockOn.Value = false;

                //尝试寻找新的目标
                if (lockOnCoroutine != null)
                    StopCoroutine(lockOnCoroutine);

                lockOnCoroutine = StartCoroutine(PlayerCamera.instance.WaitThenFindNewTarget());
            }

        }

        if (lock_On_Input)
        {
            lock_On_Input = false;

            if (player.playerNetworkManager.isLockOn.Value)
            {

                //如果有目标，取消锁定
                PlayerCamera.instance.ClearLockOnTargets();

                player.playerNetworkManager.isLockOn.Value = false;
            }
            else
            {
                //如果使用远程武器，不需要锁定

                //如果没有目标，尝试锁定
                PlayerCamera.instance.HandleLocatingLockOnTargets();

                if(PlayerCamera.instance.nearestLockOnTarget != null)
                {
                    player.playerCombatManager.SetTarget(PlayerCamera.instance.nearestLockOnTarget);

                    player.playerNetworkManager.isLockOn.Value = true;
                }

            }
        }
    }
    
    private void HandleLockOnSwitchTargetInput()
    {
        if (lockOn_Left_Input)
        {
            lockOn_Left_Input = false;

            if(player.playerNetworkManager.isLockOn.Value)
            {
                //Debug.Log("Switching Left Lock On Target");
                PlayerCamera.instance.HandleLocatingLockOnTargets();
                if (PlayerCamera.instance.leftLockOnTarget != null)
                {
                    player.playerCombatManager.SetTarget(PlayerCamera.instance.leftLockOnTarget);
                }
            }
        }

        if(lockOn_Right_Input)
        {
            lockOn_Right_Input = false;

            if(player.playerNetworkManager.isLockOn.Value)
            {
                //Debug.Log("Switching Right Lock On Target");

                PlayerCamera.instance.HandleLocatingLockOnTargets();

                if (PlayerCamera.instance.rightLockOnTarget != null)
                {
                    player.playerCombatManager.SetTarget(PlayerCamera.instance.rightLockOnTarget);
                }
            }
        }
    }

    private void HandlePlayerMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
        
        moveAmount = Mathf.Clamp01
            (Mathf.Abs(verticalInput)+Mathf.Abs(horizontalInput));

        if (moveAmount > 0 && moveAmount <= 0.5f)
        {
            moveAmount = 0.5f;
        }
        else if (moveAmount <= 1f && moveAmount > 0.5f)
        {
            moveAmount = 1f;
        }
        
        if(player == null)
            return;

        if (moveAmount != 0)
        {
            player.playerNetworkManager.isMoving.Value = true;

        }
        else
        {
            player.playerNetworkManager.isMoving.Value = false;
        }

        if(!player.playerNetworkManager.isLockOn.Value || player.playerNetworkManager.isSprinting.Value)
        {
            //未锁定时只需要前进的动作 或者疾跑
            player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
        }
        else
        {
            player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontalInput, verticalInput, player.playerNetworkManager.isSprinting.Value);
        }
    }

    private void HandleCameraMovementInput()
    {
        cameraVerticalInput = cameraInput.y/* + cameraMouseInput.y * 0.2f*/;
        cameraHorizontalInput = cameraInput.x/* + cameraMouseInput.x * 0.2f*/;
    }

    private void HandleDodgeInput()
    {
        if (dodge_Input)
        {
            dodge_Input = false;
            
            //后跳或者翻滚
            player.playerLocomotionManager.AttemptToPerformDodge();
        }
    }

    private void HandleSprintInput()
    {
        if (sprint_Input)
        {
            player.playerLocomotionManager.HandleSprinting();
        }
        else
        {
            player.playerNetworkManager.isSprinting.Value = false;
        }
    }

    private void HandleJumpInput()
    {
        if (jump_Input)
        {
            jump_Input = false;
            
            //有UI，不反应
            
            //尝试跳跃
            player.playerLocomotionManager.AttemptToPerformJump();
        }
    }

    private void HandleRBInput()
    {
        if(RB_Input)
        {
            RB_Input = false;

            //如果有UI，不反应

            player.playerNetworkManager.SetCharacterActionHand(true);

            player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RB_Action, player.playerInventoryManager.currentRightHandWeapon);
        }

    }

    private void HandleRTInput()
    {
        if (RT_Input)
        {
            RT_Input = false;
            //如果有UI，不反应

            player.playerNetworkManager.SetCharacterActionHand(true);

            player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RT_Action, player.playerInventoryManager.currentRightHandWeapon);
        }

    }

    private void HandleHoldRTInput()
    {
        if (player.isPerformingAction)
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerNetworkManager.isChargingAttack.Value = Hold_RT_Input;
            }
        }
    }

    private void HandleSwitchRightWeaponsInput()
    {
        if (switch_Right_Weapons_Input)
        {
            switch_Right_Weapons_Input = false;
            player.playerEquipmentManager.SwitchRightWeapon();
        }
    }
    private void HandleSwitchLeftWeaponsInput()
    {
        if (switch_Leftt_Weapons_Input)
        {
            switch_Leftt_Weapons_Input = false;
            player.playerEquipmentManager.SwitchLeftWeapon();
        }
    }

}
