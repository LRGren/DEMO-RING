using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    
    [Header("Player Movement Inputs")]
    [SerializeField] private Vector2 movementInput;
    public float verticalInput;
    public float horizontalInput;
    public float moveAmount;
    
    [Header("Player Dodge Inputs")]
    [SerializeField] private bool dodgeInput = false;
    [SerializeField] private bool sprintInput = false;
    [SerializeField] private bool jumpInput = false;
    
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
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        //加载场景的时候应该启用player controls
        if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
        {
            instance.enabled = true;
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
            playerControls.PlayerCamera.Mouse.performed += i => cameraMouseInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.Dodge.performed += i => dodgeInput = true;
            
            playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
            
            playerControls.PlayerActions.Sprint.performed += i => sprintInput = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprintInput = false;
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
        HandlePlayerMovementInput();
        HandleCameraMovementInput();
        HandleDodgeInput();
        HandleSprintInput();
        HandleJumpInput();
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
        cameraVerticalInput = cameraInput.y + cameraMouseInput.y * 0.2f;
        cameraHorizontalInput = cameraInput.x + cameraMouseInput.x * 0.2f;
    }

    private void HandleDodgeInput()
    {
        if (dodgeInput)
        {
            dodgeInput = false;
            
            //后跳或者翻滚
            player.playerLocomotionManager.AttemptToPerformDodge();
        }
    }

    private void HandleSprintInput()
    {
        if (sprintInput)
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
        if (jumpInput)
        {
            jumpInput = false;
            
            //有UI，不反应
            
            //尝试跳跃
            player.playerLocomotionManager.AttemptToPerformJump();
        }
    }
}
