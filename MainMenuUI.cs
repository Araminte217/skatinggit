using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("��ť����")]
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
        // ������ĳ����������л�����һ���ؿ���������Level1��
        SceneManagerController.Instance.LoadLevel1();
        // ��ѡ��UIManager.Instance.ShowMessage("��ʼ��Ϸ��", 2f);
    }

    private void OnTestScene()
    {
        SceneManagerController.Instance.LoadTestScene();
        // ��ѡ��UIManager.Instance.ShowMessage("������Գ�����", 2f);
    }

    private void OnQuitGame()
    {
        SceneManagerController.Instance.QuitGame();
        // ��ѡ��UIManager.Instance.ShowMessage("�˳���Ϸ", 2f);
    }
}