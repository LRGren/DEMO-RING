using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedProjectileDamageCollider : DamageCollider
{
    [Header("Marksmen")]
    public CharacterManager characterShootingProjectile;

    [Header("Collision")]
    public Rigidbody projectileRigidbody;
    public CapsuleCollider capsuleCollider;
    public bool hasPenetratedSurface = false;

    protected override void Awake()
    {
        base.Awake();

        projectileRigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        if (projectileRigidbody.velocity != Vector3.zero)
        {
            projectileRigidbody.rotation = Quaternion.LookRotation(projectileRigidbody.velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CreatePenetrationIntoObject(collision);

        CharacterManager potentialTarget = collision.gameObject.GetComponentInParent<CharacterManager>();
        contactPoint = collision.GetContact(0).point;

        // (TODO:) Check if the potential target is blocking the projectile, if so, do not apply damage to them

        // (TODO:) Instantiate a particle effect for the projectile hitting the target

        if (potentialTarget != null)
        {
            if (WorldUtilityManager.instance.CanIDamageThisTarget(characterShootingProjectile.characterGroup, potentialTarget.characterGroup))
            {
                CheckForBlocking(potentialTarget);
                DamageTarget(potentialTarget);
            }
        }

        // Destroy the projectile after it collides with something
        // Destroy(gameObject);
    }

    protected override void CheckForBlocking(CharacterManager damageTarget)
    {
        if (characterDamaged.Contains(damageTarget))
        {
            return;
        }

        float angle = Vector3.Angle(damageTarget.transform.forward, transform.forward);
        if (damageTarget.characterNetworkManager.isBlocking.Value && angle > 145)
        {
            characterDamaged.Add(damageTarget);

            TakeBlockedDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeBlockedDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicalDamage = magicalDamage;
            damageEffect.fireDamage = fireDamage;
            damageEffect.lightningDamage = lightningDamage;
            damageEffect.holyDamage = holyDamage;

            damageEffect.staminaCost = poiseDamage;

            damageEffect.poiseDamage = poiseDamage;

            damageEffect.contactPoint = contactPoint;

            damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
        }
    }

    private void CreatePenetrationIntoObject(Collision hit)
    {
        if (!hasPenetratedSurface)
            hasPenetratedSurface = true;

        // GET THE CONTACT POINT
        gameObject.transform.position = hit.GetContact(0).point;

        // STOPS OUR ARROW FROM "SCALING" IN SIZE WITH SCALED UP OR DOWN OBJECTS
        var emptyObject = new GameObject();
        emptyObject.transform.parent = hit.collider.transform;
        gameObject.transform.SetParent(emptyObject.transform, true);
        //HOW FAR THE ARROW PENETRATES INTO THE SURFACE
        transform.position += transform.forward * Random.Range(0.01f, 0.05f);

        //DISABLE COLLIDERS AND RIGIDBODY
        projectileRigidbody.isKinematic = true;
        capsuleCollider.enabled = false;

        //DESTROY DAMAGE COLLIDER, AND DESTROY ARROW AFTER A TIME
        Destroy(GetComponent<RangedProjectileDamageCollider>());
        Destroy(gameObject, 20);
    }

    protected override void DamageTarget(CharacterManager damageTarget)
    {
        if (characterDamaged.Contains(damageTarget))
            return;

        characterDamaged.Add(damageTarget);

        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicalDamage = magicalDamage;
        damageEffect.fireDamage = fireDamage;
        damageEffect.lightningDamage = lightningDamage;
        damageEffect.holyDamage = holyDamage;

        damageEffect.poiseDamage = poiseDamage;

        damageEffect.contactPoint = contactPoint;

        damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);

        if (characterShootingProjectile.IsOwner)
        {
            // 只有攻击者的客户端才会处理伤害效果，其他客户端通过网络同步伤害结果
            //发送攻击请求
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                damageTarget.NetworkObjectId, characterShootingProjectile.NetworkObjectId,
                damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.holyDamage, damageEffect.lightningDamage,
                damageEffect.angleHitFrom, damageEffect.poiseDamage, damageEffect.contactPoint.x, damageEffect.contactPoint.y, damageEffect.contactPoint.z);
        }
    }

}
