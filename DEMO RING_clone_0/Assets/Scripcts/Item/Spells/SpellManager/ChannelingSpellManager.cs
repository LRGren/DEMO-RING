using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelingSpellManager : SpellManager
{
    public PlayerManager player;
    protected List<LineRenderer> lineRenderer;

    [Header("Channeling Spell Settings")]
    public ChannelingSpell channelingSpell;
    public int channelLength = 15;
    protected float currentLength = 0f;
    public Transform startPoint;
    public Vector3 forward;
    public GameObject beamSource;

    [Header("Glow")]
    public LineRenderer glow_LineRenderer;
    public Material glow_Material;
    public float glow_RunningSpeed = -.5f;
    public float glow_Offset = 0f;

    private ParticleSystem impactGlow;

    [Header("Enemy Hit VFX")]
    [SerializeField] private ParticleSystem enemyHitEffectPrefab;
    private float enemyHitEffectTimer;
    private readonly HashSet<CharacterManager> hitEffectTargets = new HashSet<CharacterManager>();

    protected float tickTimer = 0f;

    [Header("Damage")]
    protected Vector3 directionToAttacker;
    protected float dotFromDamageTargetToAttacker;

    protected override void Awake()
    {
        base.Awake();

        var lineRenderers = GetComponentsInChildren<LineRenderer>();
        lineRenderer = new List<LineRenderer>(lineRenderers);

        if (glow_LineRenderer != null)
        {
            glow_Material = glow_LineRenderer.material;
        }

        if (impactParticle != null)
        {
            impactGlow = impactParticle.GetComponent<ParticleSystem>();
            StopImpactGlow();
        }
    }

    protected override void Update()
    {
        base.Update();

        UpdateChannelingSpell();
        UpdateEnemyHitEffects();
        ContinuouslyTakeEffect();
        GlowEffect();
    }


    private void GlowEffect()
    {
        glow_Offset += Time.deltaTime * glow_RunningSpeed;
        glow_Material.SetTextureOffset("_BaseMap", new Vector2(glow_Offset, 0f));
    }

    private void UpdateImpactGlow(bool hasHit, RaycastHit hit)
    {
        if (impactGlow == null)
            return;

        if (!hasHit)
        {
            StopImpactGlow();
            return;
        }

        // 稍微离开表面，避免光晕被墙面遮住。
        impactGlow.transform.SetPositionAndRotation(hit.point + hit.normal * 0.02f,
            Quaternion.LookRotation(hit.normal));

        if (!impactGlow.isPlaying)
            impactGlow.Play(true);
    }

    private void StopImpactGlow()
    {
        if (impactGlow != null && (!impactGlow.isStopped || impactGlow.particleCount > 0))
            impactGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDisable()
    {
        StopImpactGlow();
    }

    private void UpdateEnemyHitEffects()
    {
        if (enemyHitEffectPrefab == null || player == null || channelingSpell == null)
            return;

        enemyHitEffectTimer -= Time.deltaTime;
        if (enemyHitEffectTimer > 0f)
            return;

        enemyHitEffectTimer = Mathf.Max(0.05f, channelingSpell.tickInterval);
        hitEffectTargets.Clear();

        // 每个客户端只播放本地特效，伤害仍由原来的 Owner 流程处理。
        RaycastHit[] hits = Physics.SphereCastAll(startPoint.position, spellSetup.attackRadius, forward, currentLength, WorldUtilityManager.instance.GetCharacterLayers());
        foreach (RaycastHit hit in hits)
        {
            CharacterManager target = hit.collider.GetComponentInParent<CharacterManager>();
            if (target == null || target == player || target.isDead.Value ||
                target.characterNetworkManager.isInvulnerable.Value ||
                !WorldUtilityManager.instance.CanIDamageThisTarget(player.characterGroup, target.characterGroup) ||
                !hitEffectTargets.Add(target))
                continue;

            PlayEnemyHitEffect(hit.point, hit.normal);
        }
    }

    private void PlayEnemyHitEffect(Vector3 point, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        ParticleSystem effect = Instantiate(enemyHitEffectPrefab, point + normal * 0.15f,
            rotation, transform);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 大光晕配合散开的光点，让命中效果露出光束轮廓。
        var main = effect.main;
        main.loop = false;
        main.duration = 0.3f;
        main.startLifetime = 0.3f;
        main.startSize = 1.6f;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = effect.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[0]);

        var fade = effect.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.2f),
                new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        var size = effect.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.65f, 1f, 1.25f));

        var renderer = effect.GetComponent<ParticleSystemRenderer>();
        var properties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(properties);
        properties.SetColor("_EmissionColor", new Color(1.5f, 3f, 6f, 1f));
        renderer.SetPropertyBlock(properties);
        renderer.sortingOrder = 3;

        effect.Play(true);
        effect.Emit(1);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 0.25f;
            Vector3 radial = rotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            var spark = new ParticleSystem.EmitParams
            {
                position = effect.transform.position + radial * 0.12f,
                velocity = radial * Random.Range(1.5f, 2.5f) + normal * 0.5f,
                startSize = Random.Range(0.16f, 0.24f),
                startLifetime = Random.Range(0.2f, 0.3f)
            };
            effect.Emit(spark, 1);
        }
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
        foreach (var lr in lineRenderer)
        {
            lr.SetPosition(0, startPoint.transform.position);
        }

        forward = new Vector3(player.transform.forward.x, 0f, player.transform.forward.z).normalized;
        beamSource.transform.position = startPoint.transform.position;

        Vector3 endPosition = startPoint.position + forward * channelLength;

        RaycastHit hit;
        bool hasHit = Physics.Raycast(startPoint.position, forward, out hit, channelLength, WorldUtilityManager.instance.GetEnviroLayers());
        if (hasHit)
        {
            endPosition = hit.point;
        }

        foreach (var lr in lineRenderer)
        {
            lr.SetPosition(1, endPosition);
        }
        currentLength = Vector3.Distance(startPoint.position, endPosition);
        UpdateImpactGlow(hasHit, hit);
    }
    protected void ContinuouslyTakeEffect()
    {
        if (!player)
            return;

        if (!player.IsOwner)
            return;

        if (!player.playerCombatManager.isChannelingSpell)
            return;

        if ((!PlayerInputManager.instance.isChannelingSpell_Right && player.playerCombatManager.isChannelingRightHand) || (!PlayerInputManager.instance.isChannelingSpell_Left && !player.playerCombatManager.isChannelingRightHand))
            return;

        if (player.playerCombatManager.currentChannelingSpellManager != this)
            return;

        tickTimer += Time.deltaTime;

        if (tickTimer >= channelingSpell.tickInterval && player.playerNetworkManager.currentFocusPoints.Value > 0 && channelingSpell.focusCostPerTick > 0)
        {
            tickTimer -= channelingSpell.tickInterval;

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

        if ((!PlayerInputManager.instance.isChannelingSpell_Right && player.playerCombatManager.isChannelingRightHand) || (!PlayerInputManager.instance.isChannelingSpell_Left && !player.playerCombatManager.isChannelingRightHand))
            return;

        if (player.playerCombatManager.currentChannelingSpellManager != this)
            return;

        // 在这里添加对敌人的伤害逻辑
        RaycastHit[] hits = Physics.SphereCastAll(startPoint.position, spellSetup.attackRadius, forward, currentLength, WorldUtilityManager.instance.GetCharacterLayers());
        if (hits.Length > 0)
        {
            List<CharacterManager> damagedTargets = new List<CharacterManager>();
            foreach (RaycastHit hit in hits)
            {
                CharacterManager character = hit.collider.GetComponentInParent<CharacterManager>();
                if (character != null && character != player)
                {
                    if (character.isDead.Value || character.characterNetworkManager.isInvulnerable.Value
                        || !WorldUtilityManager.instance.CanIDamageThisTarget(player.characterGroup, character.characterGroup))
                        continue;

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
        directionToAttacker = (startPoint.position - damageTarget.transform.position).normalized;
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

            damageTarget.characterNetworkManager.RequestBlockedDamage(player,
                spellSetup.physicalDamage, spellSetup.magicalDamage, spellSetup.fireDamage,
                spellSetup.holyDamage, spellSetup.lightningDamage,
                spellSetup.poiseDamage, spellSetup.poiseDamage,
                Vector3.SignedAngle(player.transform.forward, damageTarget.transform.forward, Vector3.up), hit.point);
        }
    }
}
