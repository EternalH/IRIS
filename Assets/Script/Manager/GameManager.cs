using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LitJson;
using System.IO;
using IRIS.Load;

/// <summary>
/// IRIS·Manager
/// </summary>
namespace IRIS.Manager
{
    /// <summary>
    /// 游戏主进程管理
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private MusicManager music = new MusicManager();

        public GameObject _GameController;
        private bool isStartGame = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            _GameController = GameObject.FindGameObjectWithTag("GameController");
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            isStartGame = true;
            SceneManager.LoadScene("Loading");
            LoadName.GetInstance().loadName = music._Name;
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public void ExitGame()
        {
            isStartGame = false;
            SceneManager.LoadScene("Loading");
            LoadName.GetInstance().loadName = "Menu";
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            _GameController.GetComponent<RhythmGameController>().PauseMusic();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void RePlay()
        {
            _GameController.GetComponent<RhythmGameController>().Replay();
        }

        /// <summary>
        /// 创建存档
        /// </summary>
        /// <returns></returns>
        private SaveManager CreateSave()
        {
            SaveManager save = new SaveManager();
            save._Scores.Add(_GameController.GetComponent<RhythmGameController>().Score);

            return save;
        }

        /// <summary>
        /// 读取存档
        /// </summary>
        /// <param name="save">存储信息</param>
        private void SetGame(SaveManager save)
        {

        }

        /// <summary>
        /// 编译Json
        /// </summary>
        public void SaveByJson()
        {
            SaveManager save = CreateSave();

            string filePath = Application.dataPath + "/StreamingFile" + "/byJson.json";
            string saveJsonStr = JsonMapper.ToJson(save);

            StreamWriter sw = new StreamWriter(filePath);
            sw.Write(saveJsonStr);
            sw.Close();

            Debug.Log("存储成功");
        }

        /// <summary>
        /// 反编译Json
        /// </summary>
        public void LoadByJson()
        {
            string filePath = Application.dataPath + "/StreamingFile" + "/byJson.json";
            if (File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath);
                string jsonStr = sr.ReadToEnd();
                sr.Close();

                SaveManager save = JsonMapper.ToObject<SaveManager>(jsonStr);
                SetGame(save);

                Debug.Log("读取成功");
            }
            else
            {
                Debug.Log("读取失败");
            }
        }
    }
}

