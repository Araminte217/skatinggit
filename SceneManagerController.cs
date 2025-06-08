using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SceneManagerController : MonoBehaviour
{
    // ����ģʽ
    public static SceneManagerController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float minimumLoadTime = 1.5f; // ��С����ʱ�䣨Ϊ����ʾ����Ч����

    // �������Ƴ���
    public const string MAIN_MENU_SCENE = "MainMenu";     // ��ʼ����
    public const string LEVEL_1_SCENE = "Level1";         // ��һ��
    public const string LEVEL_2_SCENE = "Level2";         // �ڶ���
    public const string TEST_SCENE = "TestScene";         // ���Գ���

    // �Ƿ����ڼ��س���
    private bool isLoading = false;

    void Awake()
    {
        // ����ʵ��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ȷ��������Ļ��ʼ״̬�����ص�
        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }
    }

    void Update()
    {
        // �����ǰ���������˵������Ұ���O�������ز��Գ���
        if (SceneManager.GetActiveScene().name == MAIN_MENU_SCENE && Input.GetKeyDown(KeyCode.O))
        {
            LoadTestScene();
            Debug.Log("Loading test scene");
        }
    }

    /// <summary>
    /// ����ָ������
    /// </summary>
    /// <param name="sceneName">��������</param>
    public void LoadScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    /// <summary>
    /// �������˵�
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(MAIN_MENU_SCENE);
    }

    /// <summary>
    /// ���ص�һ��
    /// </summary>
    public void LoadLevel1()
    {
        LoadScene(LEVEL_1_SCENE);
    }

    /// <summary>
    /// ���صڶ���
    /// </summary>
    public void LoadLevel2()
    {
        LoadScene(LEVEL_2_SCENE);
    }

    /// <summary>
    /// ���ز��Գ���
    /// </summary>
    public void LoadTestScene()
    {
        LoadScene(TEST_SCENE);
    }

    /// <summary>
    /// ������һ��
    /// </summary>
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // �����ǰ�ǲ��Գ�����ֱ�ӷ������˵�
        if (SceneManager.GetActiveScene().name == TEST_SCENE)
        {
            LoadMainMenu();
            return;
        }

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex).name);
        }
        else
        {
            // ���û����һ�أ��������˵�
            LoadMainMenu();
        }
    }

    /// <summary>
    /// ���¼��ص�ǰ����
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }

    /// <summary>
    /// �첽���س���
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // ��ʾ������Ļ
        if (loadingScreen)
        {
            loadingScreen.SetActive(true);
        }

        // ��¼��ʼ���ص�ʱ��
        float startTime = Time.time;

        // ��ʼ�첽���س���
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        // ��ֹ����������ɺ��Զ�����
        asyncOperation.allowSceneActivation = false;

        // ���¼��ؽ�����
        while (!asyncOperation.isDone)
        {
            // ���ؽ��ȴ�0��0.9
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            if (loadingBarFill)
            {
                loadingBarFill.fillAmount = progress;
            }

            if (loadingText)
            {
                loadingText.text = $"Loading... {Mathf.Round(progress * 100)}%";
            }

            // �����ؽӽ�����Ҿ�������С����ʱ��
            if (asyncOperation.progress >= 0.9f && Time.time - startTime >= minimumLoadTime)
            {
                // ����������
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // �����Ѽ������
        isLoading = false;

        // ���ؼ�����Ļ
        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }
    }

    /// <summary>
    /// �˳���Ϸ
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

        Debug.Log("Game has been quit");
    }

    /// <summary>
    /// ��鳡���Ƿ�����ڹ���������
    /// </summary>
    public bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scene = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            if (scene == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}