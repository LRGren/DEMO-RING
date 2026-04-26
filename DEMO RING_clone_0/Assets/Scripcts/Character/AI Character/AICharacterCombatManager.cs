using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICharacterCombatManager : CharacterCombatManager
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float minimumDetectionAngle = -35f;
    [SerializeField] private float maximumDetectionAngle = 35f;

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
                float angleToTarget = Vector3.Angle(aiCharacter.transform.forward, targetDirection);

                if (angleToTarget >= minimumDetectionAngle && angleToTarget <= maximumDetectionAngle)
                {
                    if(Physics.Linecast(aiCharacter.characterCombatManager.lockOnTransform.position, 
                        targetCharacter.transform.position, 
                        WorldUtilityManager.instance.GetEnviroLayers()))
                    {
                        Debug.DrawLine(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.transform.position, Color.red, 0.1f);
                        Debug.Log("Blocked by environment");
                    }
                    else
                    {
                        aiCharacter.characterCombatManager.SetTarget(targetCharacter);
                    }
                }
            }
        }
    }
}
