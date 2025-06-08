using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SceneManagerController : MonoBehaviour
{
    // 单例模式
    public static SceneManagerController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float minimumLoadTime = 1.5f; // 最小加载时间（为了显示加载效果）

    // 场景名称常量
    public const string MAIN_MENU_SCENE = "MainMenu";     // 开始界面
    public const string LEVEL_1_SCENE = "Level1";         // 第一关
    public const string LEVEL_2_SCENE = "Level2";         // 第二关
    public const string TEST_SCENE = "TestScene";         // 测试场景

    // 是否正在加载场景
    private bool isLoading = false;

    void Awake()
    {
        // 单例实现
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

        // 确保加载屏幕初始状态是隐藏的
        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }
    }

    void Update()
    {
        // 如果当前场景是主菜单，并且按下O键，加载测试场景
        if (SceneManager.GetActiveScene().name == MAIN_MENU_SCENE && Input.GetKeyDown(KeyCode.O))
        {
            LoadTestScene();
            Debug.Log("Loading test scene");
        }
    }

    /// <summary>
    /// 加载指定场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    public void LoadScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    /// <summary>
    /// 加载主菜单
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(MAIN_MENU_SCENE);
    }

    /// <summary>
    /// 加载第一关
    /// </summary>
    public void LoadLevel1()
    {
        LoadScene(LEVEL_1_SCENE);
    }

    /// <summary>
    /// 加载第二关
    /// </summary>
    public void LoadLevel2()
    {
        LoadScene(LEVEL_2_SCENE);
    }

    /// <summary>
    /// 加载测试场景
    /// </summary>
    public void LoadTestScene()
    {
        LoadScene(TEST_SCENE);
    }

    /// <summary>
    /// 加载下一关
    /// </summary>
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // 如果当前是测试场景，直接返回主菜单
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
            // 如果没有下一关，返回主菜单
            LoadMainMenu();
        }
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // 显示加载屏幕
        if (loadingScreen)
        {
            loadingScreen.SetActive(true);
        }

        // 记录开始加载的时间
        float startTime = Time.time;

        // 开始异步加载场景
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        // 防止场景加载完成后自动激活
        asyncOperation.allowSceneActivation = false;

        // 更新加载进度条
        while (!asyncOperation.isDone)
        {
            // 加载进度从0到0.9
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            if (loadingBarFill)
            {
                loadingBarFill.fillAmount = progress;
            }

            if (loadingText)
            {
                loadingText.text = $"Loading... {Mathf.Round(progress * 100)}%";
            }

            // 当加载接近完成且经过了最小加载时间
            if (asyncOperation.progress >= 0.9f && Time.time - startTime >= minimumLoadTime)
            {
                // 允许场景激活
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 场景已加载完成
        isLoading = false;

        // 隐藏加载屏幕
        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }
    }

    /// <summary>
    /// 退出游戏
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
    /// 检查场景是否存在于构建设置中
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