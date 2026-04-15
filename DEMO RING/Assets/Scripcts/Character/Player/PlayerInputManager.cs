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
    [SerializeField] private Vector2 cameraMouseInput;
    public float cameraVerticalInput;
    public float cameraHorizontalInput;

    [Header("Lock On Inputs")]
    [SerializeField] private bool lock_On_Input = false;

    [Header("Player Movement Inputs")]
    [SerializeField] private Vector2 movementInput;
    public float verticalInput;
    public float horizontalInput;
    public float moveAmount;
    
    [Header("Player Dodge Inputs")]
    [SerializeField] private bool dodge_Input = false;
    [SerializeField] private bool sprint_Input = false;
    [SerializeField] private bool jump_Input = false;
    [SerializeField] private bool RB_Input = false;

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
            
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            playerControls.PlayerCamera.Movement.performed += i => cameraInput = i.ReadValue<Vector2>();
            //playerControls.PlayerCamera.Mouse.performed += i => cameraMouseInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true;
            playerControls.PlayerActions.Jump.performed += i => jump_Input = true;
            playerControls.PlayerActions.LockOn.performed += i => lock_On_Input = true;
            
            playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false;

            playerControls.PlayerActions.RB.performed += i => RB_Input = true;

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
        HandleDodgeInput();
        HandleSprintInput();
        HandleJumpInput();
        HandleRBInput();
        HandleLockOnInput();
    }

    private void HandleLockOnInput()
    {
        //时刻检测锁定状态，如果有目标死亡，取消锁定
        if (player.playerNetworkManager.isLockOn.Value)
        {
            //如果没有目标，取消锁定
            if (player.playerCombatManager.currentTarget == null)
                return ;

            if(player.playerCombatManager.currentTarget.isDead.Value){
                player.playerNetworkManager.isLockOn.Value = false;
            }
        }

        if(lock_On_Input)
        {
            lock_On_Input = false;

            if (player.playerNetworkManager.isLockOn.Value)
            {

                //如果有目标，取消锁定

            }
            else
            {

                //如果使用远程武器，不需要锁定

                //如果没有目标，尝试锁定
                PlayerCamera.instance.HandleLocatingLockOnTargets();

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
        //未锁定时只需要前进的动作
        player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount,
            player.playerNetworkManager.isSprinting.Value);
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

}
