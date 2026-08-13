using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSoundFXManager : MonoBehaviour
{
    public static WorldSoundFXManager instance;

    [Header("Boss Track")]
    public AudioSource bossIntroPlayer;
    public AudioSource bossLoopPlayer;


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

    public void PlayBossTrack(AudioClip bossIntro, AudioClip bossLoop)
    {
        bossIntroPlayer.volume = 1f;
        bossIntroPlayer.clip = bossIntro;
        bossIntroPlayer.loop = false;
        bossIntroPlayer.Play();

        bossLoopPlayer.volume = 1f;
        bossLoopPlayer.clip = bossLoop;
        bossLoopPlayer.loop = true;
        bossLoopPlayer.PlayDelayed(bossIntroPlayer.clip.length);
    }

    public void StopBossTrack()
    {
        StartCoroutine(FadeOutAudioSourceOverTime());
    }

    private IEnumerator FadeOutAudioSourceOverTime()
    {
        while (bossLoopPlayer.volume > 0)
        {
            bossLoopPlayer.volume -= Time.deltaTime;
            bossIntroPlayer.volume -= Time.deltaTime;
            yield return null;
        }

        bossLoopPlayer.Stop();
        bossIntroPlayer.Stop();
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
