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
    public TakeDamageEffect takeDamageEffect;

    [SerializeField] private List<InstantCharacterEffect> instantEffects;
    
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
    }
}
