using UnityEngine;
//让挂载这个脚本的GameObject（游戏物体）在原地上下周期性地浮动（“bob up and down”），形成平滑的上下摆动画。

public class BobUpDown : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] private float frequency = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(
            0,
            amplitude * Mathf.Sin(Time.time * frequency),
            0
        );
    }
}