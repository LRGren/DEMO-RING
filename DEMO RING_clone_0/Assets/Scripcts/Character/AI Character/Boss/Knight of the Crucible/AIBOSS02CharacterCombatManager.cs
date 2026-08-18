using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBOSS02CharacterCombatManager : AICharacterCombatManager
{
    AIBOSS02CharacterManager aiBOSS02CharacterManager;


    [Header("BOSS02 Damage Colliders")]
    [SerializeField] private BOSS02DamageCollider swordDamageCollider;
    [SerializeField] private BOSS02DamageCollider shieldDamageCollider;
    [SerializeField] private BOSS02DamageCollider wholeBodyDamageCollider;

    [Header("BOSS02 Earthquake")]
    [SerializeField] private Transform stumpAttackPoint;
    [SerializeField] private float stumpAttackRadius = 2f;
    [SerializeField] private GameObject stumpAttackEffect;

    [Header("BOSS02 Attack Damage")]
    [SerializeField] private int baseDamage = 50;
    [SerializeField] private float basePoiseDamage = 25;
    [SerializeField] private float attack01Modifier = 1f;
    [SerializeField] private float attack02Modifier = 1.4f;
    [SerializeField] private float attack03Modifier = 2.5f;
    [SerializeField] private float stumpAttackDamage = 60f;

    override protected void Awake()
    {
        base.Awake();

        aiBOSS02CharacterManager = GetComponent<AIBOSS02CharacterManager>();
    }

    public void SetAttack01Damage()
    {
        aiBOSS02CharacterManager.aiBOSS02SoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.physicalDamage = baseDamage * attack01Modifier;
        swordDamageCollider.poiseDamage = basePoiseDamage * attack01Modifier;

        shieldDamageCollider.physicalDamage = baseDamage * attack01Modifier;
        shieldDamageCollider.poiseDamage = basePoiseDamage * attack01Modifier;
    }

    public void SetAttack02Damage()
    {
        aiBOSS02CharacterManager.aiBOSS02SoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.physicalDamage = baseDamage * attack02Modifier;
        swordDamageCollider.poiseDamage = basePoiseDamage * attack02Modifier;

        shieldDamageCollider.physicalDamage = baseDamage * attack02Modifier;
        shieldDamageCollider.poiseDamage = basePoiseDamage * attack02Modifier;
    }

    public void SetAttack03Damage()
    {
        aiBOSS02CharacterManager.aiBOSS02SoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.physicalDamage = baseDamage * attack03Modifier;
        swordDamageCollider.poiseDamage = basePoiseDamage * attack03Modifier;

        shieldDamageCollider.physicalDamage = baseDamage * attack03Modifier;
        shieldDamageCollider.poiseDamage = basePoiseDamage * attack03Modifier;
    }

    public void EnableSwordDamageCollider()
    {
        swordDamageCollider.EnableDamageCollider();

        aiBOSS02CharacterManager.aiBOSS02SoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(aiBOSS02CharacterManager.aiBOSS02SoundFXManager.wooshes));
    }

    public void DisableSwordDamageCollider()
    {
        swordDamageCollider.DisableDamageCollider();
    }

    public void EnableShieldDamageCollider()
    {
        shieldDamageCollider.EnableDamageCollider();
    }

    public void DisableShieldDamageCollider()
    {
        shieldDamageCollider.DisableDamageCollider();
    }

    public void EnableWholeBodyDamageCollider()
    {
        wholeBodyDamageCollider.EnableDamageCollider();
    }

    public void DisableWholeBodyDamageCollider()
    {
        wholeBodyDamageCollider.DisableDamageCollider();
    }

    public void ActivateBOSS02StumpAttack()
    {
        // Implement the earthquake attack logic here
        Collider[] colliders = Physics.OverlapSphere(stumpAttackPoint.position, stumpAttackRadius, WorldUtilityManager.instance.GetCharacterLayers());

        List<CharacterManager> characterDamaged = new List<CharacterManager>
        {
            GetComponentInParent<CharacterManager>()
        };

        foreach (Collider coll in colliders)
        {
            CharacterManager character = coll.GetComponentInParent<CharacterManager>();

            if (character != null)
            {
                if (characterDamaged.Contains(character))
                    continue;

                Debug.Log("Stump Attack hit: " + character.name);
                characterDamaged.Add(character);

                TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
                damageEffect.physicalDamage = stumpAttackDamage;
                damageEffect.poiseDamage = stumpAttackDamage;

                character.characterEffectsManager.ProcessInstantEffect(damageEffect);
            }
        }

        if (stumpAttackEffect != null)
        {
            aiBOSS02CharacterManager.aiBOSS02SoundFXManager.PlaySoundFX(aiBOSS02CharacterManager.aiBOSS02SoundFXManager.stumpAttackSFX);
            Instantiate(stumpAttackEffect, stumpAttackPoint.position, Quaternion.Euler(-90f, 0f, 0f));
        }
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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(stumpAttackPoint.position, stumpAttackRadius);
    }

}
