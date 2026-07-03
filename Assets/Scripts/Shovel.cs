using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Shovel : MonoBehaviour
{
     private Player player; // 引用父物体的 Player 脚本

    void Start()
    {
        // 获取父物体身上的 Player 脚本
        player = GetComponentInParent<Player>();
    }

    void Update()
    {
        if (player == null) return;

        // 1. 获取玩家脚本里计算好的目标位置（那个被限制在半径内的点）
        Vector3 targetPos = player.targetPosition;

        // 2. 计算铲子位置到目标位置的向量
        Vector3 direction = targetPos - transform.position;

        // 3. 计算旋转角度 (Z轴)
        // 注意：2D游戏中，Atan2 返回的是弧度，乘以 Mathf.Rad2Deg 转为角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

        // 4. 应用到铲子的旋转上
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
