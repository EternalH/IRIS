using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// IRIS·Load
/// </summary>
namespace IRIS.Load
{
    /// <summary>
    /// 场景选择
    /// </summary>
    public class SceneChange : MonoBehaviour
    {
        /// <summary>
        /// 切换至该场景
        /// </summary>
        public void EnterScene()
        {
            SceneManager.LoadScene("Loading");
            //Application.LoadLevel("Loading");
            LoadName.GetInstance().loadName = gameObject.name;
        }
    }

}
