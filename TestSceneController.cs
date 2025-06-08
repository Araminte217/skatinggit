using UnityEngine;
using TMPro;

public class TestSceneController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;

    private void Start()
    {
        // 显示测试场景信息
        if (infoText != null)
        {
            infoText.text = "测试场景\n按 ESC 返回主菜单\n按 1 测试功能1\n按 2 测试功能2";
        }

        Debug.Log("测试场景已加载");
    }

    private void Update()
    {
        // 按ESC返回主菜单
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManagerController.Instance.LoadMainMenu();
        }

        // 测试功能按键
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
        Debug.Log("测试功能1已执行");
        // 在这里实现测试功能1
    }

    private void TestFunction2()
    {
        Debug.Log("测试功能2已执行");
        // 在这里实现测试功能2
    }
}