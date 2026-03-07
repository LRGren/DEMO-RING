using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    private PlayerManager player;
    [HideInInspector] public float verticalMovement;
    [HideInInspector] public float horizontalMovement;
    [HideInInspector] public float moveAmount;
    
    [Header("Movement")]
    private Vector3 moveDirection;
    private Vector3 targetRotationDirection;
    [SerializeField] private float walkingSpeed = 1.5f;
    [SerializeField] private float runningSpeed = 4.5f;
    [SerializeField] private float sprintingSpeed = 7f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float sprintingCost = 2f;

    [Header("Jump")]
    private Vector3 jumpDirection;
    [SerializeField] private float jumpStaminaCost = 10;
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float jumpingForwardSpeed = 5f;
    [SerializeField] private float freeFallSpeed = 2f;
    
    [Header("Dodge")]
    private Vector3 rollDirection;
    [SerializeField] private float dodgeStaminaCost = 25;
    
    protected override void Awake()
    {
        base.Awake();
        
        player = GetComponent<PlayerManager>();
    }

    protected override void Update()
    {
        base.Update();

        if (player.IsOwner)
        {
            player.characterNetworkManager.verticalMovement.Value = verticalMovement;
            player.characterNetworkManager.horizontalMovement.Value = horizontalMovement;
            player.characterNetworkManager.moveAmount.Value = moveAmount;
        }
        else
        {
            verticalMovement = player.characterNetworkManager.verticalMovement.Value;
            horizontalMovement = player.characterNetworkManager.horizontalMovement.Value;
            moveAmount = player.characterNetworkManager.moveAmount.Value;

            player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount,
                player.playerNetworkManager.isSprinting.Value);
        }
    }

    public void HandleAllMovement()
    {
        HandleGroundedMovement();
        HandleJumpingMovement();
        HandleRotation();
        HandleFreeFallMovement();
    }

    private void GetMovementValues()
    {
        verticalMovement = PlayerInputManager.instance.verticalInput;
        horizontalMovement = PlayerInputManager.instance.horizontalInput;
        moveAmount = PlayerInputManager.instance.moveAmount;
    }

    private void HandleGroundedMovement()
    {
        if (!player.canMove)
            return;
        
        GetMovementValues();
        
        moveDirection =
            PlayerCamera.instance.transform.forward * verticalMovement +
            PlayerCamera.instance.transform.right * horizontalMovement;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (player.playerNetworkManager.isSprinting.Value)
        {
            player.characterController.Move(moveDirection * (sprintingSpeed * Time.deltaTime));
        }
        else
        {
            if (PlayerInputManager.instance.moveAmount > 0.5f)
            {
                //奔跑
                player.characterController.Move(moveDirection * (runningSpeed * Time.deltaTime));
            }
            else if (PlayerInputManager.instance.moveAmount <= 0.5f)
            {
                //行走
                player.characterController.Move(moveDirection * (walkingSpeed * Time.deltaTime));
            }   
        }
        
    }

    private void HandleJumpingMovement()
    {
        if (player.isJumping)
        {
            player.characterController.Move(jumpDirection * (jumpingForwardSpeed * Time.deltaTime));
        }
    }

    private void HandleFreeFallMovement()
    {
        if (!player.isGrounded)
        {
            Vector3 freeFallDirection;
            
            freeFallDirection = PlayerCamera.instance.cameraObject.transform.forward *
                                PlayerInputManager.instance.verticalInput;
            freeFallDirection += PlayerCamera.instance.cameraObject.transform.right *
                                 PlayerInputManager.instance.horizontalInput;
            freeFallDirection.y = 0;
            player.characterController.Move(freeFallDirection * (freeFallSpeed * Time.deltaTime));
        }
    }

    

    private void HandleRotation()
    {
        if(!player.canRotate)
            return;
        
        targetRotationDirection = Vector3.zero;
        targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalMovement +
                                  PlayerCamera.instance.cameraObject.transform.right * horizontalMovement;
        
        targetRotationDirection.Normalize();
        targetRotationDirection.y = 0;

        if (targetRotationDirection == Vector3.zero)
        {
            targetRotationDirection = transform.forward;
        }
        
        Quaternion newRotation = Quaternion.LookRotation(targetRotationDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = targetRotation;
    }

    public void AttemptToPerformDodge()
    {
        if (player.isPerformingAction)
            return;

        if (player.playerNetworkManager.currentStamina.Value <= 0)
            return;
        
        if (PlayerInputManager.instance.moveAmount > 0)
        {
            //翻滚
            rollDirection = PlayerCamera.instance.cameraObject.transform.forward *
                            PlayerInputManager.instance.verticalInput;
            rollDirection += PlayerCamera.instance.cameraObject.transform.right *
                             PlayerInputManager.instance.horizontalInput;
            rollDirection.y = 0;
            rollDirection.Normalize();
            
            Quaternion playerRotation = Quaternion.LookRotation(rollDirection);
            player.transform.rotation = playerRotation;

            player.playerAnimatorManager.PlayerTargetAnimation("Roll_Forward_01", true, true);
        }
        else
        {
            //后跳
            player.playerAnimatorManager.PlayerTargetAnimation("Back_Step_01", true, true);
        }

        player.playerNetworkManager.currentStamina.Value -= dodgeStaminaCost;
    }

    public void HandleSprinting()
    {
        if (player.isPerformingAction)
        {
            player.playerNetworkManager.isSprinting.Value = false;
        }

        if (player.playerNetworkManager.currentStamina.Value <= 0.5f)
        {
            player.playerNetworkManager.isSprinting.Value = false;
            return;
        }

        if (moveAmount >= 0.5f)
        {
            player.playerNetworkManager.isSprinting.Value = true;
        }
        else
        {
            player.playerNetworkManager.isSprinting.Value = false;
        }
        
        player.playerNetworkManager.currentStamina.Value -= sprintingCost *  Time.deltaTime;
    }

    public void AttemptToPerformJump()
    {
        if (player.isPerformingAction)
            return;

        if (player.playerNetworkManager.currentStamina.Value <= 0)
            return;
        
        if(player.isJumping)
            return;
        
        if(!player.isGrounded)
            return;
        
        //注意是单手跳跃，还是双持跳跃
        player.playerAnimatorManager.PlayerTargetAnimation("Main_Jump_01", false);

        player.isJumping = true;
        
        player.playerNetworkManager.currentStamina.Value -= jumpStaminaCost;

        jumpDirection = PlayerCamera.instance.cameraObject.transform.forward *
                        PlayerInputManager.instance.verticalInput;
        jumpDirection += PlayerCamera.instance.cameraObject.transform.right *
                         PlayerInputManager.instance.horizontalInput;
        jumpDirection.y = 0;

        if (jumpDirection != Vector3.zero)
        {
            if (player.playerNetworkManager.isSprinting.Value)
            {
                jumpDirection *= 1;
            }
            else if (PlayerInputManager.instance.moveAmount > 0.5)
            {
                jumpDirection *= 0.5f;
            }
            else if (PlayerInputManager.instance.moveAmount <= 0.5)
            {
                jumpDirection *= 0.25f;
            }
        }
    }

    public void ApplyJumpVelocity()
    {
        yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityForce);
    }
}
