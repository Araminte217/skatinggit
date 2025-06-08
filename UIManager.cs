using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Message System")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    private Coroutine messageCoroutine;

    [Header("UI Panels")]
    [SerializeField] private GameObject[] allUIPanels;  // �洢���п�����ʾ��UI��壬���ڿ���ֻ��ʾһ��

    private void Start()
    {
        // ��ʼ��ʱ��������UI���
        HideAllPanels();
    }

    /// <summary>
    /// ��ʾ��ʱ��Ϣ
    /// </summary>
    /// <param name="message">��Ϣ����</param>
    /// <param name="duration">��ʾʱ��(��)</param>
    public void ShowMessage(string message, float duration)
    {
        // ����Ѿ���һ����Ϣ����ʾ��ֹͣ��
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        // ��ʼ�µ���Ϣ��ʾ
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    /// <summary>
    /// ��ʾָ����UI��壬�������������
    /// </summary>
    /// <param name="panelToShow">Ҫ��ʾ�����</param>
    public void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null) return;

        // �����������
        HideAllPanels();

        // ��ʾָ�������
        panelToShow.SetActive(true);
    }

    /// <summary>
    /// ��������UI���
    /// </summary>
    public void HideAllPanels()
    {
        if (allUIPanels != null)
        {
            foreach (var panel in allUIPanels)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// �����ض���UI���
    /// </summary>
    /// <param name="panelToHide">Ҫ���ص����</param>
    public void HidePanel(GameObject panelToHide)
    {
        if (panelToHide != null)
        {
            panelToHide.SetActive(false);
        }
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        // ȷ������Ϣ�����ı�
        if (messagePanel == null || messageText == null)
        {
            Debug.LogWarning("��Ϣ�����ı����δ����!");
            yield break;
        }

        // ������Ϣ
        messageText.text = message;
        messagePanel.SetActive(true);

        // �ȴ�ָ��ʱ��
        yield return new WaitForSeconds(duration);

        // ������Ϣ
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }

    /// <summary>
    /// ��ӵ��뵭��Ч����ʾ��Ϣ
    /// </summary>
    /// <param name="message">��Ϣ����</param>
    /// <param name="duration">��ʾʱ��</param>
    /// <param name="fadeTime">���뵭��ʱ��</param>
    public void ShowFadingMessage(string message, float duration, float fadeTime = 0.5f)
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ShowFadingMessageCoroutine(message, duration, fadeTime));
    }

    private IEnumerator ShowFadingMessageCoroutine(string message, float duration, float fadeTime)
    {
        if (messagePanel == null || messageText == null)
        {
            Debug.LogWarning("��Ϣ�����ı����δ����!");
            yield break;
        }

        // ������Ϣ
        messageText.text = message;

        // ��ȡCanvasGroup�����û�������
        CanvasGroup canvasGroup = messagePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = messagePanel.AddComponent<CanvasGroup>();
        }

        // ����͸����
        canvasGroup.alpha = 0;
        messagePanel.SetActive(true);

        // ����
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            canvasGroup.alpha = elapsedTime / fadeTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // �ȴ���ʾʱ��
        yield return new WaitForSeconds(duration - (fadeTime * 2));

        // ����
        elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            canvasGroup.alpha = 1 - (elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������Ϣ
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }
}