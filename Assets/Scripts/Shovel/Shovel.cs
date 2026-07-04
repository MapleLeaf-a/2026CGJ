using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shovel : MonoBehaviour
{
    public float attackCooldown = 0.5f; // 攻击冷却时间
    private float lastAttackTime = 0f;


    public Image cooldownMaskImage;

    public Shake_Camera mainCamera; // 摄像机抖动脚本引用

    void Start()
    {
        // 初始化：设为一个很久远的时间，确保开始时冷却就已经结束
        lastAttackTime = -attackCooldown;

        // 初始化：游戏开始时，冷却是转完的（没有灰色遮罩）
        if (cooldownMaskImage != null)
        {
            cooldownMaskImage.fillAmount = 0f; // 0 表示完全透明（无冷却）
        }
    }

    void Update()
    {
        if (cooldownMaskImage != null)
        {
            // 计算已经过去的时间
            float timeSinceLastAttack = Time.time - lastAttackTime;
            
            // 计算冷却进度比例 (0 到 1)
            // 如果没有攻击过（timeSinceLastAttack很大），Mathf.Clamp01 会保证结果限制在 0 到 1
            float fillProgress = 1f - Mathf.Clamp01(timeSinceLastAttack / attackCooldown);

            // 应用到 UI 上
            cooldownMaskImage.fillAmount = fillProgress;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Hit();
                lastAttackTime = Time.time; // 更新攻击时间
            }
        }
    }

    private void Hit()
    {        
        Debug.Log("挥动铲子！");


        mainCamera.randomShake = true; // 触发摄像机抖动


        // 1. 这里可以使用 物理扫描（Overlap） 方式，而不依赖 OnTrigger 的持续碰撞
        // 获取铲子碰撞体的信息
        Collider2D shovelCollider = GetComponent<Collider2D>();

        // 2. 用铲子的碰撞体去探测世界（探测范围就是铲子当前挥舞到的位置）
        // 使用 OverlapCollider 可以一瞬间检测铲子覆盖的范围内有没有鳄鱼
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Enemy")); // 敌人层
        filter.useTriggers = true; // 允许检测触发器

        List<Collider2D> results = new List<Collider2D>();
        int hitCount = shovelCollider.OverlapCollider(filter, results);
        // 3. 如果检测到了物体
        if (hitCount > 0)
        {
            foreach (var hit in results)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Crocodile crocodile = hit.GetComponent<Crocodile>();
                    if (crocodile != null)
                    {
                        crocodile.GetDamaged();
                        Debug.Log("挥铲！打中鳄鱼！");
                    }        
                }
            }
        }
    }
}
