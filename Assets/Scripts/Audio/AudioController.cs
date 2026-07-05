using UnityEngine;

public class AudioController : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play(); // 游戏开始时播放
    }

    void Update()
    {
        // 只在状态改变时操作，不在每帧反复调用
        if (Player.isGamePaused)
        {
            // 如果当前正在播放，就暂停
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
        else
        {
            // 如果当前是暂停状态，就继续播放
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause(); // 从暂停位置继续
            }
        }
    }
}