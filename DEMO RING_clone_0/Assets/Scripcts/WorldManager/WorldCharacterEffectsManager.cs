using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCharacterEffectsManager : MonoBehaviour
{
    public static WorldCharacterEffectsManager instance;

    [Header("VFX")]
    public GameObject bloodSplatterVFX;

    [Header("Damage")]
    public TakeStaminaDamageEffect takeStaminaDamageEffect;
    public TakeDamageEffect takeDamageEffect;
    public TakeBlockedDamageEffect takeBlockedDamageEffect;

    [Header("Two Handing")]
    public TwoHandingEffect twoHandingEffect;

    [Header("Instant Effects")]
    [SerializeField] private List<InstantCharacterEffect> instantEffects;
    [Header("Static Effects")]
    [SerializeField] private List<StaticCharacterEffect> staticEffects;

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

        GenerateEffectsIDs();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void GenerateEffectsIDs()
    {
        for (int i = 0; i < instantEffects.Count; i++)
        {
            instantEffects[i].instantEffectID = i;
        }
        for (int i = 0; i < staticEffects.Count; i++)
        {
            staticEffects[i].staticEffectID = i;
        }
    }
}
