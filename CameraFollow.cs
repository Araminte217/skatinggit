using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Position Settings")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 5, -10);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float lookAheadFactor = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationLerpSpeed = 3f;  // 旋转平滑系数
    [SerializeField] private float maxHorizontalAngle = 30f;  // 最大水平旋转角度
    [SerializeField] private bool useTargetRotation = true;  // 是否使用目标旋转

    // 当前相机旋转角度
    private float currentRotationAngle = 0f;
    private Vector3 currentVelocity;
    private Quaternion targetRotation;

    // 跟踪状态管理
    private bool isTrackingPaused = false;
    private PlayerController playerController;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        if (target != null)
        {
            // 初始化旋转角度
            currentRotationAngle = target.eulerAngles.y;
            targetRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // 获取玩家控制器组件
            playerController = target.GetComponent<PlayerController>();

            // 保存初始位置和旋转
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 检查玩家状态以决定是否暂停追踪
        CheckTrackingState();

        // 如果暂停追踪，不更新相机位置
        if (isTrackingPaused) return;

        // 获取目标的速度和前进方向
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        Vector3 velocity = Vector3.zero;
        Vector3 targetForward = target.forward;

        if (targetRb != null)
        {
            velocity = targetRb.velocity;
            velocity.y = 0; // 忽略垂直运动
            currentVelocity = Vector3.Lerp(currentVelocity, velocity, Time.deltaTime * 3f);
        }

        // 计算目标旋转角度 - 使用目标物体的朝向
        float targetAngle = target.eulerAngles.y;

        // 平滑地插值到目标角度，但限制最大变化角度
        float angleDifference = Mathf.DeltaAngle(currentRotationAngle, targetAngle);
        float clampedDifference = Mathf.Clamp(angleDifference, -maxHorizontalAngle, maxHorizontalAngle);

        // 计算实际应用的角度 - 让相机有一定的旋转空间
        float appliedAngle;
        if (useTargetRotation)
        {
            // 完全跟随目标旋转，但有平滑过渡
            appliedAngle = Mathf.LerpAngle(currentRotationAngle, targetAngle, Time.deltaTime * rotationLerpSpeed);
        }
        else
        {
            // 部分跟随，保持在有限角度内
            appliedAngle = Mathf.LerpAngle(currentRotationAngle, currentRotationAngle + clampedDifference,
                                         Time.deltaTime * rotationLerpSpeed);
        }

        currentRotationAngle = appliedAngle;
        targetRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // 根据当前旋转角度计算偏移
        Vector3 rotatedOffset = targetRotation * baseOffset;

        // 基本位置
        Vector3 desiredPosition = target.position + rotatedOffset;

        // 向前看一点，让玩家看到前方地形
        if (currentVelocity.magnitude > 0.1f)
        {
            desiredPosition += currentVelocity.normalized * lookAheadFactor;
        }

        // 平滑移动
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 相机始终看向玩家略微上方的位置，提供更好的视角
        Vector3 lookAtPosition = target.position + new Vector3(0, 1f, 0);
        transform.LookAt(lookAtPosition);

        // 保存最后的位置和旋转
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    // 检查是否应该暂停追踪
    private void CheckTrackingState()
    {
        if (playerController == null) return;

        // 获取玩家的恢复状态
        bool playerIsRecovering = playerController.IsRecovering();

        // 如果玩家正在恢复，暂停追踪
        if (playerIsRecovering && !isTrackingPaused)
        {
            isTrackingPaused = true;
        }
        // 如果玩家不在恢复，恢复追踪
        else if (!playerIsRecovering && isTrackingPaused)
        {
            isTrackingPaused = false;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            // 重新初始化角度
            currentRotationAngle = target.eulerAngles.y;
            targetRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // 重新获取玩家控制器
            playerController = target.GetComponent<PlayerController>();
        }
    }
}