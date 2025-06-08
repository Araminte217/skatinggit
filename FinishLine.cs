using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameObject finishEffect;
    [SerializeField] private AudioClip finishSound;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;

        isTriggered = true;

        // ������Ч
        if (finishEffect != null)
        {
            Instantiate(finishEffect, transform.position, Quaternion.identity);
        }

        // ������Ч
        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, transform.position);
        }

        // ֪ͨ��Ϸ������
        GameManager.Instance.LevelComplete();
    }
}