using UnityEngine;

[CreateAssetMenu(fileName = "New Fire Pot", menuName = "Quick Slot Item/Fire Pot")]
public class FirePot : QuickSlotItem
{
    [Header("Fire Pot Settings")]
    public GameObject firePotPrefab;
    public float airTime = .5f;
    [SerializeField, Min(1f)] private float freeAimRange = 12f;
    [SerializeField] private LayerMask aimMask = Physics.DefaultRaycastLayers;

    public override void AttemptToUseItem(PlayerManager player)
    {
        if (!CanIUseThisItem(player))
            return;

        if (GetAmountOfItem(player) <= 0)
            return;

        player.playerCombatManager.isUsingItem = true;

        // player.playerEffectsManager.activeQuickSlotItemFX = Instantiate(itemModel, player.playerEquipmentManager.rightHandWeaponSlot.transform);

        if (player.IsOwner)
        {
            player.playerAnimatorManager.PlayerTargetActionAnimation(useItemAnimation, true);
            player.playerNetworkManager.HideWeaponsServerRpc();
        }
    }

    public override void SuccessfullyUsedItem(PlayerManager player)
    {
        // 1. 销毁WarmUpFX
        if (player.IsOwner)
            player.playerCombatManager.DestoryAllAttemptedActions();

        Transform projectileSpawnPoint = player.playerEquipmentManager.rightHandWeaponManager.transform;
        GameObject instantiatedReleasedFX = Instantiate(firePotPrefab);
        instantiatedReleasedFX.transform.parent = projectileSpawnPoint;
        instantiatedReleasedFX.transform.localPosition = Vector3.zero;
        instantiatedReleasedFX.transform.localRotation = Quaternion.identity;
        instantiatedReleasedFX.transform.parent = null;

        QuickSlotItemProjectileManager projectileManager = instantiatedReleasedFX.GetComponent<QuickSlotItemProjectileManager>();
        projectileManager.InitializeProjectile(player);

        Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
        var projectileCollider = projectileManager.projectileCollider;
        foreach (Collider collider in characterColliders)
        {
            Physics.IgnoreCollision(collider, projectileCollider);
        }

        Vector3 aimPoint = GetAimPoint(player, projectileSpawnPoint.position);
        if (aimPoint != projectileSpawnPoint.position)
            instantiatedReleasedFX.transform.LookAt(aimPoint);

        Rigidbody projectileRigidbody = instantiatedReleasedFX.GetComponent<Rigidbody>();
        Vector3 direction = aimPoint - projectileSpawnPoint.position;
        direction.y = 0f;
        direction.Normalize();

        float horizontalDistance = Vector3.Distance(new Vector3(projectileSpawnPoint.position.x, 0f, projectileSpawnPoint.position.z), new Vector3(aimPoint.x, 0f, aimPoint.z));
        float verticalDistance = aimPoint.y - projectileSpawnPoint.position.y;

        float horizontalSpeed = horizontalDistance / airTime;
        float verticalSpeed = (verticalDistance - 0.5f * Physics.gravity.y * airTime * airTime) / airTime;

        projectileRigidbody.velocity = direction * horizontalSpeed + Vector3.up * verticalSpeed;

        SetAmountOfItem(player, GetAmountOfItem(player) - 1);

        if (isConsumable)
        {
            PlayerUIManager.instance.playerUIHudManager.SetQuickSlotCountText(GetAmountOfItem(player), true);
        }
    }

    private Vector3 GetAimPoint(PlayerManager player, Vector3 spawnPosition)
    {
        // TODO: Return the world-space aim point for locked-on or free aim.
        if (player.playerNetworkManager.isLockOn.Value && player.playerCombatManager.currentTarget != null)
        {
            // If the player has a current target, return its position.
            return player.playerCombatManager.currentTarget.transform.position;
        }
        else
        {
            // If the player does not have a current target, return a point in front of the player based on free aim range.
            Vector3 forwardDirection = PlayerCamera.instance.cameraObject.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;
            Ray aimRay = new Ray(spawnPosition, forwardDirection);

            var hits = Physics.RaycastAll(aimRay, freeAimRange, aimMask, QueryTriggerInteraction.Ignore);
            int minDistanceIndex = -1;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.transform.IsChildOf(player.transform))
                    continue;

                float distance = Vector3.Distance(spawnPosition, hits[i].point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistanceIndex = i;
                }
            }

            if (minDistanceIndex != -1)
            {
                return hits[minDistanceIndex].point;
            }

            return spawnPosition + forwardDirection * freeAimRange;
        }
    }
}
