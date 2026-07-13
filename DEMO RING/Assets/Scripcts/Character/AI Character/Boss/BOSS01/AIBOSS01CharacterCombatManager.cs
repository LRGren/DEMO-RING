using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBOSS01CharacterCombatManager : AICharacterCombatManager
{
    [Header("BOSS01 Damage Colliders")]
    [SerializeField] private DamageCollider swordDamageCollider;

    [Header("BOSS01 Attack Damage")]
    [SerializeField] private int baseDamage = 50;
    [SerializeField] private float attack01Modifier = 1f;
    [SerializeField] private float attack02Modifier = 1.4f;

    public void SetAttack01Damage()
    {
        swordDamageCollider.physicalDamage = baseDamage * attack01Modifier;
    }

    public void SetAttack02Damage()
    {
        swordDamageCollider.physicalDamage = baseDamage * attack02Modifier;
    }

    public void EnableRightHandDamageCollider()
    {
        aiCharacterManager.characterSoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.EnableDamageCollider();
    }

    public void DisableRightHandDamageCollider()
    {
        swordDamageCollider.DisableDamageCollider();
    }
}
