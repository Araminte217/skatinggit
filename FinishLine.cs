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

        // 播放特效
        if (finishEffect != null)
        {
            Instantiate(finishEffect, transform.position, Quaternion.identity);
        }

        // 播放音效
        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, transform.position);
        }

        // 通知游戏管理器
        GameManager.Instance.LevelComplete();
    }
}