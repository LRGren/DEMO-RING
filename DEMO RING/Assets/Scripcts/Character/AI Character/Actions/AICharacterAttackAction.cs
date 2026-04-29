using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "A.I/Actions/Attack")]
public class AICharacterAttackAction : ScriptableObject
{
    [Header("Attack Action")]
    [SerializeField] private string actionAnimation;

    [Header("Combo Attack Action")]
    public AICharacterAttackAction comboAction;

    [Header("Attack Values")]
    [SerializeField] private AttackType attackType;
    public int attackWeigth = 50;
    public float attackRecoveryTime = 1.5f;
    public float maximumAttackAngle = 35f;
    public float minimumAttackAngle = -35f;
    public float maximumAttackDistance = 3f;
    public float minimumAttackDistance = 0f;
    

    public void AttemptToPerformAction(AICharacterManager aiCharacter)
    {
        aiCharacter.characterAnimatorManager.PlayerTargetAttackActionAnimation(attackType, actionAnimation, true);
    }

}
