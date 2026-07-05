using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PlayerState
{
    Normal,      // 正常移动
    Struggle,    // 挣扎中
    Escape,     //挣脱后的无敌时间
}

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
    public Vector3 targetPosition;        // 玩家实际应该前往的目标点（鼠标锁定方向的位置）


    public PlayerState currentState = PlayerState.Normal;

    [Header("挣扎UI")]
    public GameObject strugglePanel;     // 挣扎界面面板（包含进度条等UI）
    public Slider struggleSlider;        // 挣扎进度条（Slider组件）

    [Header("挣扎参数")]
    public float progress = 0f;          // 当前进度 0~1
    public float minDecaySpeed = 0.15f;  // 进度高时的衰减速度（接近成功时较慢）
    public float maxDecaySpeed = 0.6f;   // 进度低时的衰减速度（接近失败时加快）
    public float decayExponent = 1.5f;   // 衰减曲线指数，>1 则前期平缓后期陡峭
    public float pressStrength = 0.15f;  // 每按一次空格增加的进度
    public float successThreshold = 0.8f; // 达到此值视为成功挣脱

    [Header("挣脱物理")]
    // public float escapeForce = 10f;      // 挣脱时玩家弹开的力度
    public float crocPushForce = 8f;     // 挣脱时鳄鱼被弹开的力度
    public float escapeCooldown = 1.5f;  // 挣脱后无敌时间

    private bool isInevincible = false; // 是否处于无敌状态（挣脱后的一段时间）

    // 当前抓住玩家的鳄鱼引用
    private Crocodile currentCrocodile;

    public static bool isGamePaused = false; // 游戏是否处于暂停状态

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化：游戏开始时，挣扎UI是隐藏的
        if (strugglePanel != null)
        {
            strugglePanel.SetActive(false);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case PlayerState.Normal:
                UpdateNormalMovement();
                break;

            case PlayerState.Struggle:
                UpdateStruggle();
                break;
            case PlayerState.Escape:
                //无敌状态下，玩家可以自由移动，但不处理挣扎逻辑

                UpdateNormalMovement();
                break;
        }

        

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

    void UpdateNormalMovement()
    {
        if (isGamePaused) return; // 游戏暂停时不处理玩家输入

        // --- 移动逻辑 ---
        if (Input.GetKey(KeyCode.W)) transform.Translate(moveSpeed * Time.deltaTime * Vector3.up);
        if (Input.GetKey(KeyCode.S)) transform.Translate(moveSpeed * Time.deltaTime * Vector3.down);
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.left);
            spriteRenderer.flipX = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.right);
            spriteRenderer.flipX = false;
        }

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
    }

    void UpdateStruggle()
    {
        if (isGamePaused) return; // 游戏暂停时不处理挣扎逻辑

        // 1. 动态衰减速度：越接近失败掉得越快
        //    progress=1 → decay≈minDecaySpeed（慢）
        //    progress=0 → decay≈maxDecaySpeed（快）
        float t = Mathf.Pow(progress, decayExponent);
        float currentDecay = Mathf.Lerp(maxDecaySpeed, minDecaySpeed, t);
        progress -= currentDecay * Time.unscaledDeltaTime;

        // 2. 检测空格按压（使用新 Input System，确保在 timeScale≠1 时也能正常检测）
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            progress += pressStrength;
            // TODO: 可在此播放按压特效或音效
        }

        // 3. 限制范围并判定结果
        progress = Mathf.Clamp(progress, 0f, 1f);

        if (progress >= successThreshold)
        {
            EscapeSuccess();
        }
        else if (progress <= 0f)
        {
            EscapeFail();
        }

        // 4. 更新UI进度条
        if (struggleSlider != null)
        {
            struggleSlider.value = progress;
        }
    }

    void EnterStruggle()
    {
        currentState = PlayerState.Struggle;
        progress = 0.5f;                 // 进度条初值
        Time.timeScale = 0.05f;          // 极慢速，避免 timeScale=0 导致输入/物理异常

        // 显示挣扎UI面板
        if (strugglePanel != null)
        {
            strugglePanel.SetActive(true);
        }

        // 初始化进度条
        if (struggleSlider != null)
        {
            struggleSlider.value = progress;
        }

        Debug.Log("玩家被鳄鱼抓住，进入挣扎！连续按空格键摆脱！");
    }

    void EscapeSuccess()
    {
        currentState = PlayerState.Normal;
        Time.timeScale = 1f;

        // 隐藏挣扎UI
        if (strugglePanel != null)
        {
            strugglePanel.SetActive(false);
        }

        // 物理分离：直接设置瞬时速度，使玩家和鳄鱼向相反方向弹开
        if (currentCrocodile != null)
        {
            Vector3 direction = (transform.position - currentCrocodile.transform.position).normalized;

            // Rigidbody2D playerRb = GetComponent<Rigidbody2D>();
            // if (playerRb != null)
            // {
            //     playerRb.AddForce(direction * escapeForce, ForceMode2D.Impulse);
            // }

            Rigidbody2D crocRb = currentCrocodile.GetComponent<Rigidbody2D>();
            if (crocRb != null)
            {
                crocRb.AddForce(-direction * crocPushForce, ForceMode2D.Impulse);
            }

            // 让鳄鱼进入硬直/受击状态
            currentCrocodile.GetDamaged();

            currentCrocodile = null;
        }

        Debug.Log("挣脱成功！");

        isInevincible = true; // 进入无敌状态
        // 开始无敌冷却计时
        StartCoroutine(EscapeCooldown(escapeCooldown));
    }

    IEnumerator EscapeCooldown(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        currentState = PlayerState.Normal;
        isInevincible = false; // 无敌状态结束
    }

    void EscapeFail()
    {
        currentState = PlayerState.Normal;
        Time.timeScale = 1f;

        // 隐藏挣扎UI
        if (strugglePanel != null)
        {
            strugglePanel.SetActive(false);
        }

        Debug.Log("挣扎失败！玩家被鳄鱼吃掉了！");
        
        SceneManager.LoadScene("GameOver"); // 加载游戏结束场景
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isInevincible && collision.gameObject.CompareTag("Enemy") && currentState == PlayerState.Normal)
        {
            // 捕获碰撞到的鳄鱼引用
            currentCrocodile = collision.gameObject.GetComponent<Crocodile>();
            if (currentCrocodile != null)
            {
                EnterStruggle();
            }
        }
    }
}