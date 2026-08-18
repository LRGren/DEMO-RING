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
        return Physics.Linecast(fromPosition, toPosition, enviroLayers);
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
}
