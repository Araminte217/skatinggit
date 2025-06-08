
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // UI引用
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image speedBar;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelCompleteScoreText;
    [SerializeField] private TextMeshProUGUI levelCompleteTimeText;
    [SerializeField] private GameObject pauseMenu;

    // 游戏设置
    [Header("Game Settings")]
    [SerializeField] private int requiredScore = 1000;     // 通关所需分数
    [SerializeField] private int coinScoreValue = 100;     // 每个金币的分数价值
    [SerializeField] private float trickScoreBonus = 100f;       // 特技得分加成
    [SerializeField] private float levelTimeLimit = 120f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool isFinalLevel = false;

    // 游戏状态
    private int currentScore;                  // 当前分数
    private int collectedCoins;
    private float levelTime;
    private bool isGameOver;
    private bool isLevelComplete;
    private bool isPaused;
    private UIManager uiManager;
    private AudioManager audioManager;
    private PlayerController player;

    // 高分记录
    private int highScore;

    void Awake()
    {
        // 单例模式
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

        // 初始化游戏状态
        currentScore = 0;
        collectedCoins = 0;
        levelTime = levelTimeLimit;
        isGameOver = false;
        isLevelComplete = false;
        isPaused = false;

        // 读取最高分数
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        highScore = PlayerPrefs.GetInt("HighScore_" + currentScene, 0);
    }

    void Start()
    {
        // 获取必要的组件
        uiManager = FindObjectOfType<UIManager>();
        audioManager = FindObjectOfType<AudioManager>();
        player = FindObjectOfType<PlayerController>();

        // 播放背景音乐
        if (audioManager != null && backgroundMusic != null)
        {
            audioManager.PlayMusic(backgroundMusic);
        }

        // 确保UI面板初始状态是隐藏的
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);

        // 更新UI
        UpdateScoreUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameOver || isLevelComplete) return;

        // 处理暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // 更新关卡时间
        if (!isPaused)
        {
            levelTime -= Time.deltaTime;
            UpdateTimerUI();

            // 检查时间是否用完
            if (levelTime <= 0)
            {
                GameOver();
            }
        }
    }

    // 增加分数
    public void AddScore(float points)
    {
        currentScore += Mathf.RoundToInt(points);
        UpdateScoreUI();

        // 播放得分音效
        if (points >= 100)
        {
            // 大分数得分音效
            audioManager?.PlaySound("BigScore");
        }
        else if (points > 0)
        {
            // 小分数得分音效
            audioManager?.PlaySound("Score");
        }

        // 如果达到了所需分数要求，更新UI显示
        if (currentScore >= requiredScore)
        {
            if (scoreText)
            {
                scoreText.color = Color.green;
            }
        }
    }

    public void CollectCoin()
    {
        collectedCoins++;
        // 增加金币对应的分数
        AddScore(coinScoreValue);
    }
    public void FinishTrick()
    {
        // 增加特技对应的分数
        AddScore(trickScoreBonus);
    }
    public void UpdateSpeedUI(float speed)
    {
        if (speedText)
        {
            speedText.text = $"Speed: {Mathf.Round(speed)} km/h";
        }

        if (speedBar)
        {
            speedBar.fillAmount = speed / maxSpeed;

            // 根据速度改变颜色
            if (speed > maxSpeed * 0.8f)
            {
                speedBar.color = Color.red;
            }
            else if (speed > maxSpeed * 0.5f)
            {
                speedBar.color = Color.yellow;
            }
            else
            {
                speedBar.color = Color.green;
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText)
        {
            scoreText.text = $"Score: {currentScore} / {requiredScore}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(levelTime / 60f);
            int seconds = Mathf.FloorToInt(levelTime % 60f);
            timerText.text = $"TimeLeft: {minutes:00}:{seconds:00}";

            // 当时间不多时改变颜色
            if (levelTime < 30f)
            {
                timerText.color = Color.red;
            }
            else if (levelTime < 60f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // 如果玩家存在，触发死亡
        if (player != null)
        {
            player.Die();
        }

        // 播放游戏结束音效
        if (audioManager != null)
        {
            audioManager.PlaySound("GameOver");
        }

        // 保存高分
        SaveHighScore();

        // 显示游戏结束UI
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);

            if (gameOverScoreText)
            {
                gameOverScoreText.text = $"FinalScore: {currentScore}\nCollectedCoins: {collectedCoins}\nHighScore: {highScore}";
            }
        }

        // 暂停游戏
        Time.timeScale = 0;
    }

    public void LevelComplete()
    {
        // 只有当收集足够的分数时才算通过关卡
        if (currentScore >= requiredScore && !isLevelComplete)
        {
            isLevelComplete = true;

            // 播放关卡完成音效
            if (audioManager != null)
            {
                audioManager.PlaySound("Victory");
            }

            // 保存高分
            SaveHighScore();

            // 解锁下一关
            if (!isFinalLevel)
            {
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    PlayerPrefs.SetInt("UnlockedLevel" + nextSceneIndex, 1);
                    PlayerPrefs.Save();
                }
            }

            // 显示通关UI
            if (levelCompletePanel)
            {
                levelCompletePanel.SetActive(true);

                if (levelCompleteScoreText)
                {
                    levelCompleteScoreText.text = $"FinalScore: {currentScore}\nCoinCollected: {collectedCoins}\nHighScore: {highScore}";
                }

                if (levelCompleteTimeText)
                {
                    int minutes = Mathf.FloorToInt(levelTime / 60f);
                    int seconds = Mathf.FloorToInt(levelTime % 60f);
                    levelCompleteTimeText.text = $"TimeLeft: {minutes:00}:{seconds:00}";
                }
            }

            // 暂停游戏
            Time.timeScale = 0;
        }
        else if (!isLevelComplete)
        {
            // 如果玩家到达终点但没有收集足够的分数
            if (audioManager != null)
            {
                audioManager.PlaySound("Fail");
            }

            // 显示提示
            if (gameOverPanel)
            {
                gameOverPanel.SetActive(true);

                if (gameOverScoreText)
                {
                    gameOverScoreText.text = $"FinalScore: {currentScore}\nCollectedCoins: {collectedCoins}\nHighScore: {highScore}";
                }
            }
        }
    }

    // 保存高分
    private void SaveHighScore()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int savedHighScore = PlayerPrefs.GetInt("HighScore_" + currentScene, 0);

        if (currentScore > savedHighScore)
        {
            PlayerPrefs.SetInt("HighScore_" + currentScene, currentScore);
            PlayerPrefs.Save();
            highScore = currentScore;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu)
        {
            pauseMenu.SetActive(isPaused);
        }

        // 暂停/恢复游戏
        Time.timeScale = isPaused ? 0 : 1;

        // 暂停/恢复音频
        if (audioManager != null)
        {
            audioManager.SetPaused(isPaused);
        }
    }

    // UI按钮回调函数
    public void RestartLevel()
    {
        // 确保恢复时间缩放
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        // 确保恢复时间缩放
        Time.timeScale = 1;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        // 确保恢复时间缩放
        Time.timeScale = 1;

        // 加载主菜单场景
        SceneManager.LoadScene(0);

        // 销毁当前的GameManager实例
        Destroy(gameObject);
    }

    public void ResetSavedData()
    {
        // 重置所有解锁的关卡数据
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            PlayerPrefs.DeleteKey("UnlockedLevel" + i);
            PlayerPrefs.DeleteKey("HighScore_" + i);
        }

        // 仅保留第一关解锁
        PlayerPrefs.SetInt("UnlockedLevel1", 1);
        PlayerPrefs.Save();
    }

    // 游戏整体重置(如关卡切换时)
    public void ResetGame()
    {
        currentScore = 0;
        collectedCoins = 0;
        levelTime = levelTimeLimit;
        isGameOver = false;
        isLevelComplete = false;
        isPaused = false;

        // 确保恢复时间缩放
        Time.timeScale = 1;

        // 重新获取引用
        uiManager = FindObjectOfType<UIManager>();
        audioManager = FindObjectOfType<AudioManager>();
        player = FindObjectOfType<PlayerController>();

        // 读取最高分数
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        highScore = PlayerPrefs.GetInt("HighScore_" + currentScene, 0);

        // 更新UI
        UpdateScoreUI();
        UpdateTimerUI();

        // 确保UI面板初始状态是隐藏的
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);
    }

    // 场景加载完成时调用
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGame();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore;
    }

    // 获取所需分数
    public int GetRequiredScore()
    {
        return requiredScore;
    }

    // 获取高分
    public int GetHighScore()
    {
        return highScore;
    }
}