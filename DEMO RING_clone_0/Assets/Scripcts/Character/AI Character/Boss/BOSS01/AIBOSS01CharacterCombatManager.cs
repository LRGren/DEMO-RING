using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBOSS01CharacterCombatManager : AICharacterCombatManager
{
    AIBOSS01CharacterManager aiBOSS01CharacterManager;

    [Header("BOSS01 Damage Colliders")]
    [SerializeField] private BOSS01DamageCollider swordDamageCollider;

    [Header("BOSS01 Earthquake")]
    [SerializeField] private Transform earthquakeAttackPoint;
    [SerializeField] private float earthquakeAttackRadius = 2f;
    [SerializeField] private GameObject earthquakeAttackEffect;

    [Header("BOSS01 SIGNAL")]
    [SerializeField] private Transform signalEffectPoint;
    [SerializeField] private GameObject BOSS01SignalEffect;

    [Header("BOSS01 Attack Damage")]
    [SerializeField] private int baseDamage = 50;
    [SerializeField] private float basePoiseDamage = 25;
    [SerializeField] private float attack01Modifier = 1f;
    [SerializeField] private float attack02Modifier = 1.4f;
    [SerializeField] private float attack03Modifier = 2.5f;
    [SerializeField] private float earthquakeDamage = 60f;

    override protected void Awake()
    {
        base.Awake();

        aiBOSS01CharacterManager = GetComponent<AIBOSS01CharacterManager>();
    }

    public void SetAttack01Damage()
    {
        aiBOSS01CharacterManager.aiBOSS01SoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.physicalDamage = baseDamage * attack01Modifier;
        swordDamageCollider.poiseDamage = basePoiseDamage * attack01Modifier;
    }

    public void SetAttack02Damage()
    {
        aiBOSS01CharacterManager.aiBOSS01SoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.physicalDamage = baseDamage * attack02Modifier;
        swordDamageCollider.poiseDamage = basePoiseDamage * attack02Modifier;
    }

    public void SetAttack03Damage()
    {
        aiBOSS01CharacterManager.aiBOSS01SoundFXManager.PlayAttackGruntSFX();
        swordDamageCollider.physicalDamage = baseDamage * attack03Modifier;
        swordDamageCollider.poiseDamage = basePoiseDamage * attack03Modifier;
    }

    public void EnableSwordDamageCollider()
    {
        swordDamageCollider.EnableDamageCollider();

        aiBOSS01CharacterManager.aiBOSS01SoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(aiBOSS01CharacterManager.aiBOSS01SoundFXManager.wooshes));
    }

    public void DisableSwordDamageCollider()
    {
        swordDamageCollider.DisableDamageCollider();
    }

    public void ActivateBOSS01Earthquake()
    {
        // Implement the earthquake attack logic here
        Collider[] colliders = Physics.OverlapSphere(earthquakeAttackPoint.position, earthquakeAttackRadius, WorldUtilityManager.instance.GetCharacterLayers());

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

                Debug.Log("Earthquake hit: " + character.name);
                characterDamaged.Add(character);

                TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
                damageEffect.physicalDamage = earthquakeDamage;
                damageEffect.poiseDamage = earthquakeDamage;

                character.characterEffectsManager.ProcessInstantEffect(damageEffect);
            }
        }

        if (earthquakeAttackEffect != null)
        {
            aiBOSS01CharacterManager.aiBOSS01SoundFXManager.PlaySoundFX(aiBOSS01CharacterManager.aiBOSS01SoundFXManager.earthquakeSFX);
            Instantiate(earthquakeAttackEffect, earthquakeAttackPoint.position, Quaternion.identity);
        }
    }
    public void ActivateBOSS01Signal()
    {
        if (BOSS01SignalEffect != null)
        {
            //Debug.Log("Activating BOSS01 Signal Effect");
            Instantiate(BOSS01SignalEffect, signalEffectPoint.position, Quaternion.identity);
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
        Gizmos.DrawWireSphere(earthquakeAttackPoint.position, earthquakeAttackRadius);
    }

}
