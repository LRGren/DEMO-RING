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

    [Header("Lock On")]
    [SerializeField] private float lockOnRadius = 20f;
    [SerializeField] private float minimumLockOnAngle = -50f;
    [SerializeField] private float maximumLockOnAngle = 50f;
    [SerializeField] private float maximumLockOnDistance = 20f;

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

                //如果目标在锁定范围外
                if(distanceFromTarget > maximumLockOnDistance)
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
                        Debug .Log("WE MADE IT");
                    }
                }
            }
        }
    }
}
