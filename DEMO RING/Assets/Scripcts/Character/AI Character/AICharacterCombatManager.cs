using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AICharacterCombatManager : CharacterCombatManager
{
    [Header("Action Recovery Time")]
    public float actionRecoveryTimer = 0f;

    [Header("Target Information")]
    public float distanceFromTarget;
    public float viewableAngle;
    public Vector3 targetDirection;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;
    public float minimumFOV = -35f;
    public float maximumFOV = 35f;

    [Header("Attack Rotation Speed")]
    public float attackRotationSpeed = 25f;

    public void FindATargetViaLineOfSight(AICharacterManager aiCharacter)
    {
        if (currentTarget != null)
            return;

        Collider[] colliders = Physics.OverlapSphere(aiCharacter.transform.position, detectionRadius,WorldUtilityManager.instance.GetCharacterLayers());

        for(int i=0; i<colliders.Length; i++)
        {
            CharacterManager targetCharacter = colliders[i].GetComponent<CharacterManager>();
            
            if(targetCharacter == null)
                continue;

            if(targetCharacter == aiCharacter)
                continue;

            if(targetCharacter.isDead.Value)
                continue;

            if(WorldUtilityManager.instance.CanIDamageThisTarget(aiCharacter.characterGroup, targetCharacter.characterGroup))
            {
                //判断是否在视线范围内
                Vector3 targetDirection = targetCharacter.transform.position - aiCharacter.transform.position;
                float angleToPotentialTarget = Vector3.Angle(aiCharacter.transform.forward, targetDirection);

                if (angleToPotentialTarget >= minimumFOV && angleToPotentialTarget <= maximumFOV)
                {
                    if(Physics.Linecast(aiCharacter.characterCombatManager.lockOnTransform.position, 
                        targetCharacter.transform.position, 
                        WorldUtilityManager.instance.GetEnviroLayers()))
                    {
                        Debug.DrawLine(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.transform.position, Color.red, 0.1f);
                        //Debug.Log("Blocked by environment");
                    }
                    else
                    {
                        targetDirection = targetCharacter.transform.position - aiCharacter.transform.position;
                        viewableAngle = WorldUtilityManager.instance.GetAngleOfTarget(transform, targetDirection);

                        aiCharacter.characterCombatManager.SetTarget(targetCharacter);
                        PivotTowardsTarget(aiCharacter);
                    }
                }
            }
        }
    }

    public void PivotTowardsTarget(AICharacterManager aiCharacter)
    {
        if (aiCharacter.isPerformingAction)
            return;

        // --- 右转逻辑 (正数) ---
        if (viewableAngle >= 20f && viewableAngle <= 60f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Right_45", true);
        }
        else if (viewableAngle > 60f && viewableAngle <= 110f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Right_90", true);
        }
        else if (viewableAngle > 110f && viewableAngle <= 150f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Right_135", true);
        }
        else if (viewableAngle > 150f && viewableAngle <= 180f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Right_180", true);
        }

        // --- 左转逻辑 (负数) ---
        else if (viewableAngle <= -20f && viewableAngle >= -60f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Left_45", true);
        }
        else if (viewableAngle < -60f && viewableAngle >= -110f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Left_90", true);
        }
        else if (viewableAngle < -110f && viewableAngle >= -150f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Left_135", true);
        }
        else if (viewableAngle < -150f && viewableAngle >= -180f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Left_180", true);
        }
    }

    public void RotateTowardsAgent(AICharacterManager aiCharacter)
    {
        if (aiCharacter.aiCharacterNetworkManager.isMoving.Value)
        {
            aiCharacter.transform.rotation = aiCharacter.navMeshAgent.transform.rotation;
        }
    }

    public void RotateTowardsTargetWhilstAttacking(AICharacterManager aiCharacter)
    {
        if(currentTarget == null)
            return;

        if (!aiCharacter.canRotate)
            return;

        if (!aiCharacter.isPerformingAction)
            return;

        Vector3 tmp_TargetDirection = currentTarget.transform.position - aiCharacter.transform.position;
        tmp_TargetDirection.y = 0;
        tmp_TargetDirection.Normalize();

        if(tmp_TargetDirection == Vector3.zero)
        {
            tmp_TargetDirection = aiCharacter.transform.forward;
        }

        Quaternion tmp_TargetRotation = Quaternion.LookRotation(tmp_TargetDirection);

        aiCharacter.transform.rotation = Quaternion.Slerp(aiCharacter.transform.rotation, tmp_TargetRotation, attackRotationSpeed * Time.deltaTime);
    }

    public void HandleActionRecovery(AICharacterManager aiCharacter)
    {
       if(actionRecoveryTimer > 0)
        {
            if(!aiCharacter.isPerformingAction)
            {
                actionRecoveryTimer -= Time.deltaTime;
            }
        } 
    }

}
