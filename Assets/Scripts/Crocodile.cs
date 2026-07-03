using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float alertRange = 2.5f;   // 警戒范围
    private float attackRange = 0.5f;  // 攻击范围
    private float anchorRange = 3f;  // 锚点范围（离开锚点太远会返回）
    private float defaultStateDuration = 1f; // 默认状态持续时间


    private float stateTimer = 0f;  // 状态计时器（用于发呆、懵等）
    
    static readonly Vector3[] directions = new Vector3[]
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    void Start()
    {
        currentState = CrocodileState.Idle;
    }

    void Update()
    {
        // 【核心判定】检查是否需要立刻触发“回锚”状态
        // 规则：只要不是正在“回锚”，不是“懵”的状态，且离锚点超过最大距离，就立刻强制切入！
        if (currentState != CrocodileState.Returning && 
            currentState != CrocodileState.Stunned) // 晕了就别动了
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
        Debug.Log("生气！");
    }

    private void UpdateStunned()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    private void ChangeState(CrocodileState newState)
    {
        Debug.Log($"state: {newState}");

        currentState = newState;
        stateTimer = 0f; // 重置状态计时器
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
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void MoveIdly()
    {
        transform.Translate(directions[UnityEngine.Random.Range(0, directions.Length)] * 0.1f);
        Debug.Log("无目的游走");        
    }
}
