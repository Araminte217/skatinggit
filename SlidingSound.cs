using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SlidingSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] slidingSounds; // 不同表面的滑雪音效
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private float minVolume = 0.2f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float fadeSpeed = 5f; // 音效淡入淡出速度

    [Header("Detection Settings")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private string iceTag = "Ice";
    [SerializeField] private string deepSnowTag = "DeepSnow";

    // 音效状态
    private AudioSource audioSource;
    private float targetVolume = 0f;
    private float currentVolumeVelocity = 0f;
    private bool wasPlaying = false;
    private int currentSurfaceType = 0; // 0 = 普通雪, 1 = 冰, 2 = 深雪

    // 引用
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        // 获取音频源
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        // 默认使用第一个音效
        if (slidingSounds != null && slidingSounds.Length > 0)
        {
            audioSource.clip = slidingSounds[0];
        }

        // 获取玩家刚体
        playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        // 检测地面类型
        DetectSurface();

        // 根据状态调整音效
        AdjustAudio();

        // 淡入淡出处理
        HandleFading();
    }

    private void DetectSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // 检测不同类型的地面
            if (hit.collider.CompareTag(iceTag))
            {
                SetSurfaceType(1); // 冰面
            }
            else if (hit.collider.CompareTag(deepSnowTag))
            {
                SetSurfaceType(2); // 深雪
            }
            else
            {
                SetSurfaceType(0); // 普通雪
            }
        }
        else
        {
            // 不在地面上，停止声音
            targetVolume = 0f;
        }
    }

    private void SetSurfaceType(int type)
    {
        // 如果地面类型改变，更新音效
        if (currentSurfaceType != type && slidingSounds != null && type < slidingSounds.Length && slidingSounds[type] != null)
        {
            currentSurfaceType = type;

            // 记住当前播放状态
            wasPlaying = audioSource.isPlaying;

            // 更新音频剪辑
            audioSource.clip = slidingSounds[type];

            // 如果之前在播放，则继续播放
            if (wasPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void AdjustAudio()
    {
        if (playerRigidbody != null)
        {
            // 获取水平速度
            Vector3 horizontalVelocity = playerRigidbody.velocity;
            horizontalVelocity.y = 0; // 忽略垂直速度
            float speed = horizontalVelocity.magnitude;

            // 只有当速度大于阈值时才播放音效
            if (speed > 5f)
            {
                // 确保音效在播放
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                // 根据速度调整音调
                float speedRatio = Mathf.Clamp01(speed / 40f); // 假设最大速度为40
                audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

                // 设置目标音量
                targetVolume = Mathf.Lerp(minVolume, maxVolume, speedRatio);

                // 根据地面类型调整音效特性
                switch (currentSurfaceType)
                {
                    case 1: // 冰面
                        // 冰面声音更尖锐
                        audioSource.pitch *= 1.2f;
                        break;
                    case 2: // 深雪
                        // 深雪声音更沉闷
                        audioSource.pitch *= 0.8f;
                        targetVolume *= 1.2f; // 深雪声音更大
                        break;
                }
            }
            else
            {
                // 速度太慢，静音
                targetVolume = 0f;
            }
        }
    }

    private void HandleFading()
    {
        // 平滑过渡音量
        audioSource.volume = Mathf.SmoothDamp(
            audioSource.volume,
            targetVolume,
            ref currentVolumeVelocity,
            1f / fadeSpeed
        );

        // 如果音量接近0且正在播放，则停止音效
        if (audioSource.volume < 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // 公共方法，允许PlayerController直接设置音效状态
    public void SetAudioState(float speed, float maxSpeed, bool isGrounded, bool isRecovering)
    {
        if (isGrounded && speed > 5f && !isRecovering)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            // 根据速度调整音量和音调
            float speedRatio = Mathf.Clamp01(speed / maxSpeed);
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, speedRatio);
            audioSource.pitch = minPitch + speedRatio * (maxPitch - minPitch);
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}