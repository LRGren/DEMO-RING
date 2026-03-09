using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterLocomotionManager : MonoBehaviour
{
    private CharacterManager character;
    
    [Header("Ground Check & Jump")] 
    [SerializeField] protected float gravityForce = -40f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckSphereRadius = 0.3f;
    [SerializeField] protected Vector3 yVelocity;//用来观测是上升还是下降
    [SerializeField] protected float groundedYVelocity = -20;
    [SerializeField] protected float fallStartYVelocity = -5;
    protected bool fallingVelocityHasBeenSet = false;
    protected float inAirTimer = 0;
    
    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    protected virtual void Update()
    {
        HandleGroundCheck();

        if (character.isGrounded)
        {
            //没有尝试跳跃或者向上移动
            if (yVelocity.y < 0)
            {
                inAirTimer = 0;
                fallingVelocityHasBeenSet = false;
                yVelocity.y = groundedYVelocity;
            }
        }
        else
        {
            //没有在跳跃并且我们的下落速度没有设置
            if (!character.characterNetworkManager.isJumping.Value && !fallingVelocityHasBeenSet)
            {
                fallingVelocityHasBeenSet = true;
                yVelocity.y = fallStartYVelocity;
            }

            inAirTimer += Time.deltaTime;
            character.animator.SetFloat("inAirTimer", inAirTimer);
            
            yVelocity.y += gravityForce * Time.deltaTime;
        }
        
        //一直模拟重力
        character.characterController.Move(yVelocity * Time.deltaTime);
    }
    

    protected void HandleGroundCheck()
    {
        character.isGrounded = Physics.CheckSphere(character.transform.position, groundCheckSphereRadius, groundLayer);
    }

    protected void OnDrawGizmosSelected()
    {
        //Gizmos.DrawSphere(character.transform.position, groundCheckSphereRadius);
    }
}
