using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpellManager : SpellManager
{
    [Header("Colliders")]
    public ProjectileSpellDamageCollider _damageCollider;

    [Header("Instantiated FX")]
    protected GameObject instantiatedImpactParticle;

    public bool isFullCharge = false;

    protected bool hasCollided = false;
    protected Rigidbody fireBallRigidbody;
    protected Coroutine destructionCoroutine;


    protected override void Awake()
    {
        base.Awake();
        fireBallRigidbody = GetComponent<Rigidbody>();
        _damageCollider = GetComponentInChildren<ProjectileSpellDamageCollider>();
    }

    protected override void Update()
    {
        base.Update();

        if (spellTarget != null)
            transform.LookAt(spellTarget.characterCombatManager.lockOnTransform.position);

        /*if (fireBallRigidbody != null)
        {
            Vector3 currentVelocity = fireBallRigidbody.velocity;
            fireBallRigidbody.velocity = transform.forward + currentVelocity;
        }*/
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
            return;

        if (!hasCollided)
        {
            hasCollided = true;
            InstantiateImpactParticleFX();
        }
    }

    public virtual void InitializeProjectileSpell(CharacterManager caster)
    {
        _damageCollider.spellCaster = caster;

        var multiplier = isFullCharge ? 3f : 1f;

        _damageCollider.fireDamage = spellSetup.fireDamage * multiplier;
        _damageCollider.magicalDamage = spellSetup.magicalDamage * multiplier;
        _damageCollider.lightningDamage = spellSetup.lightningDamage * multiplier;
        _damageCollider.holyDamage = spellSetup.holyDamage * multiplier;
        _damageCollider.poiseDamage = spellSetup.poiseDamage * multiplier;
        _damageCollider.physicalDamage = spellSetup.physicalDamage * multiplier;
    }

    protected virtual void InstantiateImpactParticleFX()
    {
        if (isFullCharge)
            instantiatedImpactParticle = Instantiate(impactParticleFullCharge, transform.position, Quaternion.identity);
        else
            instantiatedImpactParticle = Instantiate(impactParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public virtual void WaitThenInstantiateSpellDestructionFX(float timeToWait)
    {

        if (destructionCoroutine != null)
            StopCoroutine(destructionCoroutine);

        destructionCoroutine = StartCoroutine(WaitThenInstantiateFX(timeToWait));
        StartCoroutine(WaitThenInstantiateFX(timeToWait));
    }
    protected virtual IEnumerator WaitThenInstantiateFX(float timeToWait)
    {

        yield return new WaitForSeconds(timeToWait);

        InstantiateImpactParticleFX();
    }
}
