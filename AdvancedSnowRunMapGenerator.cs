using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 3D雪景跑酷自动地图生成器
/// 支持3条并排跑道、随机障碍/收集物、起点终点、简单弯道、丰富装饰
/// 挂在空GameObject上，Inspector面板配置Prefab和参数
/// </summary>
public class AdvancedSnowRunMapGenerator : MonoBehaviour
{
    [Header("地图基础参数")]
    public int sectionCount = 60;           // 跑道段数（地图总长度）
    public float sectionLength = 10f;       // 每段长度
    public float laneSpacing = 4f;          // 三条跑道间距
    public bool enableCurves = true;        // 是否启用弯道
    public int minStraightSections = 5;     // 最短连续直道段数
    public int maxCurveSections = 3;        // 最多连续弯道段数

    [Header("Prefab资源")]
    public GameObject groundPrefab;         // 跑道地面
    public GameObject startPrefab;          // 起点
    public GameObject endPrefab;            // 终点
    public GameObject bridgePrefab;         // 桥（可选）
    public GameObject rampPrefab;           // 坡道（可选）

    public GameObject[] obstaclePrefabs;    // 障碍物（如雪人、bench、sled等）
    public GameObject[] collectiblePrefabs; // 收集物（如金币、礼物、糖果等）
    public GameObject[] decorPrefabs;       // 装饰物（树、雪人等，跑道外）

    [Header("参数调整")]
    [Range(0, 1)]
    public float obstacleRate = 0.20f;      // 障碍出现概率
    [Range(0, 1)]
    public float collectibleRate = 0.15f;   // 收集物出现概率
    [Range(0, 1)]
    public float decorRate = 0.6f;          // 装饰出现概率

    [Header("调试")]
    public Transform previewParent;         // 指定生成物体的父物体（可选）

    private Vector3 currentPos = Vector3.zero;
    private float currentRotY = 0f; // 当前朝向（用于弯道）

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        Transform parent = previewParent ? previewParent : this.transform;
        currentPos = Vector3.zero;
        currentRotY = 0f;

        // 1. 起点
        if (startPrefab)
            Instantiate(startPrefab, currentPos, Quaternion.Euler(0, currentRotY, 0), parent);

        // 2. 跑道主体
        int straightLeft = Random.Range(minStraightSections, minStraightSections + 3);
        int curveLeft = 0;
        int curveDir = 0; // -1:左 1:右 0:直

        for (int i = 0; i < sectionCount; i++)
        {
            // ---- 弯道逻辑 ----
            if (enableCurves)
            {
                if (straightLeft > 0)
                {
                    straightLeft--;
                }
                else if (curveLeft == 0)
                {
                    // 随机决定是否开始弯道
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
                    currentRotY += curveDir * 10f; // 每段转10度
                    curveLeft--;
                    if (curveLeft == 0) straightLeft = Random.Range(minStraightSections, minStraightSections + 3);
                }
            }
            // ---- 前进到下一个段落起点 ----
            currentPos += Quaternion.Euler(0, currentRotY, 0) * new Vector3(0, 0, sectionLength);

            // ---- 生成地面 ----
            GameObject ground = Instantiate(
                groundPrefab,
                currentPos,
                Quaternion.Euler(0, currentRotY, 0),
                parent
            );

            // ---- 生成三条跑道内容 ----
            for (int lane = -1; lane <= 1; lane++)
            {
                Vector3 laneCenter = currentPos + Quaternion.Euler(0, currentRotY, 0) * new Vector3(lane * laneSpacing, 0, 0);

                // 2.1 障碍物
                if (obstaclePrefabs.Length > 0 && Random.value < obstacleRate)
                {
                    Instantiate(
                        obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)],
                        laneCenter + Vector3.up * 0.5f,
                        Quaternion.Euler(0, Random.Range(0, 360), 0),
                        parent
                    );
                }
                // 2.2 收集物
                else if (collectiblePrefabs.Length > 0 && Random.value < collectibleRate)
                {
                    Instantiate(
                        collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)],
                        laneCenter + Vector3.up * 1.5f,
                        Quaternion.identity,
                        parent
                    );
                }
                // 2.3 特殊地形（桥/坡道，偶尔出现）
                else if (bridgePrefab && Random.value < 0.02f)
                {
                    Instantiate(bridgePrefab, laneCenter, Quaternion.Euler(0, currentRotY, 0), parent);
                }
                else if (rampPrefab && Random.value < 0.03f)
                {
                    Instantiate(rampPrefab, laneCenter, Quaternion.Euler(0, currentRotY, 0), parent);
                }
            }

            // ---- 跑道两侧装饰 ----
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

        // 3. 终点
        Vector3 endPos = currentPos + Quaternion.Euler(0, currentRotY, 0) * new Vector3(0, 0, sectionLength);
        if (endPrefab)
            Instantiate(endPrefab, endPos, Quaternion.Euler(0, currentRotY, 0), parent);
    }
}