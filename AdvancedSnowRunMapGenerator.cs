using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 3Dѩ���ܿ��Զ���ͼ������
/// ֧��3�������ܵ�������ϰ�/�ռ������յ㡢��������ḻװ��
/// ���ڿ�GameObject�ϣ�Inspector�������Prefab�Ͳ���
/// </summary>
public class AdvancedSnowRunMapGenerator : MonoBehaviour
{
    [Header("��ͼ��������")]
    public int sectionCount = 60;           // �ܵ���������ͼ�ܳ��ȣ�
    public float sectionLength = 10f;       // ÿ�γ���
    public float laneSpacing = 4f;          // �����ܵ����
    public bool enableCurves = true;        // �Ƿ��������
    public int minStraightSections = 5;     // �������ֱ������
    public int maxCurveSections = 3;        // ��������������

    [Header("Prefab��Դ")]
    public GameObject groundPrefab;         // �ܵ�����
    public GameObject startPrefab;          // ���
    public GameObject endPrefab;            // �յ�
    public GameObject bridgePrefab;         // �ţ���ѡ��
    public GameObject rampPrefab;           // �µ�����ѡ��

    public GameObject[] obstaclePrefabs;    // �ϰ����ѩ�ˡ�bench��sled�ȣ�
    public GameObject[] collectiblePrefabs; // �ռ�����ҡ�����ǹ��ȣ�
    public GameObject[] decorPrefabs;       // װ�������ѩ�˵ȣ��ܵ��⣩

    [Header("��������")]
    [Range(0, 1)]
    public float obstacleRate = 0.20f;      // �ϰ����ָ���
    [Range(0, 1)]
    public float collectibleRate = 0.15f;   // �ռ�����ָ���
    [Range(0, 1)]
    public float decorRate = 0.6f;          // װ�γ��ָ���

    [Header("����")]
    public Transform previewParent;         // ָ����������ĸ����壨��ѡ��

    private Vector3 currentPos = Vector3.zero;
    private float currentRotY = 0f; // ��ǰ�������������

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        Transform parent = previewParent ? previewParent : this.transform;
        currentPos = Vector3.zero;
        currentRotY = 0f;

        // 1. ���
        if (startPrefab)
            Instantiate(startPrefab, currentPos, Quaternion.Euler(0, currentRotY, 0), parent);

        // 2. �ܵ�����
        int straightLeft = Random.Range(minStraightSections, minStraightSections + 3);
        int curveLeft = 0;
        int curveDir = 0; // -1:�� 1:�� 0:ֱ

        for (int i = 0; i < sectionCount; i++)
        {
            // ---- ����߼� ----
            if (enableCurves)
            {
                if (straightLeft > 0)
                {
                    straightLeft--;
                }
                else if (curveLeft == 0)
                {
                    // ��������Ƿ�ʼ���
                    if (Random.value < 0.25f)
                    {
                        curveDir = Random.value < 0.5f ? -1 : 1;
                        curveLeft = Random.Range(1, maxCurveSections + 1);
                    }
                    else
                    {
                        straightLeft = Random.Range(minStraightSections, minStraightSections + 3);
                        curveDir = 0;
                    }
                }
                if (curveLeft > 0)
                {
                    currentRotY += curveDir * 10f; // ÿ��ת10��
                    curveLeft--;
                    if (curveLeft == 0) straightLeft = Random.Range(minStraightSections, minStraightSections + 3);
                }
            }
            // ---- ǰ������һ��������� ----
            currentPos += Quaternion.Euler(0, currentRotY, 0) * new Vector3(0, 0, sectionLength);

            // ---- ���ɵ��� ----
            GameObject ground = Instantiate(
                groundPrefab,
                currentPos,
                Quaternion.Euler(0, currentRotY, 0),
                parent
            );

            // ---- ���������ܵ����� ----
            for (int lane = -1; lane <= 1; lane++)
            {
                Vector3 laneCenter = currentPos + Quaternion.Euler(0, currentRotY, 0) * new Vector3(lane * laneSpacing, 0, 0);

                // 2.1 �ϰ���
                if (obstaclePrefabs.Length > 0 && Random.value < obstacleRate)
                {
                    Instantiate(
                        obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)],
                        laneCenter + Vector3.up * 0.5f,
                        Quaternion.Euler(0, Random.Range(0, 360), 0),
                        parent
                    );
                }
                // 2.2 �ռ���
                else if (collectiblePrefabs.Length > 0 && Random.value < collectibleRate)
                {
                    Instantiate(
                        collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)],
                        laneCenter + Vector3.up * 1.5f,
                        Quaternion.identity,
                        parent
                    );
                }
                // 2.3 ������Σ���/�µ���ż�����֣�
                else if (bridgePrefab && Random.value < 0.02f)
                {
                    Instantiate(bridgePrefab, laneCenter, Quaternion.Euler(0, currentRotY, 0), parent);
                }
                else if (rampPrefab && Random.value < 0.03f)
                {
                    Instantiate(rampPrefab, laneCenter, Quaternion.Euler(0, currentRotY, 0), parent);
                }
            }

            // ---- �ܵ�����װ�� ----
            for (int side = -1; side <= 1; side += 2)
            {
                if (decorPrefabs.Length > 0 && Random.value < decorRate)
                {
                    Vector3 decorPos = currentPos + Quaternion.Euler(0, currentRotY, 0) * new Vector3(2.5f * laneSpacing * side, 0, Random.Range(-sectionLength / 2, sectionLength / 2));
                    Instantiate(
                        decorPrefabs[Random.Range(0, decorPrefabs.Length)],
                        decorPos + Vector3.up * 0.5f,
                        Quaternion.Euler(0, Random.Range(0, 360), 0),
                        parent
                    );
                }
            }
        }

        // 3. �յ�
        Vector3 endPos = currentPos + Quaternion.Euler(0, currentRotY, 0) * new Vector3(0, 0, sectionLength);
        if (endPrefab)
            Instantiate(endPrefab, endPos, Quaternion.Euler(0, currentRotY, 0), parent);
    }
}