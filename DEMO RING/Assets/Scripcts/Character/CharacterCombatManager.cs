using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterCombatManager : NetworkBehaviour
{
    protected CharacterManager characterManager;

    [Header("Attack Target")]
    public CharacterManager currentTarget;

    [Header("Poise")]
    public float previousPoiseDamageTaken;

    [Header("Last Attack Animation")]
    public string lastAttackAnimation = "";

    [Header("Attack Type")]
    public AttackType currentAttackType;

    [Header("Lock On Transform")]
    public Transform lockOnTransform;

    [Header("Attack Flags")]
    public bool canPerformRollingAttack = false;
    public bool canPerformBackstepAttack = false;
    public bool canBlock = true;
    public bool canBeBackstabbed = true;

    [Header("Critical Attack")]
    [SerializeField] Transform riposteReceiverTransform;
    [SerializeField] Transform backstabReceiverTransform;
    [SerializeField] float criticalAttackMaxDistance = 0.7f;
    public int pendingCriticalAttackDamage = 0;

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public virtual void SetTarget(CharacterManager newTarget)
    {
        if (characterManager.IsOwner)
        {
            if (newTarget != null)
            {
                currentTarget = newTarget;

                //通知NETWORK，让其他玩家知道这个角色锁定了一个目标
                characterManager.characterNetworkManager.currentTargetNetworkObjectID.Value = newTarget.gameObject.GetComponent<NetworkObject>().NetworkObjectId;

            }
            else
            {
                currentTarget = null;
            }
        }
    }

    public virtual void AttemptCriticalAttack()
    {
        if (characterManager.isPerformingAction)
            return;

        if (characterManager.characterNetworkManager.currentStamina.Value <= 0)
            return;

        RaycastHit[] hits = Physics.RaycastAll(lockOnTransform.position, characterManager.transform.TransformDirection(Vector3.forward), criticalAttackMaxDistance, WorldUtilityManager.instance.GetCharacterLayers());

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            CharacterManager targetCharacter = hit.transform.GetComponent<CharacterManager>();

            if (targetCharacter != null)
            {
                if (targetCharacter == characterManager)
                    continue;

                if (!WorldUtilityManager.instance.CanIDamageThisTarget(characterManager.characterGroup, targetCharacter.characterGroup))
                    continue;

                Vector3 directionFromTargetToCharacter = characterManager.transform.position - targetCharacter.transform.position;
                float targetViewAngle = Vector3.SignedAngle(directionFromTargetToCharacter, targetCharacter.transform.forward, Vector3.up);

                if (targetCharacter.characterNetworkManager.isRipostable.Value)
                {
                    if (targetViewAngle > -60 && targetViewAngle < 60)
                    {
                        AttemptRiposte(hit);
                        return;
                    }
                }

                // Backstab
                if (targetCharacter.characterCombatManager.canBeBackstabbed)
                {
                    if ((targetViewAngle >= 145 && targetViewAngle <= 180) || (targetViewAngle >= -180 && targetViewAngle <= -140))
                    {
                        AttemptBackstab(hit);
                        return;
                    }
                }
            }
        }

    }

    public virtual void AttemptRiposte(RaycastHit hit)
    {
        Debug.Log("Attempting to riposte");
    }

    public virtual void AttemptBackstab(RaycastHit hit)
    {
        Debug.Log("Attempting to backstab");
    }

    public void ApplyCriticalDamage()
    {
        characterManager.characterEffectsManager.PlayCriticalBloodSplatterVFX(lockOnTransform.position);
        characterManager.characterSoundFXManager.PlayCriticalSrikeSoundFX();

        if (characterManager.IsOwner)
        {
            characterManager.characterNetworkManager.currentHealth.Value -= pendingCriticalAttackDamage;
            pendingCriticalAttackDamage = 0;
        }
    }

    public IEnumerator ForceMoveEnemyCharacterToRipostePosition(CharacterManager enemyCharacter, Vector3 ripostePosition)
    {
        float timer = 0f;
        if (timer < 0.02f)
        {
            timer += Time.deltaTime;

            if (enemyCharacter.characterCombatManager.riposteReceiverTransform != null)
            {
                // 敌人自带 receiver 点位：把玩家贴到该点位，面向敌人
                Transform receiver = enemyCharacter.characterCombatManager.riposteReceiverTransform;

                if (characterManager.characterController != null)
                    characterManager.characterController.enabled = false;

                transform.position = receiver.position;
                transform.rotation = Quaternion.LookRotation(-enemyCharacter.transform.forward);

                if (characterManager.characterController != null)
                    characterManager.characterController.enabled = true;
            }
            else
            {
                // 否则：把敌人贴到玩家面前，面向玩家
                if (riposteReceiverTransform == null)
                {
                    GameObject riposteReceiver = new GameObject("RiposteReceiver");
                    riposteReceiver.transform.parent = transform;
                    riposteReceiver.transform.localPosition = Vector3.zero;
                    riposteReceiverTransform = riposteReceiver.transform;
                }

                riposteReceiverTransform.localPosition = ripostePosition;

                if (enemyCharacter.characterController != null)
                    enemyCharacter.characterController.enabled = false;

                enemyCharacter.transform.position = riposteReceiverTransform.position;
                enemyCharacter.transform.rotation = Quaternion.LookRotation(-transform.forward);

                if (enemyCharacter.characterController != null)
                    enemyCharacter.characterController.enabled = true;
            }

            yield return null;
        }
    }

    public IEnumerator ForceMoveEnemyCharacterToBackstabPosition(CharacterManager enemyCharacter, Vector3 backstabPosition)
    {
        float timer = 0f;
        while (timer < 0.02f)
        {
            timer += Time.deltaTime;

            if (backstabReceiverTransform == null)
            {
                GameObject backstabReceiver = new GameObject("BackstabReceiver");
                backstabReceiver.transform.parent = transform;
                backstabReceiver.transform.localPosition = Vector3.zero;
                backstabReceiverTransform = backstabReceiver.transform;
            }

            backstabReceiverTransform.localPosition = backstabPosition;
            enemyCharacter.transform.position = backstabReceiverTransform.position;
            backstabReceiverTransform.localRotation = Quaternion.LookRotation(-enemyCharacter.transform.forward);
            yield return null;
        }
    }

    public virtual void EnableIsRipostable()
    {
        if (characterManager.IsOwner)
            characterManager.characterNetworkManager.isRipostable.Value = true;
    }

    public void EnableIsInvulnerable()
    {
        if (characterManager.IsOwner)
            characterManager.characterNetworkManager.isInvulnerable.Value = true;
    }

    public void DisableIsInvulnerable()
    {
        if (characterManager.IsOwner)
            characterManager.characterNetworkManager.isInvulnerable.Value = false;
    }

    public void EnableCanPerformRollingAttack()
    {
        canPerformRollingAttack = true;
    }

    public void DisableCanPerformRollingAttack()
    {
        canPerformRollingAttack = false;
    }

    public void EnableCanPerformBackstepAttack()
    {
        canPerformBackstepAttack = true;
    }

    public void DisableCanPerformBackstepAttack()
    {
        canPerformBackstepAttack = false;
    }

    public virtual void EnableDoCombo()
    {
    }

    public virtual void DisableDoCombo()
    {
    }
}
