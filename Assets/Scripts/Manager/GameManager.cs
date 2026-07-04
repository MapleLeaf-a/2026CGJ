using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("预制体")]
    public GameObject anchorPrefab;    
    public GameObject crocodilePrefab; 

    [Header("生成参数")]
    public int minAnchorCount = 3;     
    public int maxAnchorCount = 5;     
    public float spawnRadius = 3f;     
    public int minCrocPerAnchor = 2;   
    public int maxCrocPerAnchor = 4;   
    public float minAnchorDistance = 4f; // 【新增】两个锚点之间必须至少距离 4 个单位

    [Header("地图限制 (防止出界)")]
    public Vector2 mapBoundary = new Vector2(8f, 4f); // 地图X和Y的边界 (根据你的相机大小调整)
    public Bounds waterBounds; // 【新增】存放水池的范围

    [Header("UI")]
    public TextMeshProUGUI progressText;   

    [Header("玩家生成")]
    public GameObject player; 

    private int totalAnchors = 0;      
    private int clearedAnchors = 0;    
    private List<Vector3> spawnedAnchorPositions = new List<Vector3>(); // 【新增】记录已生成锚点的位置

    void Start()
    {
        // 查找水池并获取包围盒（假设水池物体叫 PoolWater）
        GameObject poolObj = GameObject.Find("PoolWater");
        if (poolObj != null)
        {
            // 注意：如果水池是多个Collider组合的，你可以手动在Inspector里填入范围
            // 这里我们用水池自身的 Collider 边界（如果你是用图片的话）
            BoxCollider2D poolCollider = poolObj.GetComponent<BoxCollider2D>();
            if(poolCollider != null) {
                waterBounds = poolCollider.bounds;
            }
        }

        GenerateGame();
    }

    void GenerateGame()
    {
        int anchorCount = Random.Range(minAnchorCount, maxAnchorCount + 1);
        totalAnchors = anchorCount;
        clearedAnchors = 0;
        spawnedAnchorPositions.Clear(); // 清空上次的生成记录

        for (int i = 0; i < anchorCount; i++)
        {
            SpawnAnchorWithCrocs();
        }

        SpawnPlayerSafely();

        UpdateUI();
    }

    void SpawnAnchorWithCrocs()
    {
        // 【核心改动 1】：获取一个完全安全的位置
        Vector3 anchorPos = GetSafeSpawnPosition();

        // 记录这个位置，防止下一个锚点离它太近
        spawnedAnchorPositions.Add(anchorPos);
        
        GameObject newAnchorObj = Instantiate(anchorPrefab, anchorPos, Quaternion.identity);
        Anchor anchorScript = newAnchorObj.GetComponent<Anchor>();
        if (anchorScript == null) anchorScript = newAnchorObj.AddComponent<Anchor>();
        
        int crocCount = Random.Range(minCrocPerAnchor, maxCrocPerAnchor + 1);
        anchorScript.SetupAnchor(this, crocCount); 

        for (int i = 0; i < crocCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = anchorPos + new Vector3(randomCircle.x, randomCircle.y, 0);

            // 【核心改动 2】：让鳄鱼也不能刷在边界外或水里
            spawnPos = ClampToBounds(spawnPos); // 把鳄鱼压回安全区

            GameObject newCroc = Instantiate(crocodilePrefab, spawnPos, Quaternion.identity);
            Crocodile crocScript = newCroc.GetComponent<Crocodile>();
            if (crocScript != null)
            {
                crocScript.SetGameManager(this); 
                crocScript.SetAssignedAnchor(anchorScript); 
            }
        }
    }

    // ---- 【新增】寻找安全位置的函数 ----
    Vector3 GetSafeSpawnPosition()
    {
        Vector3 pos = Vector3.zero;
        bool isSafe = false;
        int maxAttempts = 100; // 最多尝试 100 次随机，防止死循环

        for (int i = 0; i < maxAttempts; i++)
        {
            // 在边界内随机生成一个坐标
            float x = Random.Range(-mapBoundary.x, mapBoundary.x);
            float y = Random.Range(-mapBoundary.y, mapBoundary.y);
            pos = new Vector3(x, y, 0f);

            // 1. 检查是否在水池里
            if (waterBounds.Contains(pos)) continue; // 在水里，位置不合法，重试

            // 2. 检查是否离其他锚点太近
            bool tooCloseToOthers = false;
            foreach (var existingPos in spawnedAnchorPositions)
            {
                if (Vector3.Distance(pos, existingPos) < minAnchorDistance)
                {
                    tooCloseToOthers = true;
                    break; // 离得太近了，跳出循环，重试
                }
            }
            if (tooCloseToOthers) continue; // 离太近，位置不合法，重试

            // 如果走到这里，说明位置是安全的
            isSafe = true;
            break;
        }

        // 如果尝试了 100 次都没找到安全位置，退而求其次，返回边界内的随机点（但可能会重叠）
        if (!isSafe)
        {
            float x = Random.Range(-mapBoundary.x, mapBoundary.x);
            float y = Random.Range(-mapBoundary.y, mapBoundary.y);
            pos = new Vector3(x, y, 0f);
            Debug.LogWarning("找不到不重叠的安全位置，强制生成了一个");
        }

        return pos;
    }
    
    // ---- 【新增】玩家的安全生成逻辑 ----
    void SpawnPlayerSafely()
    {
        if (player == null) return;

        Vector3 safePos = Vector3.zero;
        bool isSafe = false;
        int maxAttempts = 200; // 尝试200次，足够多了

        // 预设：在这个半径内不能有任何障碍物
        float playerCheckRadius = 1.0f; 

        for (int i = 0; i < maxAttempts; i++)
        {
            // 1. 在安全地图范围内随机取点（沿用刚才锚点的边界）
            float x = Random.Range(-mapBoundary.x, mapBoundary.x);
            float y = Random.Range(-mapBoundary.y, mapBoundary.y);
            safePos = new Vector3(x, y, 0f);

            // 2. 必须避开水池
            if (waterBounds.Contains(safePos)) continue;

            // 3. 【关键步】：物理探测这里有没有东西
            // Physics2D.OverlapCircle 会在 safePos 处画一个虚拟圈，
            // 如果圈内碰到了任何 Collider，说明这里不干净。
            // 第 3 个参数是 LayerMask，如果你不想让它撞到墙，可以传参屏蔽，但这里最好全部扫描。
            Collider2D hitCollider = Physics2D.OverlapCircle(safePos, playerCheckRadius);

            if (hitCollider == null) 
            {
                // 如果返回 null，说明这个地方半径 1 米内没有任何东西！
                isSafe = true;
                break; 
            }
        }

        // 如果尝试 200 次都没找到安全的（极低概率），兜底措施：强行走到一个锚点旁边
        if (!isSafe)
        {
            // 取第一个锚点的位置，并往左上方偏移一点
            if (spawnedAnchorPositions.Count > 0)
            {
                safePos = spawnedAnchorPositions[0] + new Vector3(-3f, 3f, 0f);
                // 强制再按一次边界保护
                safePos.x = Mathf.Clamp(safePos.x, -mapBoundary.x, mapBoundary.x);
                safePos.y = Mathf.Clamp(safePos.y, -mapBoundary.y, mapBoundary.y);
            }
            else
            {
                safePos = Vector3.zero; // 最后保底
            }
            Debug.LogWarning("找不到绝对安全的出生点，强制生成了偏离点");
        }

        // 4. 终于找到了安全点，生成玩家！
        player.transform.position = safePos;
    }

    // ---- 【新增】强制把位置限制在边界内，且在水池外的函数 ----
    Vector3 ClampToBounds(Vector3 pos)
    {
        // 限制 X 和 Y 轴不超出屏幕
        pos.x = Mathf.Clamp(pos.x, -mapBoundary.x, mapBoundary.x);
        pos.y = Mathf.Clamp(pos.y, -mapBoundary.y, mapBoundary.y);

        // 如果这个点落入了水池，简单的做法是将它往左上/右上推一点点偏移
        if (waterBounds.Contains(pos))
        {
            pos.x += waterBounds.size.x * 0.5f; // 向右推开
            pos.y += waterBounds.size.y * 0.5f; // 向上推开
            // 再次 Clamp 确保推开后没出屏幕
            pos.x = Mathf.Clamp(pos.x, -mapBoundary.x, mapBoundary.x);
            pos.y = Mathf.Clamp(pos.y, -mapBoundary.y, mapBoundary.y);
        }
        return pos;
    }

    public void OnCrocodileDestroyed(Anchor anchor)
    {
        if (anchor != null) anchor.OnCrocodileKilled();
    }

    public void OnAnchorCleared()
    {
        clearedAnchors++;
        UpdateUI();
        if (clearedAnchors >= totalAnchors)
        {
            Debug.Log("胜利！所有锚点的鳄鱼已被清除！");
        }
    }

    void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"已净化锚点: {clearedAnchors} / {totalAnchors}";
        }
    }
}