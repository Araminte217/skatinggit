using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float minDamageVelocity = 10f;
    [SerializeField] private GameObject impactEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            float impactVelocity = collision.relativeVelocity.magnitude;

            // 如果碰撞速度够大，展示效果
            if (impactVelocity > minDamageVelocity && impactEffect != null)
            {
                Instantiate(impactEffect, collision.contacts[0].point, Quaternion.identity);
            }
        }
    }
}