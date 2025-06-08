using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;

    void Start()
    {
        // 启用已解锁的关卡按钮
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool unlocked = PlayerPrefs.GetInt("UnlockedLevel" + (i + 1), i == 0 ? 1 : 0) == 1;
            levelButtons[i].interactable = unlocked;
        }
    }

    public void StartLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}