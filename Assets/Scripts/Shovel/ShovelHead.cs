using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelHead : MonoBehaviour
{
    public Transform playerTransform; // 玩家位置

    float minRadius = 1f; // 铲子运动的最小半径

    void Start()
    {
        
    }


    void Update()
    {
        if (Time.timeScale == 0) return; // 游戏暂停时不处理铲子头部位置更新

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        float distance = Vector3.Distance(playerTransform.position, mouseWorldPos);
        
        if (distance < minRadius)
        {
            // 如果鼠标距离小于最小半径，则铲子的位置不变
            Vector3 direction = (mouseWorldPos - playerTransform.position).normalized;
            transform.position = playerTransform.position + direction * minRadius;
        }
        else
        {
            // 将铲子头部的位置直接设置为鼠标位置
            transform.position = mouseWorldPos;
        }
    }
}
