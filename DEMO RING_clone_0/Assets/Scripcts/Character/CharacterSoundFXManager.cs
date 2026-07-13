using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundFXManager : MonoBehaviour
{
    [Header("Damage Grunt SFX")]
    public AudioClip[] damageGrunts;

    [Header("Attack Grunt SFX")]
    public AudioClip[] attackGrunts;

    private AudioSource audioSource;

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
        AudioClip gruntClip = WorldSoundFXManager.instance.ChooseRandomSFXFromArray(damageGrunts);
        PlaySoundFX(gruntClip);
    }

    public virtual void PlayAttackGruntSFX()
    {
        AudioClip gruntClip = WorldSoundFXManager.instance.ChooseRandomSFXFromArray(attackGrunts);
        PlaySoundFX(gruntClip);
    }
}
