using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenuUI; // 暂停菜单的UI对象

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
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0; // 暂停游戏
            pauseMenuUI.SetActive(true); // 显示暂停菜单
        }
        else
        {
            Time.timeScale = 1; // 恢复游戏
            pauseMenuUI.SetActive(false); // 隐藏暂停菜单
        }
    }
}
