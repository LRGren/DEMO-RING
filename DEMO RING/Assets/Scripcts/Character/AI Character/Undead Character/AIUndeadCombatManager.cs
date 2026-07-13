using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUndeadCombatManager : AICharacterCombatManager
{
    [Header("Undead Damage Colliders")]
    [SerializeField] private UndeadDamageCollider rightHandUndeadDamageCollider;
    [SerializeField] private UndeadDamageCollider leftHandUndeadDamageCollider;

    [Header("Undead Attack Damage")]
    [SerializeField] private int baseDamage = 25;
    [SerializeField] private float attack01Modifier = 1f;
    [SerializeField] private float attack02Modifier = 1.4f;

    public void SetAttack01Damage()
    {
        rightHandUndeadDamageCollider.physicalDamage = baseDamage * attack01Modifier;
        leftHandUndeadDamageCollider.physicalDamage = baseDamage * attack01Modifier;
    }

    public void SetAttack02Damage()
    {
        rightHandUndeadDamageCollider.physicalDamage = baseDamage * attack02Modifier;
        leftHandUndeadDamageCollider.physicalDamage = baseDamage * attack02Modifier;
    }

    public void EnableRightHandDamageCollider()
    {
        aiCharacterManager.characterSoundFXManager.PlayAttackGruntSFX();
        rightHandUndeadDamageCollider.EnableDamageCollider();
    }

    public void EnableLeftHandDamageCollider()
    {
        aiCharacterManager.characterSoundFXManager.PlayAttackGruntSFX();
        leftHandUndeadDamageCollider.EnableDamageCollider();
    }

    public void DisableRightHandDamageCollider()
    {
        rightHandUndeadDamageCollider.DisableDamageCollider();
    }

    public void DisableLeftHandDamageCollider()
    {
        leftHandUndeadDamageCollider.DisableDamageCollider();
    }
}
