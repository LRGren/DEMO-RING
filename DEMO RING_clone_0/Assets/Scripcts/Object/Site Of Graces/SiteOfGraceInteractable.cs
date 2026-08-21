using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SiteOfGraceInteractable : Interactable
{
    [Header("Site Of Grace Info")]
    [SerializeField] private int siteOfGraceID;
    public NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Particle Effects")]
    [SerializeField] private GameObject activatedParticleEffect;


    [Header("Site Of Grace Text")]
    [SerializeField] private string unactivatedSiteOfGraceText = "Press Y To Restore The Site Of Grace";
    [SerializeField] private string activatedSiteOfGraceText = "Press Y To Rest At The Site Of Grace";

    protected override void Start()
    {
        base.Start();

        if (IsOwner)
        {
            if (WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.ContainsKey(siteOfGraceID))
            {
                isActivated.Value = WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated[siteOfGraceID];
            }
            else
            {
                isActivated.Value = false;
            }
        }

        if (isActivated.Value)
        {
            activatedParticleEffect.SetActive(true);
            InteractableText = activatedSiteOfGraceText;
        }
        else
        {
            activatedParticleEffect.SetActive(false);
            InteractableText = unactivatedSiteOfGraceText;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            OnIsActivatedChanged(false, isActivated.Value);
        }

        isActivated.OnValueChanged += OnIsActivatedChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        isActivated.OnValueChanged -= OnIsActivatedChanged;
    }

    private void OnIsActivatedChanged(bool previousValue, bool newValue)
    {
        if (isActivated.Value)
        {
            activatedParticleEffect.SetActive(true);
            InteractableText = activatedSiteOfGraceText;
        }
        else
        {
            activatedParticleEffect.SetActive(false);
            InteractableText = unactivatedSiteOfGraceText;
        }
    }

    public void RestoreSiteOfGrace(PlayerManager player)
    {
        //Add the site of grace to the player's activated list
        isActivated.Value = true;
        if (WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.ContainsKey(siteOfGraceID))
            WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.Remove(siteOfGraceID);

        WorldSaveGameManager.instance.currentCharacterData.siteOfGraceActivated.Add(siteOfGraceID, true);

        //Player An Animation
        player.playerAnimatorManager.PlayerTargetActionAnimation("Activate_Site_Of_Grace_01", true);

        player.characterSoundFXManager.PlaySoundFX(player.characterSoundFXManager.restoreSiteOfGraceSFX);

        //Sends A Pop Up To The Player Saying "Site Of Grace Restored"
        PlayerUIManager.instance.playerUIPopUpManager.SendSiteOfGraceActivatedPopUp("Site Of Grace Restored");

        StartCoroutine(WaitForAnimationToFinish(player));
    }

    public void RestAtSiteOfGrace(PlayerManager player)
    {
        Debug.Log("Player is resting at the Site of Grace");
        //Play The Rest Animation
        //Update/Force Move Quest Character
        //Reset The Monsters In The Area

        //Reset The Player's Health And Stamina
        player.playerNetworkManager.currentHealth.Value = player.playerNetworkManager.maxHealth.Value;
        player.playerNetworkManager.currentStamina.Value = player.playerNetworkManager.maxStamina.Value;

        WorldSaveGameManager.instance.SaveGame();

        interactableCollider.enabled = true;

        WorldAIManager.instance.RestAllCharacters();
    }

    private IEnumerator WaitForAnimationToFinish(PlayerManager player)
    {
        Animator animator = player.animator;

        // 等 CrossFade 切到目标状态（跳过 0.2s 过渡）
        yield return null;

        while (true)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            // normalizedTime >= 1 表示播完（若循环则用 % 1 判断）
            if (state.normalizedTime >= 1f && !animator.IsInTransition(0))
                break;
            yield return null;
        }

        // 动画结束后执行后续逻辑
        interactableCollider.enabled = true;
    }

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);

        if (isActivated.Value)
        {
            RestAtSiteOfGrace(player);
        }
        else
        {
            RestoreSiteOfGrace(player);
        }
    }

}
