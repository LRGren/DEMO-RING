using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallManager : SpellManager
{
    // - What does this script do? - This script serves as a central hub to manipulate and adjust the fireball spell once its active, doing things such as
    // 1. Making the spell slightly "curve" or "follow" its lock targets as they are moving
    // 2. Assigning damage neatly with a function from this script
    // 3. Enabling/Disabling VFX and SFX, such as "contact" explosions, trails, ect

    // OPTIONAL THINGS TO DO: IF YOU HAVE MANY SPELLS THAT SHARE THIS LOGIC (Spell Target, Collider, Impact Particles ect) MAKE A BASE CLASS

    [Header("Colliders")]
    public FireBallDamageCollider damageCollider;

    [Header("Instantiated FX")]
    private GameObject instantiatedImpactParticle;

    public bool isFullCharge = false;

    private bool hasCollided = false;
    private Rigidbody fireBallRigidbody;
    private Coroutine destructionCoroutine;


    protected override void Awake()
    {
        base.Awake();
        fireBallRigidbody = GetComponent<Rigidbody>();
        damageCollider = GetComponentInChildren<FireBallDamageCollider>();
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
            return;

        if (!hasCollided)
        {
            hasCollided = true;
            InstantiateImpactParticleFX();
        }
    }

    public void InitializeFireBall(CharacterManager caster)
    {
        damageCollider.spellCaster = caster;
        damageCollider.fireDamage = 150;

        if (isFullCharge)
            damageCollider.fireDamage *= 3f; // Increase damage by 50% if fully charged
    }

    public void InstantiateImpactParticleFX()
    {
        if (isFullCharge)
            instantiatedImpactParticle = Instantiate(impactParticleFullCharge, transform.position, Quaternion.identity);
        else
            instantiatedImpactParticle = Instantiate(impactParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void WaitThenInstantiateSpellDestructionFX(float timeToWait)
    {

        if (destructionCoroutine != null)
            StopCoroutine(destructionCoroutine);

        destructionCoroutine = StartCoroutine(WaitThenInstantiateFX(timeToWait));
        StartCoroutine(WaitThenInstantiateFX(timeToWait));
    }
    private IEnumerator WaitThenInstantiateFX(float timeToWait)
    {

        yield return new WaitForSeconds(timeToWait);

        InstantiateImpactParticleFX();
    }

}
