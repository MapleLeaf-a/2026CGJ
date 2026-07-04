using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenuUI; // 暂停菜单的UI对象

    public Player player;

    void Start()
    {
        pauseMenuUI.SetActive(false); // 游戏开始时隐藏暂停菜单
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (!Player.isGamePaused)
        {
            Time.timeScale = 0; // 暂停游戏
            pauseMenuUI.SetActive(true); // 显示暂停菜单
            Player.isGamePaused = true;
        }
        else
        {
            if (player.currentState != PlayerState.Struggle)
            {
                Time.timeScale = 1; //不处于挣扎状态才正常重置时间    
            }
            pauseMenuUI.SetActive(false); // 隐藏暂停菜单
            Player.isGamePaused = false;
        }
    }
}
