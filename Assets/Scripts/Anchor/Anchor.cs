using UnityEngine;

public class Anchor : MonoBehaviour
{
    private GameManager gameManager;
    private int remainingCrocodiles = 0;

    public void SetupAnchor(GameManager gm, int initialCount)
    {
        gameManager = gm;
        remainingCrocodiles = initialCount;
    }

    // 这条锚点附近的鳄鱼死了一条
    public void OnCrocodileKilled()
    {
        remainingCrocodiles--;
        
        // 当锚点下的鳄鱼数量归零时，通知 GameManager
        if (remainingCrocodiles <= 0)
        {
            // 可以在这里播放一个锚点销毁的特效
            gameManager.OnAnchorCleared();
            Destroy(gameObject); // 锚点任务完成，消失
        }
    }
}