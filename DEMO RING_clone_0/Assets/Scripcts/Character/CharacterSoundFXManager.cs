using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSoundFXManager : MonoBehaviour
{
    [Header("Damage Grunt SFX")]
    public AudioClip[] damageGrunts;

    [Header("Attack Grunt SFX")]
    public AudioClip[] attackGrunts;

    [Header("Footstep SFX")]
    public AudioClip[] footstepSFX;
    public AudioClip[] footstepSFXGrass;
    public AudioClip[] footstepSFXDirt;
    public AudioClip[] footstepSFXStone;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundFX(AudioClip soundFX, float volume = 1f, bool randomizePitch = true, float randomPitchRange = 0.1f)
    {
        audioSource.PlayOneShot(soundFX, volume);

        audioSource.pitch = 1;

        if (randomizePitch)
        {
            audioSource.pitch += UnityEngine.Random.Range(-randomPitchRange, randomPitchRange);
        }
    }

    public void PlayRollSFX()
    {
        audioSource.PlayOneShot(WorldSoundFXManager.instance.rollSFX);
    }

    public virtual void PlayDamageGruntSFX()
    {
        if (damageGrunts.Length == 0)
            return;

        AudioClip gruntClip = WorldSoundFXManager.instance.ChooseRandomSFXFromArray(damageGrunts);
        PlaySoundFX(gruntClip);
    }

    public virtual void PlayAttackGruntSFX()
    {
        if (attackGrunts.Length == 0)
            return;

        AudioClip gruntClip = WorldSoundFXManager.instance.ChooseRandomSFXFromArray(attackGrunts);
        PlaySoundFX(gruntClip);
    }

    public virtual void PlayFootstepSFX(GameObject surfaceObject, CharacterManager character)
    {
        if (footstepSFX.Length == 0)
            return;

        AudioClip footstepClip = WorldSoundFXManager.instance.ChooseRandomFootstepSFXBasedOnSurfaceType(surfaceObject, character);

        PlaySoundFX(footstepClip, 0.2f);
    }

    public virtual void PlayBlockSFX()
    {

    }
}
