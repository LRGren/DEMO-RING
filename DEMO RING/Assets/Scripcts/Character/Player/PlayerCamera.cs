using System.Collections;
using System.Collections.Generic;
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
}
