using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IRIS·Load
/// </summary>
namespace IRIS.Load
{
    /// <summary>
    /// 加载场景名
    /// </summary>
    public class LoadName
    {
        public string loadName;
        private static LoadName instance;
        public static LoadName GetInstance()
        {
            if (instance == null)
            {
                instance = new LoadName();
            }
            return instance;
        }
        void Awake()
        {
            instance = this;
        }
    }
}

