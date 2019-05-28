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
    /// 异步加载
    /// </summary>
    public class Loading : MonoBehaviour
    {
        //public List<GameObject> slogans = new List<GameObject> { };

        //异步对象
        AsyncOperation async;
        //private GameObject player;

        //当前加载的进度
        private int load_index = 0;
        //读取场景的进度，它的取值范围在0 - 1 之间。
        private int progress = 0;
        // 进度条显示值 
        private int displayProgress = 0;
        // 进度条要达到的值
        private int toProgress = 0;

        void Start()
        {
            //SlogansSelect();
            //在这里开启一个异步任务，
            //进入loadScene方法。
            StartCoroutine(LoadScene());

            //player = GameObject.FindGameObjectWithTag("Player");
            //if (player == null)
            //{
            //    return;
            //}
            //player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

        void Update()
        {
            //CharacterPositionSetting();
        }

        //注意这里返回值一定是 IEnumerator
        IEnumerator LoadScene()
        {
            //yield return new WaitForEndOfFrame();
            //异步读取场景。
            //Globe.loadName 就是A场景中需要读取的C场景名称。
            //async = Application.LoadLevelAsync(LoadName.GetInstance().loadName);
            async = SceneManager.LoadSceneAsync(LoadName.GetInstance().loadName);

            async.allowSceneActivation = false;

            while (async.progress < 0.9f)
            {
                toProgress = (int)(async.progress * 100);
                while (displayProgress < toProgress)
                {
                    ++displayProgress;
                    //LoadingAnimator(displayProgress);
                }
                yield return new WaitForEndOfFrame();// 每次循环结束执行完当前帧才继续下一个循环  
            }

            toProgress = 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                //LoadingAnimator(displayProgress);
                yield return new WaitForEndOfFrame();
            }
            //CharacterPause();
            yield return new WaitForEndOfFrame();
            async.allowSceneActivation = true;
            SystemController.StartSystem();
            Time.timeScale = 1;
            ////读取完毕后返回， 系统会自动进入C场景
            //yield return async;
        }

        /// <summary>
        /// 进度条动画
        /// </summary>
        /// <param name="value"></param>
        //public void LoadingAnimator(int value)
        //{
        //    transform.Translate(new Vector2(value * 0.005f, 0));
        //}

        /// <summary>
        /// 暂停
        /// </summary>
        //public void CharacterPause()
        //{
        //    if (player != null)
        //    {
        //        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        //        Time.timeScale = 1;
        //    }
        //}

        /// <summary>
        /// 随机标语
        /// </summary>
        //public void SlogansSelect()
        //{
        //    int i = Random.Range(0, 7);
        //    slogans[i].SetActive(true);
        //}
    }
}

