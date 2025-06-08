using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button testSceneButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGame);

        if (testSceneButton != null)
            testSceneButton.onClick.AddListener(OnTestScene);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);
    }

    private void OnStartGame()
    {
        // 调用你的场景管理器切换到第一个关卡（假设是Level1）
        SceneManagerController.Instance.LoadLevel1();
        // 可选：UIManager.Instance.ShowMessage("开始游戏！", 2f);
    }

    private void OnTestScene()
    {
        SceneManagerController.Instance.LoadTestScene();
        // 可选：UIManager.Instance.ShowMessage("进入测试场景！", 2f);
    }

    private void OnQuitGame()
    {
        SceneManagerController.Instance.QuitGame();
        // 可选：UIManager.Instance.ShowMessage("退出游戏", 2f);
    }
}