using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IRIS.ECS;

/// <summary>
/// IRIS·Load
/// </summary>
namespace IRIS.Load
{
    /// <summary>
    /// 启动加载
    /// </summary>
    public class Starting : MonoBehaviour
    {
        private float time;

        void Start()
        {
            time = 5.0f;
        }

        void Update()
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                SceneManager.LoadScene("Addiction");
                SystemController.StartSystem();
            }

            if (Input.touchCount >= 1 || Input.GetMouseButton(0)) 
            {
                SceneManager.LoadScene("WingsOfPiano");
                SystemController.StartSystem();
            }
        }
    }
}

