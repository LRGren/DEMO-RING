using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelingSpellManager : SpellManager
{
    public PlayerManager player;
    protected LineRenderer lineRenderer;

    [Header("Channeling Spell Settings")]
    public ChannelingSpell channelingSpell;
    public int channelLength = 15;
    protected float currentLength = 0f;
    public Transform startPoint;
    public Vector3 forward;

    [Header("Damage")]
    protected Vector3 directionToAttacker;
    protected float dotFromDamageTargetToAttacker;

    protected override void Awake()
    {
        base.Awake();

        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        UpdateChannelingSpell();
        ContinuouslyTakeEffect();
    }

    public void SetChannelForward(Vector3 forward)
    {
        this.forward = forward;
    }
    public void SetStartPoint(Transform point)
    {
        startPoint = point;
    }

    protected void UpdateChannelingSpell()
    {
        lineRenderer.SetPosition(0, startPoint.transform.position);

        Vector3 endPosition = startPoint.position + forward * channelLength;

        RaycastHit hit;
        if (Physics.Raycast(startPoint.position, forward, out hit, channelLength, WorldUtilityManager.instance.GetEnviroLayers()))
        {
            endPosition = hit.point;
        }

        lineRenderer.SetPosition(1, endPosition);
        currentLength = Vector3.Distance(startPoint.position, endPosition);
    }

    protected float tickTimer = 0f;
    protected void ContinuouslyTakeEffect()
    {
        if (!player)
            return;

        if (!player.IsOwner)
            return;

        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if (!PlayerInputManager.instance.isChannelingSpell_Right)
            return;

        if (player.playerCombatManager.currentChannelingSpellManager != this)
            return;

        tickTimer += Time.deltaTime;

        if (tickTimer >= channelingSpell.tickInterval)
        {
            tickTimer = 0f;

            if (player.playerNetworkManager.currentFocusPoints.Value >= channelingSpell.focusCostPerTick)
            {
                player.playerNetworkManager.currentFocusPoints.Value -= channelingSpell.focusCostPerTick;
                ApplyChannelingDamage();
                if (player.playerNetworkManager.currentFocusPoints.Value <= 0)
                {
                    player.playerNetworkManager.currentFocusPoints.Value = 0;
                    player.playerCombatManager.currentChannelingSpell.StopChannelingSpell(player);
                }
            }
            else
            {
                player.playerCombatManager.currentChannelingSpell.StopChannelingSpell(player);
            }
        }
    }

    protected void ApplyChannelingDamage()
    {
        if (!player)
            return;

        if (!player.IsOwner)
            return;

        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if (!PlayerInputManager.instance.isChannelingSpell_Right)
            return;

        if (player.playerCombatManager.currentChannelingSpellManager != this)
            return;

        // 在这里添加对敌人的伤害逻辑
        RaycastHit[] hits = Physics.RaycastAll(startPoint.position, forward, currentLength, WorldUtilityManager.instance.GetCharacterLayers());
        if (hits.Length > 0)
        {
            List<CharacterManager> damagedTargets = new List<CharacterManager>();
            foreach (RaycastHit hit in hits)
            {
                CharacterManager character = hit.collider.GetComponentInParent<CharacterManager>();
                if (character != null && character != player)
                {
                    CheckForBlocking(hit, character, damagedTargets);
                    DamageTarget(hit, character, damagedTargets);
                }
            }
            damagedTargets.Clear();
        }
    }

    protected void DamageTarget(RaycastHit hit, CharacterManager damageTarget, List<CharacterManager> damagedTargets)
    {
        if (damagedTargets.Contains(damageTarget))
            return;

        damagedTargets.Add(damageTarget);

        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
        damageEffect.physicalDamage = spellSetup.physicalDamage;
        damageEffect.magicalDamage = spellSetup.magicalDamage;
        damageEffect.fireDamage = spellSetup.fireDamage;
        damageEffect.lightningDamage = spellSetup.lightningDamage;
        damageEffect.holyDamage = spellSetup.holyDamage;
        damageEffect.poiseDamage = spellSetup.poiseDamage;
        damageEffect.contactPoint = hit.point;
        damageEffect.angleHitFrom = Vector3.SignedAngle(player.transform.forward, damageTarget.transform.forward, Vector3.up);

        if (player.IsOwner)
        {
            // 只有攻击者的客户端才会处理伤害效果，其他客户端通过网络同步伤害结果
            //发送攻击请求
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                damageTarget.NetworkObjectId, player.NetworkObjectId,
                damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.holyDamage, damageEffect.lightningDamage,
                damageEffect.angleHitFrom, damageEffect.poiseDamage, damageEffect.contactPoint.x, damageEffect.contactPoint.y, damageEffect.contactPoint.z);
        }

        //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }

    protected virtual void CalculateDirectionToAttacker(CharacterManager damageTarget)
    {
        directionToAttacker = (player.transform.position - damageTarget.transform.position).normalized;
        dotFromDamageTargetToAttacker = Vector3.Dot(damageTarget.transform.forward, directionToAttacker);
    }

    protected virtual void CheckForBlocking(RaycastHit hit, CharacterManager damageTarget, List<CharacterManager> damagedTargets)
    {
        //如果已经被击中，直接不执行
        if (damagedTargets.Contains(damageTarget))
            return;

        CalculateDirectionToAttacker(damageTarget);

        if (damageTarget.characterNetworkManager.isBlocking.Value && dotFromDamageTargetToAttacker > 0.5f)
        {
            //如果是格挡状态，且攻击方向在角色前方，则不使用DmageEffect，而是使用BlockEffect
            damagedTargets.Add(damageTarget);

            TakeBlockedDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeBlockedDamageEffect);
            damageEffect.physicalDamage = spellSetup.physicalDamage;
            damageEffect.magicalDamage = spellSetup.magicalDamage;
            damageEffect.fireDamage = spellSetup.fireDamage;
            damageEffect.lightningDamage = spellSetup.lightningDamage;
            damageEffect.holyDamage = spellSetup.holyDamage;
            damageEffect.staminaCost = spellSetup.poiseDamage;
            damageEffect.poiseDamage = spellSetup.poiseDamage;
            damageEffect.contactPoint = hit.point;
            damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
        }
    }
}
