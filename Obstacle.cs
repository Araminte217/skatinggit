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

            // �����ײ�ٶȹ���չʾЧ��
            if (impactVelocity > minDamageVelocity && impactEffect != null)
            {
                Instantiate(impactEffect, collision.contacts[0].point, Quaternion.identity);
            }
        }
    }
}