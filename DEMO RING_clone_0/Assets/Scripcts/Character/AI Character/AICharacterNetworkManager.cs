using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AICharacterNetworkManager : CharacterNetworkManager
{
    private const string ActionLayer = "Action Override";
    private static readonly int DefaultDeathState = Animator.StringToHash(ActionLayer + ".Death_01");
    private static readonly int BackstabDeathState = Animator.StringToHash(ActionLayer + ".core_backstab_death_01");
    private static readonly int RiposteDeathState = Animator.StringToHash(ActionLayer + ".core_riposte_death_01");
    private static readonly int CrucibleRiposteDeathState = Animator.StringToHash(ActionLayer + ".c2500_riposted_death");

    private readonly NetworkVariable<int> deathState = new();
    private int pendingCriticalDeathState;
    private AICharacterManager aiCharacterManager;

    protected override void Awake()
    {
        base.Awake();

        aiCharacterManager = GetComponent<AICharacterManager>();
    }

    public override void OnIsDeadChanged(bool old, bool newStatus)
    {
        base.OnIsDeadChanged(old, newStatus);

        if (!IsOwner)
            return;

        if (!newStatus)
        {
            deathState.Value = 0;
            pendingCriticalDeathState = 0;
            return;
        }

        deathState.Value = isBeingRiposted.Value ? pendingCriticalDeathState : 0;
        aiCharacterManager.aiCharacterInventoryManager.DropItem();
    }

    public void SetPendingCriticalDeathState(bool backstab)
    {
        var animator = aiCharacterManager.animator;
        int layer = animator.GetLayerIndex(ActionLayer);
        pendingCriticalDeathState = backstab ? BackstabDeathState : RiposteDeathState;
        if (!backstab && layer >= 0 && !animator.HasState(layer, RiposteDeathState))
            pendingCriticalDeathState = CrucibleRiposteDeathState;
    }

    public void RestoreDeathPose()
    {
        if (!aiCharacterManager.isDead.Value)
            return;

        var animator = aiCharacterManager.animator;
        animator.SetBool("isDead", true);
        int layer = animator.GetLayerIndex(ActionLayer);
        if (layer < 0)
            return;

        int state = deathState.Value;
        if (state == 0 || !animator.HasState(layer, state))
            state = DefaultDeathState;

        if (animator.HasState(layer, state))
            animator.Play(state, layer, 1f);
    }
}
