using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TryAgain : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TryAgainButtonClicked);
    }

    private void TryAgainButtonClicked()
    {
        SceneManager.LoadScene("Main"); // 加载游戏场景
    }
}
