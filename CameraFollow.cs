using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Position Settings")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 5, -10);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float lookAheadFactor = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationLerpSpeed = 3f;  // ��תƽ��ϵ��
    [SerializeField] private float maxHorizontalAngle = 30f;  // ���ˮƽ��ת�Ƕ�
    [SerializeField] private bool useTargetRotation = true;  // �Ƿ�ʹ��Ŀ����ת

    // ��ǰ�����ת�Ƕ�
    private float currentRotationAngle = 0f;
    private Vector3 currentVelocity;
    private Quaternion targetRotation;

    // ����״̬����
    private bool isTrackingPaused = false;
    private PlayerController playerController;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        if (target != null)
        {
            // ��ʼ����ת�Ƕ�
            currentRotationAngle = target.eulerAngles.y;
            targetRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // ��ȡ��ҿ��������
            playerController = target.GetComponent<PlayerController>();

            // �����ʼλ�ú���ת
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ������״̬�Ծ����Ƿ���ͣ׷��
        CheckTrackingState();

        // �����ͣ׷�٣����������λ��
        if (isTrackingPaused) return;

        // ��ȡĿ����ٶȺ�ǰ������
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        Vector3 velocity = Vector3.zero;
        Vector3 targetForward = target.forward;

        if (targetRb != null)
        {
            velocity = targetRb.velocity;
            velocity.y = 0; // ���Դ�ֱ�˶�
            currentVelocity = Vector3.Lerp(currentVelocity, velocity, Time.deltaTime * 3f);
        }

        // ����Ŀ����ת�Ƕ� - ʹ��Ŀ������ĳ���
        float targetAngle = target.eulerAngles.y;

        // ƽ���ز�ֵ��Ŀ��Ƕȣ����������仯�Ƕ�
        float angleDifference = Mathf.DeltaAngle(currentRotationAngle, targetAngle);
        float clampedDifference = Mathf.Clamp(angleDifference, -maxHorizontalAngle, maxHorizontalAngle);

        // ����ʵ��Ӧ�õĽǶ� - �������һ������ת�ռ�
        float appliedAngle;
        if (useTargetRotation)
        {
            // ��ȫ����Ŀ����ת������ƽ������
            appliedAngle = Mathf.LerpAngle(currentRotationAngle, targetAngle, Time.deltaTime * rotationLerpSpeed);
        }
        else
        {
            // ���ָ��棬���������޽Ƕ���
            appliedAngle = Mathf.LerpAngle(currentRotationAngle, currentRotationAngle + clampedDifference,
                                         Time.deltaTime * rotationLerpSpeed);
        }

        currentRotationAngle = appliedAngle;
        targetRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // ���ݵ�ǰ��ת�Ƕȼ���ƫ��
        Vector3 rotatedOffset = targetRotation * baseOffset;

        // ����λ��
        Vector3 desiredPosition = target.position + rotatedOffset;

        // ��ǰ��һ�㣬����ҿ���ǰ������
        if (currentVelocity.magnitude > 0.1f)
        {
            desiredPosition += currentVelocity.normalized * lookAheadFactor;
        }

        // ƽ���ƶ�
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // ���ʼ�տ��������΢�Ϸ���λ�ã��ṩ���õ��ӽ�
        Vector3 lookAtPosition = target.position + new Vector3(0, 1f, 0);
        transform.LookAt(lookAtPosition);

        // ��������λ�ú���ת
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    // ����Ƿ�Ӧ����ͣ׷��
    private void CheckTrackingState()
    {
        if (playerController == null) return;

        // ��ȡ��ҵĻָ�״̬
        bool playerIsRecovering = playerController.IsRecovering();

        // ���������ڻָ�����ͣ׷��
        if (playerIsRecovering && !isTrackingPaused)
        {
            isTrackingPaused = true;
        }
        // �����Ҳ��ڻָ����ָ�׷��
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
            // ���³�ʼ���Ƕ�
            currentRotationAngle = target.eulerAngles.y;
            targetRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // ���»�ȡ��ҿ�����
            playerController = target.GetComponent<PlayerController>();
        }
    }
}