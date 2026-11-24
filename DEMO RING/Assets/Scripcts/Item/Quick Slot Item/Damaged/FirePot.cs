using UnityEngine;

[CreateAssetMenu(fileName = "New Fire Pot", menuName = "Quick Slot Item/Fire Pot")]
public class FirePot : QuickSlotItem
{
    [Header("Fire Pot Settings")]
    public GameObject firePotPrefab;
    [SerializeField, Min(1f)] private float freeAimRange = 12f;
    [SerializeField, Min(0.1f)] private float preferredHorizontalSpeed = 12f;
    [SerializeField] private LayerMask aimMask = Physics.DefaultRaycastLayers;

    private const float MinimumFlightTime = 0.35f;
    private const float MaximumFlightTime = 1f;

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
        Vector3 spawnPosition = projectileSpawnPoint.position;
        Vector3 aimPoint = GetAimPoint(player, spawnPosition);
        GameObject projectile = Instantiate(firePotPrefab, spawnPosition, projectileSpawnPoint.rotation);
        var projectileManager = projectile.GetComponent<QuickSlotItemProjectileManager>();
        projectileManager.InitializeProjectile(player);

        foreach (Collider collider in player.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(collider, projectileManager.projectileCollider);

        var projectileBody = projectile.GetComponent<Rigidbody>();
        Vector3 displacement = aimPoint - projectileBody.position;
        Vector3 gravity = projectileBody.useGravity ? Physics.gravity : Vector3.zero;
        float horizontalDistance = Vector3.ProjectOnPlane(displacement, Vector3.up).magnitude;
        float flightTime = Mathf.Clamp(horizontalDistance / Mathf.Max(0.1f, preferredHorizontalSpeed),
            MinimumFlightTime, MaximumFlightTime);

        Vector3 launchVelocity = displacement / flightTime - 0.5f * gravity * flightTime;
        if (launchVelocity.sqrMagnitude > 0.001f)
            projectileBody.rotation = Quaternion.LookRotation(launchVelocity);
        projectileBody.velocity = launchVelocity;

        SetAmountOfItem(player, GetAmountOfItem(player) - 1);

        if (isConsumable)
        {
            PlayerUIManager.instance.playerUIHudManager.SetQuickSlotCountText(GetAmountOfItem(player), true);
        }
    }

    private Vector3 GetAimPoint(PlayerManager player, Vector3 spawnPosition)
    {
        var target = player.playerCombatManager.currentTarget;
        if (player.playerNetworkManager.isLockOn.Value && target != null)
        {
            Vector3 targetFeet = target.transform.position;
            if (target.characterController != null && target.characterController.enabled)
                targetFeet.y = target.characterController.bounds.min.y;

            return targetFeet;
        }

        Camera camera = player.IsOwner && PlayerCamera.instance != null
            ? PlayerCamera.instance.cameraObject
            : null;
        if (camera == null)
            return spawnPosition + player.transform.forward * freeAimRange;

        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 aimPoint = ray.GetPoint(freeAimRange);
        float nearestDistance = freeAimRange;
        foreach (RaycastHit hit in Physics.RaycastAll(ray, freeAimRange, aimMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.distance >= nearestDistance || hit.collider.transform.IsChildOf(player.transform))
                continue;

            nearestDistance = hit.distance;
            aimPoint = hit.point;
        }

        return aimPoint;
    }
}
