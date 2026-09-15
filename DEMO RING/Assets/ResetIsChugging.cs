using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetIsChugging : StateMachineBehaviour
{
    PlayerManager player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
            player = animator.GetComponentInParent<PlayerManager>();

        if (player == null)
            return;

        // 如果玩家正在使用药水，并且是本地玩家，药水喝完了，则将手中的药水替换为空药水
        if (player.playerNetworkManager.isChugging.Value)
        {
            FlaskItem flaskItem = player.playerInventoryManager.currentQuickSlotItem as FlaskItem;
            if (flaskItem == null)
                return;

            if (player.playerNetworkManager.remainingManaFlasks.Value <= 0 && !flaskItem.isHealthFlask)
            {
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX.gameObject);
                GameObject emptyFlask = Instantiate(flaskItem.emptyFlaskPrefab, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;

                if (player.IsOwner)
                {
                    player.playerAnimatorManager.PlayerTargetActionAnimation(flaskItem.useEmptyFlaskAnimation, false, false, true, true, false);
                    player.playerNetworkManager.HideWeaponsServerRpc();
                }
            }
            else if (player.playerNetworkManager.remainingHealthFlasks.Value <= 0 && flaskItem.isHealthFlask)
            {
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX.gameObject);
                GameObject emptyFlask = Instantiate(flaskItem.emptyFlaskPrefab, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;

                if (player.IsOwner)
                {
                    player.playerAnimatorManager.PlayerTargetActionAnimation(flaskItem.useEmptyFlaskAnimation, false, false, true, true, false);
                    player.playerNetworkManager.HideWeaponsServerRpc();
                }
            }
        }

        if (player.playerNetworkManager.isChugging.Value)
        {
            player.playerNetworkManager.isChugging.Value = false;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
