using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotItemProjectileManager : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Header("Visual Effects")]
    public GameObject impactParticle;

    [Header("Damage Settings")]
    public CharacterManager projectileTarget;
    protected QuickSlotItemProjectileDamageCollider _damageCollider;
    protected Coroutine destructionCoroutine;
    protected GameObject instantiatedImpactParticle;

    public int physicalDamage;
    public int fireDamage;
    public int magicalDamage;
    public int lightningDamage;
    public int holyDamage;
    public int poiseDamage;

    [Header("Motion Settings")]
    public bool hasCollided = false;
    public Collider projectileCollider;
    protected Rigidbody projectileRigidbody;
    protected virtual void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();
        _damageCollider = GetComponentInChildren<QuickSlotItemProjectileDamageCollider>();
    }

    protected virtual void Update()
    {
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided)
        {
            hasCollided = true;
            WaitThenDestroyProjectile(0.1f);
        }
    }
    private void OpenDamageCollider()
    {
        if (_damageCollider != null)
            _damageCollider.damageCollider.enabled = true;
    }
    public virtual void InitializeProjectile(CharacterManager user)
    {
        _damageCollider.user = user;

        _damageCollider.fireDamage = fireDamage;
        _damageCollider.magicalDamage = magicalDamage;
        _damageCollider.lightningDamage = lightningDamage;
        _damageCollider.holyDamage = holyDamage;
        _damageCollider.poiseDamage = poiseDamage;
        _damageCollider.physicalDamage = physicalDamage;
    }

    public virtual void WaitThenDestroyProjectile(float timeToWait)
    {
        if (destructionCoroutine != null)
            StopCoroutine(destructionCoroutine);

        destructionCoroutine = StartCoroutine(WaitThenInstantiateFX(timeToWait));
        StartCoroutine(WaitThenInstantiateFX(timeToWait));
    }
    protected virtual IEnumerator WaitThenInstantiateFX(float timeToWait)
    {

        yield return new WaitForSeconds(timeToWait);
        OpenDamageCollider();
        yield return new WaitForFixedUpdate();
        InstantiateImpactParticleFX();
    }

    protected virtual void InstantiateImpactParticleFX()
    {
        instantiatedImpactParticle = Instantiate(impactParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
