using UnityEngine;
using TMPro;

public class TestSceneController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;

    private void Start()
    {
        // ��ʾ���Գ�����Ϣ
        if (infoText != null)
        {
            infoText.text = "���Գ���\n�� ESC �������˵�\n�� 1 ���Թ���1\n�� 2 ���Թ���2";
        }

        Debug.Log("���Գ����Ѽ���");
    }

    private void Update()
    {
        // ��ESC�������˵�
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManagerController.Instance.LoadMainMenu();
        }

        // ���Թ��ܰ���
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestFunction1();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestFunction2();
        }
    }

    private void TestFunction1()
    {
        Debug.Log("���Թ���1��ִ��");
        // ������ʵ�ֲ��Թ���1
    }

    private void TestFunction2()
    {
        Debug.Log("���Թ���2��ִ��");
        // ������ʵ�ֲ��Թ���2
    }
}