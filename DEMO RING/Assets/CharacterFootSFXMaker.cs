using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFootSFXMaker : MonoBehaviour
{
    [SerializeField] private bool playSoundFXBasedOnSurfaceType = true;
    [SerializeField] private AudioClip[] selfFootstepSFX;


    private CharacterManager characterManager;

    [SerializeField]
    private AudioSource audioSource;
    private GameObject steppedOnObject;

    private bool hasSteppedOnObject = false;
    private bool hasPlayedSoundFX = false;

    [SerializeField] private float footstepCheckDistance = 0.05f;

    private void Awake()
    {
        characterManager = GetComponentInParent<CharacterManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        CheckForFootSteps();
    }

    private void CheckForFootSteps()
    {
        if (characterManager == null || audioSource == null)
            return;

        if (!characterManager.characterNetworkManager.isMoving.Value)
            return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, characterManager.transform.TransformDirection(Vector3.down), out hit, footstepCheckDistance, WorldUtilityManager.instance.GetEnviroLayers()))
        {
            hasSteppedOnObject = true;

            if (!hasPlayedSoundFX)
            {
                steppedOnObject = hit.collider.gameObject;
            }
        }
        else
        {
            hasSteppedOnObject = false;
            hasPlayedSoundFX = false;
            steppedOnObject = null;
        }

        if (hasSteppedOnObject && !hasPlayedSoundFX && steppedOnObject != null)
        {
            hasPlayedSoundFX = true;

            if (playSoundFXBasedOnSurfaceType)
                characterManager.characterSoundFXManager.PlayFootstepSFX(steppedOnObject, characterManager);
            else
                characterManager.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(selfFootstepSFX), 0.5f);
        }
    }
}
