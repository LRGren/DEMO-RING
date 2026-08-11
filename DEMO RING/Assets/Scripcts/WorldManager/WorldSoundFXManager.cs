using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSoundFXManager : MonoBehaviour
{
    public static WorldSoundFXManager instance;

    [Header("Damage Sounds")]
    public AudioClip[] physicalDamageSFX;

    [Header("Action Sounds")]
    public AudioClip rollSFX;

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

    public AudioClip ChooseRandomSFXFromArray(AudioClip[] array)
    {
        int idx = Random.Range(0, array.Length);
        return array[idx];
    }

    public AudioClip ChooseRandomFootstepSFXBasedOnSurfaceType(GameObject surfaceObject, CharacterManager character)
    {
        if (surfaceObject == null || character == null)
            return null;

        string surfaceTag = surfaceObject.tag;

        switch (surfaceTag)
        {
            case "Grass":
                return ChooseRandomSFXFromArray(character.characterSoundFXManager.footstepSFXGrass);
            case "Dirt":
                return ChooseRandomSFXFromArray(character.characterSoundFXManager.footstepSFXDirt);
            case "Stone":
                return ChooseRandomSFXFromArray(character.characterSoundFXManager.footstepSFXStone);
            default:
                return ChooseRandomSFXFromArray(character.characterSoundFXManager.footstepSFX);
        }
    }

}
