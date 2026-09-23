using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUtilityManager : MonoBehaviour
{
    public static WorldUtilityManager instance;

    [Header("Layers")]
    [SerializeField] private LayerMask characterLayers;
    [SerializeField] private LayerMask enviroLayers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public LayerMask GetCharacterLayers()
    {
        return characterLayers;
    }

    public LayerMask GetEnviroLayers()
    {
        return enviroLayers;
    }

    public bool CanIDamageThisTarget(CharacterGroup attacker, CharacterGroup target)
    {
        if (attacker == CharacterGroup.Team_01)
        {
            switch (target)
            {
                case CharacterGroup.Team_01: return false;
                case CharacterGroup.Team_02: return true;
                default:
                    break;
            }
        }
        else if (attacker == CharacterGroup.Team_02)
        {
            switch (target)
            {
                case CharacterGroup.Team_01: return true;
                case CharacterGroup.Team_02: return false;
                default:
                    break;
            }
        }
        return false;
    }

    public float GetAngleOfTarget(Transform transform, Vector3 targetDirection)
    {
        float viewableAngle = Vector3.Angle(transform.forward, targetDirection);
        Vector3 cross = Vector3.Cross(transform.forward, targetDirection);

        if (cross.y < 0)
        {
            viewableAngle = -viewableAngle;
        }

        return viewableAngle;
    }

    public bool IsTargetBlockedByEnvironment(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector3 direction = toPosition - fromPosition;
        if (direction.sqrMagnitude <= 0.0001f)
            return false;

        if (Physics.Linecast(fromPosition, toPosition, out RaycastHit hit, enviroLayers))
        {
            CharacterManager hitCharacter = hit.collider.GetComponentInParent<CharacterManager>();
            return hitCharacter == null;
        }

        return false;
    }

    public DamageIntensity GetDamageIntensityBasedOnPoiseDamage(float poiseDamage)
    {
        if (poiseDamage < 10)
        {
            return DamageIntensity.Ping;
        }
        else if (poiseDamage < 30)
        {
            return DamageIntensity.Light;
        }
        else if (poiseDamage < 70)
        {
            return DamageIntensity.Medium;
        }
        else if (poiseDamage < 120)
        {
            return DamageIntensity.Heavy;
        }
        else
        {
            return DamageIntensity.Colossal;
        }
    }

    public float GetCriticalAttackDamageMultiplierBasedOnWeaponClass(WeaponClass weaponClass)
    {
        switch (weaponClass)
        {
            case WeaponClass.StraightSword:
                return 3.45f;
            case WeaponClass.MediumShield:
                return 2.0f;
            default:
                return 2f;
        }
    }

    public float GetBackstabAttackDamageMultiplierBasedOnWeaponClass(WeaponClass weaponClass)
    {
        switch (weaponClass)
        {
            case WeaponClass.StraightSword:
                return 2.3f;
            case WeaponClass.MediumShield:
                return 1.0f;
            default:
                return 2f;
        }
    }


    public Vector3 GetRipostingPositionBasedOnWeaponClass(WeaponClass weaponClass)
    {

        Vector3 position = new Vector3(0.11f, 0, 0.7f);

        switch (weaponClass)
        {
            case WeaponClass.StraightSword:
                break;
            case WeaponClass.MediumShield:
                break;
            default:
                break;

        }

        return position;
    }

    public Vector3 GetBackstabbingPositionBasedOnWeaponClass(WeaponClass weaponClass)
    {

        Vector3 position = new Vector3(0.12f, 0, 0.74f);

        switch (weaponClass)
        {
            case WeaponClass.StraightSword:
                break;
            case WeaponClass.MediumShield:
                break;
            default:
                break;

        }

        return position;
    }

}

