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
    [SerializeField] private GameObject[] allUIPanels;  // 存储所有可能显示的UI面板，用于控制只显示一个

    private void Start()
    {
        // 初始化时隐藏所有UI面板
        HideAllPanels();
    }

    /// <summary>
    /// 显示临时消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="duration">显示时长(秒)</param>
    public void ShowMessage(string message, float duration)
    {
        // 如果已经有一个消息在显示，停止它
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        // 开始新的消息显示
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    /// <summary>
    /// 显示指定的UI面板，并隐藏其他面板
    /// </summary>
    /// <param name="panelToShow">要显示的面板</param>
    public void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null) return;

        // 隐藏所有面板
        HideAllPanels();

        // 显示指定的面板
        panelToShow.SetActive(true);
    }

    /// <summary>
    /// 隐藏所有UI面板
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
    /// 隐藏特定的UI面板
    /// </summary>
    /// <param name="panelToHide">要隐藏的面板</param>
    public void HidePanel(GameObject panelToHide)
    {
        if (panelToHide != null)
        {
            panelToHide.SetActive(false);
        }
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        // 确保有消息面板和文本
        if (messagePanel == null || messageText == null)
        {
            Debug.LogWarning("消息面板或文本组件未设置!");
            yield break;
        }

        // 设置消息
        messageText.text = message;
        messagePanel.SetActive(true);

        // 等待指定时间
        yield return new WaitForSeconds(duration);

        // 隐藏消息
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }

    /// <summary>
    /// 添加淡入淡出效果显示消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="duration">显示时长</param>
    /// <param name="fadeTime">淡入淡出时间</param>
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
            Debug.LogWarning("消息面板或文本组件未设置!");
            yield break;
        }

        // 设置消息
        messageText.text = message;

        // 获取CanvasGroup，如果没有则添加
        CanvasGroup canvasGroup = messagePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = messagePanel.AddComponent<CanvasGroup>();
        }

        // 重置透明度
        canvasGroup.alpha = 0;
        messagePanel.SetActive(true);

        // 淡入
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            canvasGroup.alpha = elapsedTime / fadeTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // 等待显示时间
        yield return new WaitForSeconds(duration - (fadeTime * 2));

        // 淡出
        elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            canvasGroup.alpha = 1 - (elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 隐藏消息
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }
}