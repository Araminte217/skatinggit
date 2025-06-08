
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // UI����
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

    // ��Ϸ����
    [Header("Game Settings")]
    [SerializeField] private int requiredScore = 1000;     // ͨ���������
    [SerializeField] private int coinScoreValue = 100;     // ÿ����ҵķ�����ֵ
    [SerializeField] private float trickScoreBonus = 100f;       // �ؼ��÷ּӳ�
    [SerializeField] private float levelTimeLimit = 120f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool isFinalLevel = false;

    // ��Ϸ״̬
    private int currentScore;                  // ��ǰ����
    private int collectedCoins;
    private float levelTime;
    private bool isGameOver;
    private bool isLevelComplete;
    private bool isPaused;
    private UIManager uiManager;
    private AudioManager audioManager;
    private PlayerController player;

    // �߷ּ�¼
    private int highScore;

    void Awake()
    {
        // ����ģʽ
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

        // ��ʼ����Ϸ״̬
        currentScore = 0;
        collectedCoins = 0;
        levelTime = levelTimeLimit;
        isGameOver = false;
        isLevelComplete = false;
        isPaused = false;

        // ��ȡ��߷���
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        highScore = PlayerPrefs.GetInt("HighScore_" + currentScene, 0);
    }

    void Start()
    {
        // ��ȡ��Ҫ�����
        uiManager = FindObjectOfType<UIManager>();
        audioManager = FindObjectOfType<AudioManager>();
        player = FindObjectOfType<PlayerController>();

        // ���ű�������
        if (audioManager != null && backgroundMusic != null)
        {
            audioManager.PlayMusic(backgroundMusic);
        }

        // ȷ��UI����ʼ״̬�����ص�
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);

        // ����UI
        UpdateScoreUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameOver || isLevelComplete) return;

        // ������ͣ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // ���¹ؿ�ʱ��
        if (!isPaused)
        {
            levelTime -= Time.deltaTime;
            UpdateTimerUI();

            // ���ʱ���Ƿ�����
            if (levelTime <= 0)
            {
                GameOver();
            }
        }
    }

    // ���ӷ���
    public void AddScore(float points)
    {
        currentScore += Mathf.RoundToInt(points);
        UpdateScoreUI();

        // ���ŵ÷���Ч
        if (points >= 100)
        {
            // ������÷���Ч
            audioManager?.PlaySound("BigScore");
        }
        else if (points > 0)
        {
            // С�����÷���Ч
            audioManager?.PlaySound("Score");
        }

        // ����ﵽ���������Ҫ�󣬸���UI��ʾ
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
        // ���ӽ�Ҷ�Ӧ�ķ���
        AddScore(coinScoreValue);
    }
    public void FinishTrick()
    {
        // �����ؼ���Ӧ�ķ���
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

            // �����ٶȸı���ɫ
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

            // ��ʱ�䲻��ʱ�ı���ɫ
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

        // �����Ҵ��ڣ���������
        if (player != null)
        {
            player.Die();
        }

        // ������Ϸ������Ч
        if (audioManager != null)
        {
            audioManager.PlaySound("GameOver");
        }

        // ����߷�
        SaveHighScore();

        // ��ʾ��Ϸ����UI
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);

            if (gameOverScoreText)
            {
                gameOverScoreText.text = $"FinalScore: {currentScore}\nCollectedCoins: {collectedCoins}\nHighScore: {highScore}";
            }
        }

        // ��ͣ��Ϸ
        Time.timeScale = 0;
    }

    public void LevelComplete()
    {
        // ֻ�е��ռ��㹻�ķ���ʱ����ͨ���ؿ�
        if (currentScore >= requiredScore && !isLevelComplete)
        {
            isLevelComplete = true;

            // ���Źؿ������Ч
            if (audioManager != null)
            {
                audioManager.PlaySound("Victory");
            }

            // ����߷�
            SaveHighScore();

            // ������һ��
            if (!isFinalLevel)
            {
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    PlayerPrefs.SetInt("UnlockedLevel" + nextSceneIndex, 1);
                    PlayerPrefs.Save();
                }
            }

            // ��ʾͨ��UI
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

            // ��ͣ��Ϸ
            Time.timeScale = 0;
        }
        else if (!isLevelComplete)
        {
            // �����ҵ����յ㵫û���ռ��㹻�ķ���
            if (audioManager != null)
            {
                audioManager.PlaySound("Fail");
            }

            // ��ʾ��ʾ
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

    // ����߷�
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

        // ��ͣ/�ָ���Ϸ
        Time.timeScale = isPaused ? 0 : 1;

        // ��ͣ/�ָ���Ƶ
        if (audioManager != null)
        {
            audioManager.SetPaused(isPaused);
        }
    }

    // UI��ť�ص�����
    public void RestartLevel()
    {
        // ȷ���ָ�ʱ������
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        // ȷ���ָ�ʱ������
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
        // ȷ���ָ�ʱ������
        Time.timeScale = 1;

        // �������˵�����
        SceneManager.LoadScene(0);

        // ���ٵ�ǰ��GameManagerʵ��
        Destroy(gameObject);
    }

    public void ResetSavedData()
    {
        // �������н����Ĺؿ�����
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            PlayerPrefs.DeleteKey("UnlockedLevel" + i);
            PlayerPrefs.DeleteKey("HighScore_" + i);
        }

        // ��������һ�ؽ���
        PlayerPrefs.SetInt("UnlockedLevel1", 1);
        PlayerPrefs.Save();
    }

    // ��Ϸ��������(��ؿ��л�ʱ)
    public void ResetGame()
    {
        currentScore = 0;
        collectedCoins = 0;
        levelTime = levelTimeLimit;
        isGameOver = false;
        isLevelComplete = false;
        isPaused = false;

        // ȷ���ָ�ʱ������
        Time.timeScale = 1;

        // ���»�ȡ����
        uiManager = FindObjectOfType<UIManager>();
        audioManager = FindObjectOfType<AudioManager>();
        player = FindObjectOfType<PlayerController>();

        // ��ȡ��߷���
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        highScore = PlayerPrefs.GetInt("HighScore_" + currentScene, 0);

        // ����UI
        UpdateScoreUI();
        UpdateTimerUI();

        // ȷ��UI����ʼ״̬�����ص�
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);
    }

    // �����������ʱ����
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

    // ��ȡ��ǰ����
    public int GetCurrentScore()
    {
        return currentScore;
    }

    // ��ȡ�������
    public int GetRequiredScore()
    {
        return requiredScore;
    }

    // ��ȡ�߷�
    public int GetHighScore()
    {
        return highScore;
    }
}