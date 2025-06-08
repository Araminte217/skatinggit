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
        // 如果没有分配粒子系统，尝试获取组件
        if (trailParticles == null)
            trailParticles = GetComponent<ParticleSystem>();

        // 如果没有分配玩家刚体，尝试从父对象获取
        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // 检测是否接触地面
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        // 管理粒子效果
        if (trailParticles != null)
        {
            // 当玩家在地面上且速度足够快时展示雪尘效果
            if (isGrounded && playerRigidbody != null && playerRigidbody.velocity.magnitude > minSpeed)
            {
                if (!trailParticles.isPlaying)
                    trailParticles.Play();

                // 根据速度调整发射率
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