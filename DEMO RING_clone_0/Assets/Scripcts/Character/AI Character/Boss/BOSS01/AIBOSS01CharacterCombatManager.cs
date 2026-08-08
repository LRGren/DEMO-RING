using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBOSS01CharacterCombatManager : AICharacterCombatManager
{
    [Header("BOSS01 Damage Colliders")]
    [SerializeField] private BOSS01DamagerCollider swordDamageCollider;

    [Header("BOSS01 Attack Damage")]
    [SerializeField] private int baseDamage = 50;
    [SerializeField] private float attack01Modifier = 1f;
    [SerializeField] private float attack02Modifier = 1.4f;
    [SerializeField] private float attack03Modifier = 2.5f;

    public void SetAttack01Damage()
    {
        swordDamageCollider.physicalDamage = baseDamage * attack01Modifier;
    }

    public void SetAttack02Damage()
    {
        swordDamageCollider.physicalDamage = baseDamage * attack02Modifier;
    }

    public void SetAttack03Damage()
    {
        swordDamageCollider.physicalDamage = baseDamage * attack03Modifier;
    }

    public void EnableSwordDamageCollider()
    {
        aiCharacterManager.characterSoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.EnableDamageCollider();
    }

    public void DisableSwordDamageCollider()
    {
        swordDamageCollider.DisableDamageCollider();
    }

    public void ActivateBOSS01Stomp()
    {

    }

    public override void PivotTowardsTarget(AICharacterManager aiCharacter)
    {
        if (aiCharacter.isPerformingAction)
            return;

        // --- 右转逻辑 (正数) ---
        if (viewableAngle > 60f && viewableAngle <= 110f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Right_90", true);
        }
        else if (viewableAngle > 150f && viewableAngle <= 180f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Right_180", true);
        }

        // --- 左转逻辑 (负数) ---
        else if (viewableAngle < -60f && viewableAngle >= -110f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Left_90", true);
        }
        else if (viewableAngle < -150f && viewableAngle >= -180f)
        {
            aiCharacter.characterAnimatorManager.PlayerTargetActionAnimation("Turn_Left_180", true);
        }
    }
}
