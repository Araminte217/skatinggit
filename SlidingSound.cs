using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SlidingSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] slidingSounds; // ��ͬ����Ļ�ѩ��Ч
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private float minVolume = 0.2f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float fadeSpeed = 5f; // ��Ч���뵭���ٶ�

    [Header("Detection Settings")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private string iceTag = "Ice";
    [SerializeField] private string deepSnowTag = "DeepSnow";

    // ��Ч״̬
    private AudioSource audioSource;
    private float targetVolume = 0f;
    private float currentVolumeVelocity = 0f;
    private bool wasPlaying = false;
    private int currentSurfaceType = 0; // 0 = ��ͨѩ, 1 = ��, 2 = ��ѩ

    // ����
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        // ��ȡ��ƵԴ
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        // Ĭ��ʹ�õ�һ����Ч
        if (slidingSounds != null && slidingSounds.Length > 0)
        {
            audioSource.clip = slidingSounds[0];
        }

        // ��ȡ��Ҹ���
        playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        // ����������
        DetectSurface();

        // ����״̬������Ч
        AdjustAudio();

        // ���뵭������
        HandleFading();
    }

    private void DetectSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // ��ⲻͬ���͵ĵ���
            if (hit.collider.CompareTag(iceTag))
            {
                SetSurfaceType(1); // ����
            }
            else if (hit.collider.CompareTag(deepSnowTag))
            {
                SetSurfaceType(2); // ��ѩ
            }
            else
            {
                SetSurfaceType(0); // ��ͨѩ
            }
        }
        else
        {
            // ���ڵ����ϣ�ֹͣ����
            targetVolume = 0f;
        }
    }

    private void SetSurfaceType(int type)
    {
        // ����������͸ı䣬������Ч
        if (currentSurfaceType != type && slidingSounds != null && type < slidingSounds.Length && slidingSounds[type] != null)
        {
            currentSurfaceType = type;

            // ��ס��ǰ����״̬
            wasPlaying = audioSource.isPlaying;

            // ������Ƶ����
            audioSource.clip = slidingSounds[type];

            // ���֮ǰ�ڲ��ţ����������
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
            // ��ȡˮƽ�ٶ�
            Vector3 horizontalVelocity = playerRigidbody.velocity;
            horizontalVelocity.y = 0; // ���Դ�ֱ�ٶ�
            float speed = horizontalVelocity.magnitude;

            // ֻ�е��ٶȴ�����ֵʱ�Ų�����Ч
            if (speed > 5f)
            {
                // ȷ����Ч�ڲ���
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                // �����ٶȵ�������
                float speedRatio = Mathf.Clamp01(speed / 40f); // ��������ٶ�Ϊ40
                audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

                // ����Ŀ������
                targetVolume = Mathf.Lerp(minVolume, maxVolume, speedRatio);

                // ���ݵ������͵�����Ч����
                switch (currentSurfaceType)
                {
                    case 1: // ����
                        // ��������������
                        audioSource.pitch *= 1.2f;
                        break;
                    case 2: // ��ѩ
                        // ��ѩ����������
                        audioSource.pitch *= 0.8f;
                        targetVolume *= 1.2f; // ��ѩ��������
                        break;
                }
            }
            else
            {
                // �ٶ�̫��������
                targetVolume = 0f;
            }
        }
    }

    private void HandleFading()
    {
        // ƽ����������
        audioSource.volume = Mathf.SmoothDamp(
            audioSource.volume,
            targetVolume,
            ref currentVolumeVelocity,
            1f / fadeSpeed
        );

        // ��������ӽ�0�����ڲ��ţ���ֹͣ��Ч
        if (audioSource.volume < 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // ��������������PlayerControllerֱ��������Ч״̬
    public void SetAudioState(float speed, float maxSpeed, bool isGrounded, bool isRecovering)
    {
        if (isGrounded && speed > 5f && !isRecovering)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            // �����ٶȵ�������������
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