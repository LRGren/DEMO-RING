using System.Collections;
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
    private float originalRotationSpeed;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float sprintingCost = 5f;

    [Header("Jump")]
    private Vector3 jumpDirection;
    [SerializeField] private float jumpStaminaCost = 10;
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float jumpingForwardSpeed = 5f;
    [SerializeField] private float freeFallSpeed = 2f;
    [SerializeField, Min(0f)] private float lateJumpAttackWindow = 0.55f;

    private WeaponItem queuedJumpWeapon;
    private WeaponItem queuedRightWeapon;
    private WeaponItem queuedLeftWeapon;
    private AttackType queuedJumpAttackType;
    private string queuedGroundAttackState;
    private bool queuedJumpUsesRightHand;
    private bool queuedJumpTwoHanded;
    private float queuedJumpDeadline;

    private const string ActionLayer = "Action Override";
    private const float JumpAttackBlend = 0.10f;

    [Header("Dodge")]
    private Vector3 rollDirection;
    [SerializeField] private float dodgeStaminaCost = 25;

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
        originalRotationSpeed = rotationSpeed;
    }

    protected override void Update()
    {
        base.Update();

        if (player.IsOwner)
        {
            UpdateQueuedJumpAttack();
            player.characterNetworkManager.verticalMovement.Value = verticalMovement;
            player.characterNetworkManager.horizontalMovement.Value = horizontalMovement;
            player.characterNetworkManager.moveAmount.Value = moveAmount;
        }
        else
        {
            verticalMovement = player.characterNetworkManager.verticalMovement.Value;
            horizontalMovement = player.characterNetworkManager.horizontalMovement.Value;
            moveAmount = player.characterNetworkManager.moveAmount.Value;

            if (!player.playerNetworkManager.isLockOn.Value || player.playerNetworkManager.isSprinting.Value)
            {
                //未锁定时只需要前进的动作 或者疾跑
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
            }
            else
            {
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontalMovement, verticalMovement,
                player.playerNetworkManager.isSprinting.Value);
            }
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
        if (player.characterLocomotionManager.canMove || player.playerLocomotionManager.canRotate)
        {
            GetMovementValues();
        }
        else
        {
            verticalMovement = 0;
            horizontalMovement = 0;
            moveAmount = 0;
        }

        if (!player.characterLocomotionManager.canMove)
            return;


        if (player.playerNetworkManager.isAiming.Value)
        {
            moveDirection =
                player.transform.forward * verticalMovement +
                player.transform.right * horizontalMovement;
            moveDirection.Normalize();
            moveDirection.y = 0;
        }
        else
        {
            moveDirection =
                PlayerCamera.instance.transform.forward * verticalMovement +
                PlayerCamera.instance.transform.right * horizontalMovement;
            moveDirection.Normalize();
            moveDirection.y = 0;
        }

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
        if (player.playerNetworkManager.isJumping.Value)
        {
            player.characterController.Move(jumpDirection * (jumpingForwardSpeed * Time.deltaTime));
        }
    }

    private void HandleFreeFallMovement()
    {
        if (!player.playerLocomotionManager.isGrounded)
        {
            Vector3 freeFallDirection;

            freeFallDirection = PlayerCamera.instance.cameraObject.transform.forward * PlayerInputManager.instance.verticalInput;
            freeFallDirection += PlayerCamera.instance.cameraObject.transform.right * PlayerInputManager.instance.horizontalInput;
            freeFallDirection.y = 0;
            player.characterController.Move(freeFallDirection * (freeFallSpeed * Time.deltaTime));
        }
    }

    private void HandleRotation()
    {
        if (player.isDead.Value)
            return;

        if (!player.characterLocomotionManager.canRotate)
            return;

        if (player.playerNetworkManager.isAiming.Value)
        {
            HandleAimingRotation();
        }
        else
        {
            HandleStandardRotation();
        }
    }

    public void HandleAimingRotation()
    {
        targetRotationDirection = Vector3.zero;
        targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward;

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

    private void HandleStandardRotation()
    {
        if (player.playerNetworkManager.isLockOn.Value)
        {
            if (player.playerNetworkManager.isSprinting.Value || player.playerLocomotionManager.isRolling)
            {
                //如果当前有锁定目标，并且正在疾跑或者翻滚，那么应该根据移动方向进行旋转
                Vector3 targetDirection = Vector3.zero;
                targetDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalMovement + PlayerCamera.instance.cameraObject.transform.right * horizontalMovement;

                targetDirection.Normalize();
                targetDirection.y = 0;

                if (targetDirection == Vector3.zero)
                {
                    //如果没有移动方向，那么应该保持当前朝向
                    targetDirection = transform.forward;
                }

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.rotation = finalRotation;
            }
            else
            {
                //如果当前有锁定目标但是没有在疾跑或翻滚，那么应该始终面向锁定目标
                if (player.playerCombatManager.currentTarget == null)
                    return;

                Vector3 targetDirection = player.playerCombatManager.currentTarget.transform.position - transform.position;
                targetDirection.y = 0;
                targetDirection.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.rotation = finalRotation;
            }
        }
        else
        {
            //如果没有锁定目标，那么应该根据移动方向进行旋转
            targetRotationDirection = Vector3.zero;
            targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalMovement + PlayerCamera.instance.cameraObject.transform.right * horizontalMovement;

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

    }

    public void AttemptToPerformDodge()
    {
        if (!canRoll)
            return;

        //如果当前体力小于等于0，那么无法翻滚
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

            player.playerAnimatorManager.PlayerTargetActionAnimation("Roll_Forward_01", true, true);
            player.playerLocomotionManager.isRolling = true;
        }
        else
        {
            //后跳
            player.playerAnimatorManager.PlayerTargetActionAnimation("Back_Step_01", true, true);
        }

        player.playerNetworkManager.currentStamina.Value -= dodgeStaminaCost;
        player.playerCombatManager.DestoryAllAttemptedActions();
    }

    public void HandleSprinting()
    {
        if (player.isPerformingAction)
        {
            player.playerNetworkManager.isSprinting.Value = false;
            return;
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

        player.playerNetworkManager.currentStamina.Value -= sprintingCost * Time.deltaTime;
    }

    public void AttemptToPerformJump()
    {
        if (player.isPerformingAction)
            return;

        if (player.playerNetworkManager.currentStamina.Value <= 0)
            return;

        if (player.playerNetworkManager.isJumping.Value)
            return;

        if (!player.playerLocomotionManager.isGrounded)
            return;

        //注意是单手跳跃，还是双持跳跃

        if (player.playerNetworkManager.isTwoHandingWeapon.Value)
            player.playerAnimatorManager.PlayerTargetActionAnimation("TH_Jump_01", false);
        else
            player.playerAnimatorManager.PlayerTargetActionAnimation("Main_Jump_01", false);

        player.playerNetworkManager.isJumping.Value = true;

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

    public bool CanStartJumpAttack()
    {
        return player.playerCombatManager.canPerformJumpAttack &&
            (!isGrounded || (player.playerNetworkManager.isJumping.Value && yVelocity.y < 0f));
    }

    public void PerformJumpAttack(WeaponItem weapon, AttackType attackType, string airborneState)
    {
        if (player.isPerformingAction || queuedJumpWeapon != null)
            return;

        int layer = player.animator.GetLayerIndex(ActionLayer);
        string lateState = ActionLayer + ".Jump Attacks." + airborneState + "_Ground_Attack";
        if (lateJumpAttackWindow > 0f && layer >= 0 &&
            player.animator.HasState(layer, Animator.StringToHash(lateState)) && IsLandingSoon())
        {
            queuedJumpWeapon = weapon;
            queuedRightWeapon = player.playerInventoryManager.currentRightHandWeapon;
            queuedLeftWeapon = player.playerInventoryManager.currentLeftHandWeapon;
            queuedJumpAttackType = attackType;
            queuedGroundAttackState = lateState;
            queuedJumpUsesRightHand = player.playerNetworkManager.isUsingRightHand.Value;
            queuedJumpTwoHanded = player.playerNetworkManager.isTwoHandingWeapon.Value;
            queuedJumpDeadline = Time.time + lateJumpAttackWindow + 0.20f;
            return;
        }

        player.playerCombatManager.DisableCanPerformJumpAttack();
        player.playerAnimatorManager.PlayerTargetAttackActionAnimation(
            weapon, attackType, airborneState, true, fixedTransitionDuration: JumpAttackBlend);
    }

    private bool IsLandingSoon()
    {
        if (player.characterController.isGrounded && yVelocity.y <= 0f)
            return true;

        float distance = (-yVelocity.y - 0.5f * gravityForce * lateJumpAttackWindow) * lateJumpAttackWindow;
        if (distance <= 0f)
            return false;

        float lift = groundCheckSphereRadius;
        Vector3 origin = transform.position + Vector3.up * lift;
        return Physics.SphereCast(origin, groundCheckSphereRadius, Vector3.down, out RaycastHit hit,
                   distance + lift, groundLayer, QueryTriggerInteraction.Ignore) &&
            Vector3.Angle(hit.normal, Vector3.up) <= player.characterController.slopeLimit;
    }

    private void UpdateQueuedJumpAttack()
    {
        if (queuedJumpWeapon == null)
            return;

        var network = player.playerNetworkManager;
        var inventory = player.playerInventoryManager;
        if (Time.time > queuedJumpDeadline || player.isDead.Value || player.playerCombatManager.isUsingItem ||
            inventory.currentRightHandWeapon != queuedRightWeapon || inventory.currentLeftHandWeapon != queuedLeftWeapon ||
            network.isTwoHandingWeapon.Value != queuedJumpTwoHanded ||
            (queuedJumpTwoHanded && network.isTwoHandingRightWeapon.Value != queuedJumpUsesRightHand))
        {
            ClearQueuedJumpAttack();
            return;
        }

        if (player.isPerformingAction)
        {
            ClearQueuedJumpAttack();
            return;
        }

        if (!player.characterController.isGrounded)
            return;

        WeaponItem weapon = queuedJumpWeapon;
        AttackType attackType = queuedJumpAttackType;
        string groundAttack = queuedGroundAttackState;
        bool useRightHand = queuedJumpUsesRightHand;
        ClearQueuedJumpAttack();

        player.playerCombatManager.DisableCanPerformJumpAttack();
        network.SetCharacterActionHand(useRightHand);
        network.currentWeaponBeingUsed.Value = weapon.itemID;
        player.playerAnimatorManager.PlayerTargetAttackActionAnimation(
            weapon, attackType, groundAttack, true, fixedTransitionDuration: JumpAttackBlend);
    }

    private void ClearQueuedJumpAttack()
    {
        queuedJumpWeapon = null;
        queuedRightWeapon = null;
        queuedLeftWeapon = null;
    }

    public void ApplyJumpVelocity()
    {
        player.playerCombatManager.EnableCanPerformJumpAttack();

        yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityForce);
    }

    public void ChangeRotationSpeed(float newRotationSpeed)
    {
        rotationSpeed = newRotationSpeed;
    }

    public void ResetRotationSpeed()
    {
        rotationSpeed = originalRotationSpeed;
    }
}
