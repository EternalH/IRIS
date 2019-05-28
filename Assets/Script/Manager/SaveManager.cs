using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IRIS·Manager
/// </summary>
namespace IRIS.Manager
{
    /// <summary>
    /// 游戏存档管理
    /// </summary>
    [System.Serializable]
    public class SaveManager : MonoBehaviour
    {
        public int _Score;
        public int _MaxScore { get; private set; }

        public List<int> _Scores = new List<int>();

    }
}

