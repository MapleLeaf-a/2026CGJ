using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private float moveSpeed = 3f;

    // 定义地图边界
    private float xMin = -8.5f;
    private float xMax = 8.5f;
    private float yMin = -4.5f;
    private float yMax = 4.5f;

    // 这两个变量用于控制鼠标跟随半径
    public float mouseFollowRadius = 2.5f; // 鼠标可以自由活动的半径
    private Vector3 targetPosition;        // 玩家实际应该前往的目标点（鼠标锁定方向的位置）

    void Update()
    {
        // --- 移动逻辑 ---
        if (Input.GetKey(KeyCode.W)) transform.Translate(moveSpeed * Time.deltaTime * Vector3.up);
        if (Input.GetKey(KeyCode.S)) transform.Translate(moveSpeed * Time.deltaTime * Vector3.down);
        if (Input.GetKey(KeyCode.A)) transform.Translate(moveSpeed * Time.deltaTime * Vector3.left);
        if (Input.GetKey(KeyCode.D)) transform.Translate(moveSpeed * Time.deltaTime * Vector3.right);

        // --- 边界限制逻辑 ---
        // 获取当前玩家位置
        Vector3 currentPos = transform.position;

        // 使用 Mathf.Clamp 将坐标锁定在最小值与最大值之间
        currentPos.x = Mathf.Clamp(currentPos.x, xMin, xMax);
        currentPos.y = Mathf.Clamp(currentPos.y, yMin, yMax);

        // 把修正后的坐标重新赋值给玩家
        transform.position = currentPos;

        // 鼠标锁定和跟随逻辑
        HandleMouseAim();

         // 按 F1 键暂停游戏（调试用）
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        #endif
    }

    void HandleMouseAim()
    {
        // 1. 获取鼠标在游戏世界中的位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; 

        // 2. 计算鼠标相对于【当前玩家位置】的偏移向量
        Vector3 offset = mouseWorldPos - transform.position;

        // 3. 限制向量的长度（核心步骤）
        // 如果偏移量的长度超过了半径，就把长度截断为半径的长度
        if (offset.magnitude > mouseFollowRadius)
        {
            offset = offset.normalized * mouseFollowRadius;
        }

        // 4. 计算最终的实际目标点
        // 不管玩家怎么走，目标点永远是【玩家当前位置】+【限制后的偏移量】
        // 这样，当你按住键盘移动时，这个“光环”会带着鼠标位置平移！
        targetPosition = transform.position + offset;

        Mouse.current.WarpCursorPosition(Camera.main.WorldToScreenPoint(targetPosition));
    }
}