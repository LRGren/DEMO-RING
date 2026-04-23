using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;
    public PlayerManager player;
    public Camera cameraObject;
    [SerializeField] private Transform cameraPivotTransform;
    
    [Header("Camera Settings")]
    public float cameraSmoothTime = 1f;
    [SerializeField] private float leftAndRightRotationSpeed = 220f;
    [SerializeField] private float upAndDownRotationSpeed = 220f;
    [SerializeField] private float minimumPivot = -30f;
    [SerializeField] private float maximumPivot = 60f;
    [SerializeField] private float cameraCollisionRadius = 0.2f;
    [SerializeField] private LayerMask collideWithLayers;
    
    
    [Header("Camera Values")]
    private Vector3 cameraVelocity;
    private Vector3 cameraObjectPosition;
    [SerializeField] private float leftAndRightAngle;
    [SerializeField] private float upAndDownAngle;
    private float cameraZPosition;
    private float targetCameraZPosition;
    private Coroutine cameraLockOnCoroutine;

    [Header("Lock On")]
    [SerializeField] private float lockOnRadius = 20f;
    [SerializeField] private float minimumLockOnAngle = -50f;
    [SerializeField] private float maximumLockOnAngle = 50f;
    [SerializeField] private float lockOnTargetFollowSpeed = 0.2f;
    [SerializeField] private float setCameraHeightSpeed = 0.2f;
    [SerializeField] private float unlockedCameraHeight = 1.65f;
    [SerializeField] private float lockedCameraHeight = 2f;
    private List<CharacterManager> availableTargets = new List<CharacterManager>();
    public CharacterManager nearestLockOnTarget;
    public CharacterManager leftLockOnTarget;
    public CharacterManager rightLockOnTarget;

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
        cameraZPosition = cameraObject.transform.localPosition.z;
    }

    public void HandleAllCameraActions()
    {
        if (player != null)
        {
            //跟随
            HandleFollowTarget();
            //旋转
            HandleRotation();
            //与物体碰撞
            HandleCollisions();
        }
    }

    private void HandleFollowTarget()
    {
        Vector3 targetCameraPosition = Vector3.SmoothDamp(transform.position, player.transform.position,
            ref cameraVelocity, cameraSmoothTime * Time.deltaTime);
        transform.position = targetCameraPosition;
    }

    private void HandleRotation()
    {
        //锁定状态下，摄像机始终面向锁定目标
        if (player.playerNetworkManager.isLockOn.Value)
        {
            Vector3 rotationDirection = player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - transform.position;
            rotationDirection.Normalize();
            rotationDirection.y = 0; //保持水平旋转
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockOnTargetFollowSpeed);

            rotationDirection = player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - cameraPivotTransform.position;
            rotationDirection.Normalize();

            targetRotation = Quaternion.LookRotation(rotationDirection);
            cameraPivotTransform.rotation = Quaternion.Slerp(cameraPivotTransform.rotation, targetRotation, lockOnTargetFollowSpeed);

            leftAndRightAngle = transform.eulerAngles.y;
            upAndDownAngle = cameraPivotTransform.eulerAngles.x;
        }
        //非锁定状态下，摄像机根据玩家输入进行旋转
        else
        {

            leftAndRightAngle += (PlayerInputManager.instance.cameraHorizontalInput * leftAndRightRotationSpeed) * Time.deltaTime;

            upAndDownAngle -= (PlayerInputManager.instance.cameraVerticalInput * upAndDownRotationSpeed) * Time.deltaTime;
            upAndDownAngle = Mathf.Clamp(upAndDownAngle, minimumPivot, maximumPivot);

            Vector3 cameraRotation = Vector3.zero;
            Quaternion targetRotation;

            cameraRotation.y = leftAndRightAngle;
            targetRotation = Quaternion.Euler(cameraRotation);
            transform.rotation = targetRotation;

            cameraRotation.x = upAndDownAngle;
            targetRotation = Quaternion.Euler(cameraRotation);
            cameraPivotTransform.rotation = targetRotation;
        }
    }

    private void HandleCollisions()
    {
        targetCameraZPosition = cameraZPosition;
        
        RaycastHit hit;
        Vector3 direction = cameraObject.transform.position - cameraPivotTransform.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit,
                Mathf.Abs(targetCameraZPosition), collideWithLayers))
        {
            float distanceFromHitObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetCameraZPosition = -(distanceFromHitObject - cameraCollisionRadius);
        }

        if (Mathf.Abs(targetCameraZPosition) < cameraCollisionRadius)
        {
            targetCameraZPosition = -cameraCollisionRadius;
        }

        cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraZPosition, 0.2f);
        cameraObject.transform.localPosition = cameraObjectPosition;
    }

    public void HandleLocatingLockOnTargets()
    {
        float shortestDistance = Mathf.Infinity;                //锁定目标的最短距离
        float shortestDistanceOfRightTarget = Mathf.Infinity;   //用于确定某一轴【当前目标右侧】的最短距离
        float shortestDistanceOfLeftTarget = -Mathf.Infinity;   //用于确定某一轴【当前目标左侧】的最短距离

        //TO DO: Layers
        Collider[] colliders = Physics.OverlapSphere(player.transform.position, lockOnRadius,WorldUtilityManager.instance.GetCharacterLayers());

        for (int i = 0; i < colliders.Length; i++)
        {
            CharacterManager lockOnTarget = colliders[i].GetComponent<CharacterManager>();

            if (lockOnTarget != null)
            {
                Vector3 lockTargetDirection = lockOnTarget.transform.position - player.transform.position;
                float distanceFromTarget = Vector3.Distance(player.transform.position, lockOnTarget.transform.position);
                float viewableAngle = Vector3.Angle(lockTargetDirection, cameraObject.transform.forward);

                //如果目标死亡
                if (lockOnTarget.isDead.Value)
                    continue;

                //如果目标是自己
                if(lockOnTarget.transform.root == player.transform.root)
                    continue;

                //如果目标在锁定视野内
                if(viewableAngle > minimumLockOnAngle && viewableAngle < maximumLockOnAngle)
                {
                    RaycastHit hit;

                    //TO DO: Layers
                    if (Physics.Linecast(player.playerCombatManager.lockOnTransform.position, 
                        lockOnTarget.characterCombatManager.lockOnTransform.position, 
                        out hit,WorldUtilityManager.instance.GetEnviroLayers()))
                    {
                        //如果射线碰撞到的物体不是锁定目标
                        continue;
                    }
                    else
                    {
                        //Debug .Log("WE MADE IT");
                        availableTargets.Add(lockOnTarget);
                    }
                }

            }

        }

        //找到最近的锁定目标，优先选择当前目标
        for (int i = 0; i < availableTargets.Count; i++)
        {

            if(availableTargets[i] != null) {
                float distanceFromTarget = Vector3.Distance(player.transform.position, availableTargets[i].transform.position);

                if (distanceFromTarget < shortestDistance)
                {
                    shortestDistance = distanceFromTarget;
                    nearestLockOnTarget = availableTargets[i];
                }

                //找到最近目标之后，根据目标相对于玩家的位置，确定左右锁定目标
                if (player.playerNetworkManager.isLockOn.Value)
                {
                    Vector3 relativeEnemyPosition = player.transform.InverseTransformPoint(availableTargets[i].transform.position);

                    var distanceFromLeftTarget = relativeEnemyPosition.x;
                    var distanceFromRightTarget = relativeEnemyPosition.x;

                    if (availableTargets[i] == player.playerCombatManager.currentTarget)
                        continue;

                    if (relativeEnemyPosition.x > 0 && distanceFromRightTarget < shortestDistanceOfRightTarget)
                    {
                        shortestDistanceOfRightTarget = distanceFromRightTarget;
                        rightLockOnTarget = availableTargets[i];
                    }
                    if (relativeEnemyPosition.x < 0 && distanceFromLeftTarget > shortestDistanceOfLeftTarget)
                    {
                        shortestDistanceOfLeftTarget = distanceFromLeftTarget;
                        leftLockOnTarget = availableTargets[i];
                    }
                }
            }
            else
            {
                ClearLockOnTargets();
                player.playerNetworkManager.isLockOn.Value = false;
            }

        }

    }

    public void SetLockOnCameraHeight()
    {
        if(cameraLockOnCoroutine != null)
        {
            StopCoroutine(cameraLockOnCoroutine);
        }

        cameraLockOnCoroutine = StartCoroutine(SetCameraHeight());
    }

    public void ClearLockOnTargets()
    {
        leftLockOnTarget = null;
        rightLockOnTarget = null;
        nearestLockOnTarget = null;
        availableTargets.Clear();
    }

    public IEnumerator WaitThenFindNewTarget()
    {
        while (player.isPerformingAction)
        {
            yield return null;
        }

        ClearLockOnTargets();
        HandleLocatingLockOnTargets();

        if(nearestLockOnTarget != null)
        {
            player.playerCombatManager.currentTarget = nearestLockOnTarget;
            player.playerNetworkManager.isLockOn.Value = true;
        }

        yield return null;
    }

    private IEnumerator SetCameraHeight()
    {
        /*float duration = 1f;
        float timer = 0f;*/

        Vector3 velocity = Vector3.zero;
        Vector3 newLockedCameraHeight = new Vector3(cameraPivotTransform.localPosition.x, lockedCameraHeight);
        Vector3 newUnlockedCameraHeight = new Vector3(cameraPivotTransform.localPosition.x, unlockedCameraHeight);


        var targetCameraHeight = player.playerCombatManager.currentTarget != null ? newLockedCameraHeight : newUnlockedCameraHeight;

        while(Vector3.Distance(cameraPivotTransform.localEulerAngles, targetCameraHeight) > 0.01f)
        //while (timer < duration)
        {
            //Debug.Log("Setting Camera Height");

            //timer += Time.deltaTime;


            if (player != null)
            {
                if (player.playerCombatManager.currentTarget != null)
                {
                    cameraPivotTransform.localPosition =
                        Vector3.SmoothDamp(cameraPivotTransform.localPosition, newLockedCameraHeight, ref velocity, setCameraHeightSpeed);
                    cameraPivotTransform.localRotation =
                        Quaternion.Slerp(cameraPivotTransform.localRotation, Quaternion.Euler(0, 0, 0), lockOnTargetFollowSpeed);
                }
                else
                {
                    cameraPivotTransform.localPosition =
                        Vector3.SmoothDamp(cameraPivotTransform.localPosition, newUnlockedCameraHeight, ref velocity, setCameraHeightSpeed);
                }
            }

            yield return null;
        }

        if (player != null)
        {
            if (player.playerCombatManager.currentTarget != null)
            {
                cameraPivotTransform.localPosition = newLockedCameraHeight;
                cameraPivotTransform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                cameraPivotTransform.localPosition = newUnlockedCameraHeight;
            }
        }

        yield return null;
    }
}
