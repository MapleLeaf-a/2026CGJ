using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CrocodileState
{
    Idle, //非警戒
    Alert, //警戒
    Angry, //被激怒
    Stunned, //懵的状态
    Returning,  //返回锚点
    Diving //下水
}


public class Crocodile : MonoBehaviour
{
    public CrocodileState currentState;
    public Transform player;        // 玩家位置
    public Transform anchorPos;     // 锚点位置
    public float alertRange = 2.5f;   // 警戒范围
    public float anchorRange = 5f;  // 锚点范围（离开锚点太远会返回）

    private float defaultStateDuration = 1f; // 默认状态持续时间

    
    [Header("水池与下水")]
    public Transform poolCenter; // 水池中心
    public float diveSpeed = 2f; // 下水时的冲刺速度
    
    [Header("状态图片")]
    public SpriteRenderer stateSpriteRenderer; // 用于显示状态图片的SpriteRenderer

    [Header("状态图片数据")]
    [SerializeField] private CrocodileStateDataSO stateData;

    private GameManager gameManager;
    private Anchor assignedAnchor; // 记住自己是哪个锚点的人

    private SpriteRenderer spriteRenderer; // 用于翻转鳄鱼的SpriteRenderer

    private float stateTimer = 0f;  // 状态计时器（用于发呆、懵等）

    static readonly Vector3[] directions = new Vector3[]
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };
    

    public List<Image> CrocodileStateImages;

    void Start()
    {
        currentState = CrocodileState.Idle;
        stateSpriteRenderer.sprite = null; // 初始状态没有图片

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("场景中找不到带有 'Player' 标签的物体！请检查标签设置。");
        }

        anchorPos = assignedAnchor.transform;

        GameObject poolObj = GameObject.FindGameObjectWithTag("Water");
        if (poolObj != null)
        {
            poolCenter = poolObj.transform;
        }
        else
        {
            Debug.LogError("场景中找不到带有 'Water' 标签的物体！请检查标签设置。");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 【核心判定】检查是否需要立刻触发“回锚”状态
        // 规则：只要不是正在“回锚”，不是“懵”“愤怒”“下水”的状态，且离锚点超过最大距离，就立刻强制切入！
        if (currentState != CrocodileState.Returning && 
            currentState != CrocodileState.Stunned && 
            currentState != CrocodileState.Angry && 
            currentState != CrocodileState.Diving) 
        {
            float distToAnchor = Vector3.Distance(transform.position, anchorPos.position);
            if (distToAnchor > anchorRange)
            {
                ChangeState(CrocodileState.Returning); // 触发状态切换
                // 注意：不要在这里写 return; 让它继续往下走，进入 switch 执行 Returning 的代码
            }
        }

        switch (currentState)
        {
            case CrocodileState.Idle:
                UpdateIdle();
                break;
            case CrocodileState.Alert:
                UpdateAlert();
                break;
            case CrocodileState.Angry:
                UpdateAngry();
                break;
            case CrocodileState.Stunned:
                UpdateStunned();
                break;
            case CrocodileState.Returning:
                UpdateReturning();
                break;
            case CrocodileState.Diving:
                UpdateDiving();
                break;
        }
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    public void SetAssignedAnchor(Anchor anchor) 
    {
        assignedAnchor = anchor;
    }

    private void UpdateIdle()
    {
        // 检测触发条件：玩家是否进入警戒范围？
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < alertRange) 
        {
            ChangeState(CrocodileState.Alert); // 切换到警戒
            return;
        }

        // ---- 执行当前的动作 ----
        // 每次状态开始时，我们先记录一下这次的行动类型
        if (stateTimer <= 0)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (rand < 0.7f)
            {
                // 70% 概率：晒太阳 (无所事事)
                BathSun();
            }
            else
            {
                // 30% 概率：缓慢无定向游动
                MoveIdly();
            }
            // 重置计时器，每次行动维持几秒
            stateTimer = defaultStateDuration; 
        }
        else
        {
            stateTimer -= Time.deltaTime;
        }
    }

    private void UpdateAlert()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > alertRange)
        {
            ChangeState(CrocodileState.Idle);
        }

        if (stateTimer <= 0)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (rand < 0.5f) 
            {
                // 50% 晒太阳（在栖息地周围发呆）
                BathSun();
            }
            else if (rand < 0.8f) // 0.5 + 0.3
            {
                // 30% 无定向游动（好奇查看四周）
                MoveIdly();
            }
            else if (rand < 0.99f) // 0.8+0.19
            {
                // 19% 向人方向移动（试探性靠近）
                Vector3 direction = (player.position - transform.position).normalized;
                float speed = 0.5f; // 移动速度
                MoveTowards(direction, speed);
                Debug.Log("试探性靠近玩家");
            }
            else // 剩下的 1%
            {
                // 1% 直接暴怒！(触发条件转换到 Angry)
                ChangeState(CrocodileState.Angry);
                return;
            }
            stateTimer = defaultStateDuration;
        }
        else
        {
            stateTimer -= Time.deltaTime;
        }
    }

    private void UpdateAngry()
    {
        Debug.Log("愤怒状态：追击玩家");
        Vector3 direction = (player.position - transform.position).normalized;
        float speed = 2.5f; // 移动速度
        MoveTowards(direction, speed);
    }

    private void UpdateStunned()
    {
        Debug.Log("懵了");

        if (stateTimer <= 0)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > alertRange)
            {
                ChangeState(CrocodileState.Idle);
            }
            else
            {
                ChangeState(CrocodileState.Alert);
            }
            stateTimer = defaultStateDuration;
        }
        else
        {
            stateTimer -= Time.deltaTime;
        }
    }

    private void UpdateReturning()
    {
        // 1. 向锚点移动
        Vector3 direction = (anchorPos.position - transform.position).normalized;
        float returningSpeed = 3f;
        MoveTowards(direction, returningSpeed); 

        // 2. 判定是否已经回到安全的领地内
        float currentDist = Vector3.Distance(transform.position, anchorPos.position);
        
        if (currentDist <= anchorRange * 0.8f) // 只要回到半径的80%内，就算到家了
        {
            // --- 回到锚地后的切换逻辑 ---
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > alertRange)
            {
                ChangeState(CrocodileState.Idle);
            }
            else
            {
                ChangeState(CrocodileState.Alert);
            }
        }    
    }

    private void UpdateDiving()
    {
        Vector3 dirToPool = (poolCenter.position - transform.position).normalized;
        MoveTowards(dirToPool, diveSpeed);
    }

    private void ChangeState(CrocodileState newState)
    {
        Debug.Log($"state: {newState}");
    
        currentState = newState;
        stateTimer = defaultStateDuration; // 重置状态计时器
    
        // 更新状态图片
        int stateIndex = (int)newState;
        if (stateIndex < stateData.stateImages.Count)
        {
            stateSpriteRenderer.sprite = stateData.stateImages[stateIndex];
        }
        else
        {
            stateSpriteRenderer.sprite = null; // 如果索引超出范围，清空图片
        }
    }

    /// <summary>
    /// 晒太阳
    /// </summary>
    private void BathSun()
    {
        Debug.Log("晒太阳");
    }

    private void MoveTowards(Vector3 direction, float speed)
    {
        spriteRenderer.flipX = direction.x > 0;
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void MoveIdly()
    {
        int index = UnityEngine.Random.Range(0, directions.Length);
        spriteRenderer.flipX = directions[index].x > 0;
        transform.Translate(directions[index] * 0.1f);
        Debug.Log("无目的游走");        
    }

    public void GetDamaged()
    {
        float rand = UnityEngine.Random.Range(0f, 1f);
        if (rand < 0.4f) 
        {
            // 40% 懵
            ChangeState(CrocodileState.Stunned);
            return;
        }
        else if (rand < 0.6f) // 0.4 + 0.2
        {
            // 20% 向人方向移动（试探性靠近）
            Vector3 direction = (player.position - transform.position).normalized;
            float speed = 0.5f; // 移动速度
            MoveTowards(direction, speed);
            Debug.Log("试探性靠近玩家");
        }
        else if (rand < 0.8f) // 0.6 + 0.2
        {
            // 20% 被激怒
            ChangeState(CrocodileState.Angry);
            return;
        }
        else // 剩下的 20%
        {
            // 20% 下水
            ChangeState(CrocodileState.Diving);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测碰撞到的物体是不是水池
        if (other.CompareTag("Water")) 
        {
            // 通知 GameManager：有一头鳄鱼死了（把锚点传回去）
            if (gameManager != null)
            {
                gameManager.OnCrocodileDestroyed(assignedAnchor);
            }

            Destroy(gameObject); // 销毁鳄鱼
            Debug.Log("鳄鱼下水，销毁");
        }
    }
}
