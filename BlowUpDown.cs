using UnityEngine;
//�ù�������ű���GameObject����Ϸ���壩��ԭ�����������Եظ�������bob up and down�������γ�ƽ�������°ڶ�����

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