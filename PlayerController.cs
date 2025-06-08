using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // 基础移动参数
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 15f;
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityScale = 2.0f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private Transform modelTransform; // 玩家的视觉模型

    [Header("Rotation Settings")]
    [SerializeField] private float tiltSmoothing = 5f;     // 倾斜平滑系数
    [SerializeField] private float maxTiltAngle = 15f;     // 最大倾斜角度
    [SerializeField] private float maxTurnAngle = 45f;     // 最大转向角度
    private float currentTilt = 0f;                        // 当前倾斜值
    private float targetYRotation;                         // 目标Y轴旋转
    private float currentYRotation;                        // 当前Y轴旋转

    // 特技动作设置
    [Header("Trick Settings")]
    [SerializeField] private float trickDuration = 1.0f;   // 特技动作持续时间
    [SerializeField] private float backflipRotationSpeed = 360f; // 后空翻每秒旋转度数
    [SerializeField] private float spinRotationSpeed = 360f;     // 横向转体每秒旋转度数
    [SerializeField] private float trickScoreBonus = 100f;       // 特技得分加成

    // 碰撞和恢复设置
    [Header("Crash Settings")]
    [SerializeField] private float crashRecoveryTime = 2.0f;     // 碰撞恢复时间
    [SerializeField] private float recoverySpeed = 5f;           // 恢复时的移动速度
    [SerializeField] private float timeToRewind = 3f;            // 倒回时间长度
    [SerializeField] private float flashInterval = 0.2f;         // 闪烁间隔

    // 物理和状态
    private Rigidbody rb;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 moveDirection = Vector3.forward;

    // 特技状态
    private bool isPerformingTrick = false;
    private bool isBackflip = false;
    private bool isSpin = false;
    private float trickTimer = 0f;
    private Vector3 trickStartPosition;
    private Quaternion initialModelRotation;
    private float backflipAngle = 0f;    // 后空翻当前旋转角度
    private float spinAngle = 0f;        // 转体当前旋转角度

    // 碰撞状态
    private bool isRecovering = false;
    private Vector3[] positionHistory = new Vector3[120]; // 保存4秒的位置 (30fps × 4s)
    private Quaternion[] rotationHistory = new Quaternion[120];
    private int historyIndex = 0;
    private float historyRecordInterval = 0.033f; // 约30fps
    private float lastRecordTime;

    // 特效
    [Header("Effects")]
    [SerializeField] private ParticleSystem snowEffect;
    [SerializeField] private AudioSource slidingSound;

    // 游戏状态
    private bool isDead = false;
    private Renderer modelRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 获取模型渲染器用于闪烁效果
        if (modelTransform != null)
            modelRenderer = modelTransform.GetComponentInChildren<Renderer>();

        // 如果没有分配滑雪特效，则尝试获取组件
        if (snowEffect == null)
            snowEffect = GetComponentInChildren<ParticleSystem>();

        // 如果没有分配滑行音效，则尝试获取组件
        if (slidingSound == null)
            slidingSound = GetComponent<AudioSource>();

        // 锁定光标在游戏窗口中
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化移动
        currentSpeed = forwardSpeed;
        // 初始化旋转值
        currentYRotation = transform.eulerAngles.y;
        targetYRotation = currentYRotation;

        // 初始化位置历史
        for (int i = 0; i < positionHistory.Length; i++)
        {
            positionHistory[i] = transform.position;
            rotationHistory[i] = transform.rotation;
        }

        lastRecordTime = Time.time;
    }

    void Update()
    {
        if (isDead || isRecovering) return;

        // 记录位置历史
        RecordPositionHistory();

        // 检测跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // 检测特技输入 - 只有在空中才能执行
        if (!isGrounded && !isPerformingTrick)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartBackflip();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                StartSpin();
            }
        }

        // 更新特技状态
        if (isPerformingTrick)
        {
            UpdateTrick();
        }
        else
        {
            // 正常操作
            // 处理转向输入
            float horizontalInput = Input.GetAxis("Horizontal");
            Turn(horizontalInput);
        }

        // 控制特效
        ManageEffects();

        // 更新UI显示
        GameManager.Instance.UpdateSpeedUI(currentSpeed);
    }

    void FixedUpdate()
    {
        if (isDead || isRecovering) return;

        // 向前移动 (如果不在执行特技)
        if (!isPerformingTrick)
        {
            MoveForward();
        }

        // 应用额外重力
        ApplyGravity();

        // 确保不超过最大速度
        LimitMaxSpeed();
    }

    private void RecordPositionHistory()
    {
        if (Time.time - lastRecordTime >= historyRecordInterval)
        {
            historyIndex = (historyIndex + 1) % positionHistory.Length;
            positionHistory[historyIndex] = transform.position;
            rotationHistory[historyIndex] = transform.rotation;
            lastRecordTime = Time.time;
        }
    }

    private void StartBackflip()
    {
        isPerformingTrick = true;
        isBackflip = true;
        isSpin = false;
        trickTimer = 0f;
        trickStartPosition = transform.position;
        backflipAngle = 0f;  // 重置后空翻角度

        // 保存模型初始旋转
        if (modelTransform != null)
        {
            initialModelRotation = modelTransform.localRotation;
        }

        // 播放特技开始音效
        AudioManager.Instance?.PlaySound("TrickStart");
    }

    private void StartSpin()
    {
        isPerformingTrick = true;
        isBackflip = false;
        isSpin = true;
        trickTimer = 0f;
        trickStartPosition = transform.position;
        spinAngle = 0f;  // 重置转体角度

        // 保存模型初始旋转
        if (modelTransform != null)
        {
            initialModelRotation = modelTransform.localRotation;
        }

        // 播放特技开始音效
        AudioManager.Instance?.PlaySound("TrickStart");
    }

    private void UpdateTrick()
    {
        trickTimer += Time.deltaTime;
        float trickProgress = trickTimer / trickDuration;

        if (modelTransform != null)
        {
            if (isBackflip)
            {
                // 计算本帧的后空翻旋转增量
                float rotationDelta = backflipRotationSpeed * Time.deltaTime;
                backflipAngle += rotationDelta;

                // 应用纯X轴旋转(后空翻) - 保持其他轴的旋转不变
                Quaternion backflipRotation = Quaternion.Euler(backflipAngle, 0f, 0f);
                // 将初始旋转与后空翻旋转结合
                modelTransform.localRotation = initialModelRotation * backflipRotation;
            }
            else if (isSpin)
            {
                // 计算本帧的转体旋转增量
                float rotationDelta = spinRotationSpeed * Time.deltaTime;
                spinAngle += rotationDelta;

                // 重要修改：使用世界坐标系的Y轴旋转，而不是局部坐标系
                // 保存当前位置和父级引用
                Vector3 currentPosition = modelTransform.position;
                Transform parentTransform = modelTransform.parent;

                // 临时将模型放到世界根部，这样旋转就是相对于世界坐标系的
                modelTransform.SetParent(null);

                // 应用旋转 - 保持X和Z轴的原始方向，只绕Y轴旋转
                modelTransform.rotation = Quaternion.Euler(
                    initialModelRotation.eulerAngles.x,
                    initialModelRotation.eulerAngles.y + spinAngle,
                    initialModelRotation.eulerAngles.z
                );

                // 恢复父级和位置
                modelTransform.SetParent(parentTransform);
                modelTransform.position = currentPosition;
            }
        }

        // 检查特技是否完成
        if (trickTimer >= trickDuration)
        {
            CompleteTrick();
        }
    }

    private void CompleteTrick()
    {
        isPerformingTrick = false;

        // 重置模型旋转
        if (modelTransform != null)
        {
            modelTransform.localRotation = initialModelRotation;
        }

        // 奖励得分
        GameManager.Instance.AddScore(trickScoreBonus);

        // 播放特技完成音效
        AudioManager.Instance?.PlaySound("TrickComplete");
    }

    private void FailTrick()
    {
        isPerformingTrick = false;

        // 重置模型旋转
        if (modelTransform != null)
        {
            modelTransform.localRotation = initialModelRotation;
        }

        // 处理特技失败，与撞到障碍物类似
        StartCoroutine(HandleCrashRecovery(trickStartPosition));
    }

    private void MoveForward()
    {
        // 在下坡时加速，上坡时减速
        float slopeMultiplier = 1.0f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            // 计算坡度(0 = 平地��1 = 垂直向下，-1 = 垂直向上)
            float slopeAngle = Vector3.Dot(hit.normal, Vector3.up);

            // 调整速度因子
            float slopeFactor = 1.0f - slopeAngle;
            slopeMultiplier = 1.0f + (slopeFactor * 0.5f); // 下坡最多增加50%速度
        }

        // 更新当前速度
        currentSpeed = Mathf.Lerp(currentSpeed, forwardSpeed * slopeMultiplier, Time.fixedDeltaTime);

        // 应用前进力
        rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);
    }

    private void Turn(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // 计算目标转向角度 - 受限于最大转向角度
            targetYRotation += horizontalInput * turnSpeed * Time.deltaTime;

            // 计算目标倾斜角度 - 保持在最大倾斜范围内
            float targetTilt = -horizontalInput * maxTiltAngle;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothing);
        }
        else
        {
            // 当没有输入时平滑回正
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * tiltSmoothing);
        }

        // 平滑应用Y轴旋转
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * 5f);

        // 应用旋转到整个角色，只改变Y轴
        Quaternion characterRotation = Quaternion.Euler(0, currentYRotation, 0);
        transform.rotation = characterRotation;

        // 更新移动方向跟随角色朝向
        moveDirection = transform.forward;

        // 只应用Z轴倾斜到模型（保持X和Y轴不变）
        if (modelTransform != null && !isPerformingTrick) // 如果不在执行特技
        {
            // 保留模型原有的X和Y旋转，只修改Z轴倾斜
            Vector3 currentModelRotation = modelTransform.localEulerAngles;
            Quaternion targetModelRotation = Quaternion.Euler(
                currentModelRotation.x,  // 保持X轴旋转不变
                currentModelRotation.y,  // 保持Y轴旋转不变
                currentTilt              // 只修改Z轴倾斜
            );

            // 应用旋转到模型
            modelTransform.localRotation = targetModelRotation;
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;

        // 播放跳跃音效
        AudioManager.Instance?.PlaySound("Jump");
    }

    private void ApplyGravity()
    {
        // 额外重力让滑行感觉更真实
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    private void LimitMaxSpeed()
    {
        // 限制最大速度
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void ManageEffects()
    {
        // 处理雪特效
        if (snowEffect != null)
        {
            var emission = snowEffect.emission;
            if (isGrounded && currentSpeed > 5f && !isRecovering)
            {
                if (!snowEffect.isPlaying)
                    snowEffect.Play();

                // 根据速度调整排放率
                emission.rateOverTimeMultiplier = currentSpeed * 0.5f;
            }
            else
            {
                if (snowEffect.isPlaying)
                    snowEffect.Stop();
            }
        }

        // 处理滑行音效
        /*if (slidingSound != null)
        {
            if (isGrounded && currentSpeed > 5f && !isRecovering)
            {
                if (!slidingSound.isPlaying)
                    slidingSound.Play();

                // 根据速度调整音量和音调
                slidingSound.volume = Mathf.Clamp01(currentSpeed / maxSpeed);
                slidingSound.pitch = 0.8f + (currentSpeed / maxSpeed) * 0.4f;
            }
            else
            {
                if (slidingSound.isPlaying)
                    slidingSound.Stop();
            }
        }*/
    }

    void OnCollisionEnter(Collision collision)
    {
        // 检测地面碰撞
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            // 如果在执行特技时落地
            if (isPerformingTrick)
            {
                FailTrick();
            }
        }

        // 检测障碍物碰撞
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            float impactVelocity = collision.relativeVelocity.magnitude;

            // 如果碰撞速度较大
            if (impactVelocity > 10f)
            {
                Die();
            }
            else
            {
                // 获取碰撞前的位置
                int rewindIndex = (historyIndex - (int)(timeToRewind / historyRecordInterval) + positionHistory.Length) % positionHistory.Length;
                Vector3 positionBeforeCrash = positionHistory[rewindIndex];

                // 开始恢复过程
                StartCoroutine(HandleCrashRecovery(positionBeforeCrash));

                // 播放碰撞音效
                AudioManager.Instance?.PlaySound("Crash");
            }
        }
    }

    private IEnumerator HandleCrashRecovery(Vector3 recoveryPosition)
    {
        isRecovering = true;

        // 停止所有移动
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 闪烁效果
        if (modelRenderer != null)
        {
            float elapsedTime = 0f;
            bool isVisible = true;

            // 闪烁阶段
            while (elapsedTime < crashRecoveryTime) // 整个恢复时间都用于闪烁
            {
                isVisible = !isVisible;
                modelRenderer.enabled = isVisible;

                yield return new WaitForSeconds(flashInterval);
                elapsedTime += flashInterval;
            }

            // 确保恢复可见
            modelRenderer.enabled = true;
        }
        else
        {
            // 如果没有渲染器，只等待
            yield return new WaitForSeconds(crashRecoveryTime);
        }

        // 直接传送到恢复位置
        transform.position = recoveryPosition;

        // 恢复移动速度
        currentSpeed = forwardSpeed * 0.5f;

        // 恢复正常状态
        isRecovering = false;
    }
    public bool IsRecovering()
    {
        return isRecovering;
    }
    void OnTriggerEnter(Collider other)
    {
        // 收集物
        if (other.CompareTag("Collectible"))
        {
            GameManager.Instance.CollectCoin();

            // 播放收集音效
            AudioManager.Instance?.PlaySound("Collect");
            // 销毁收集物
            Destroy(other.gameObject);
        }

        // 终点线
        if (other.CompareTag("Finish"))
        {
            GameManager.Instance.LevelComplete();
            AudioManager.Instance?.PlaySound("Arrive");
            Destroy(other.gameObject);
        }

        // 死亡区域(如果玩家掉出地图)
        if (other.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;

            // 播放死亡音效
            AudioManager.Instance?.PlaySound("Death");

            // 减慢运动
            rb.velocity *= 0.2f;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;

            // 通知游戏管理器
            GameManager.Instance.GameOver();
        }
    }
}