using UnityEngine;

public class SnowTrail : MonoBehaviour
{
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private float groundCheckDistance = 0.5f;

    private bool isGrounded;

    void Start()
    {
        // ���û�з�������ϵͳ�����Ի�ȡ���
        if (trailParticles == null)
            trailParticles = GetComponent<ParticleSystem>();

        // ���û�з�����Ҹ��壬���ԴӸ������ȡ
        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // ����Ƿ�Ӵ�����
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        // ��������Ч��
        if (trailParticles != null)
        {
            // ������ڵ��������ٶ��㹻��ʱչʾѩ��Ч��
            if (isGrounded && playerRigidbody != null && playerRigidbody.velocity.magnitude > minSpeed)
            {
                if (!trailParticles.isPlaying)
                    trailParticles.Play();

                // �����ٶȵ���������
                var emission = trailParticles.emission;
                emission.rateOverTimeMultiplier = playerRigidbody.velocity.magnitude * 0.5f;
            }
            else if (trailParticles.isPlaying)
            {
                trailParticles.Stop();
            }
        }
    }
}