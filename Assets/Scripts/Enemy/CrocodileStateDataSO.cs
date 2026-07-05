using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 这个特性让它在 Unity 的 Create 菜单中出现
[CreateAssetMenu(fileName = "CrocodileStateData", menuName = "Game/Crocodile State Data")]
public class CrocodileStateDataSO : ScriptableObject
{
    // 存储鳄鱼的状态图片列表
    public List<Sprite> stateImages;
}