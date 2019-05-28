using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;
using UnityEngine.UI;
using SonicBloom.Koreo.Players;
using UnityEngine.SceneManagement;

/// <summary>
/// IRIS
/// </summary>
namespace IRIS
{
    /// <summary>
    /// 音乐节奏事件控制
    /// </summary>
    public class RhythmGameController : MonoBehaviour
    {
        #region 可调节属性
        [EventID]
        [Header("音符事件")]
        public List<string> eventID = new List<string> { };
        [Header("音符速度")]
        [Range(1f, 5f)]
        public float noteSpeed;
        [Header("音符命中区间")]
        [Range(300f, 800f)]
        public float hitWindowRangeInMS;

        [Header("音符")]
        public NoteObject noteObject;
        [Header("按下特效")]
        public GameObject downEffectGo;
        [Header("击中音符特效")]
        public GameObject hitNoteEffectGo;
        //[Header("击中长音符特效")]
        //public GameObject hitLongNoteEffectGo;
        [Header("音乐播放组件")]
        public AudioSource audioCom;

        [Header("音符提前时间")]
        public float leadInTime;
        [Header("音频播放之前的剩余时间量")]
        public float leadInTimeLeft;
        [Header("音乐开始之前的倒计时器")]
        public float timeLeftToPlay;
        [Header("音轨")]
        public List<LaneController> noteLanes = new List<LaneController>();

        #region 选歌以及切换
        [Header("切歌")]
        public Koreography kgy;
        Koreography playingKoreo;
        SimpleMusicPlayer simpleMusicPlayer;
        public Transform simpleMusciPlayerTrans;
        #endregion

        #endregion

        #region 不可调节属性

        #region 歌曲状态
        [Header("状态")]
        public bool isPauseState;
        private bool gameStart;
        private bool gamePause;
        public bool GameOver { get;private set; }
        #endregion

        #region 最大Combo
        [Header("Combo")]
        public int comboNum;
        public Text comboText;
        public Animator comboTextAnim;
        #endregion

        #region 分数
        public int Score { get; private set; }
        public Text scoreText;
        #endregion

        #region 命中等级
        [Header("命中等级")]
        public Sprite[] hitLevelSprites;
        public Image hitLevelImage;
        public Animator hitLevelImageAnim;
        float hideHitLevelImageTimeVal;
        #endregion

        #region 命中窗口
        int hitWindowRangeInSamples;
        public float WindowsSizeInUnits
        {
            get
            {
                return noteSpeed * (hitWindowRangeInMS * 0.001f) * 5.0f + 5.0f;
            }
        }
        #endregion

        #region 对象池
        //音符对象池
        public Stack<NoteObject> noteObjectPool = new Stack<NoteObject>();
        //按下特效对象池
        public Stack<GameObject> downEffectObjectPool = new Stack<GameObject>();
        //击中音符特效对象池
        public Stack<GameObject> hitNoteEffectObjectPool = new Stack<GameObject>();
        //击中长音符特效对象池
        public Stack<GameObject> hitLongNoteEffectObjectPool = new Stack<GameObject>();
        #endregion

        public GameObject _Camera;

        /// <summary>
        /// 当前采样时间
        /// </summary>
        public int DelayedSampleTime
        {
            get
            {
                return playingKoreo.GetLatestSampleTime() - (int)(SampleRate * leadInTimeLeft);
            }
        }

        /// <summary>
        /// 在音乐样本中的命中窗口（只读）
        /// </summary>
        public int HitWindowSampleWidth { get; }

        /// <summary>
        /// 采样率
        /// </summary>
        public int SampleRate
        {
            get
            {
                return playingKoreo.SampleRate;
            }
        }

        #endregion

        void Start()
        {
            InitializeLeadIn();
            //切歌选歌
            simpleMusicPlayer = simpleMusciPlayerTrans.GetComponent<SimpleMusicPlayer>();
            simpleMusicPlayer.LoadSong(kgy, 0, false);
            foreach (LaneController nl in noteLanes)
            {
                nl.Initialize(this);
            }

            //获取到Koreograhpy对象
            playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0);
            foreach (string _eventID in eventID)
            {
                //获取事件轨迹
                KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(_eventID);
                //获取事件
                List<KoreographyEvent> rawEvents = rhythmTrack.GetAllEvents();

                for (int i = 0; i < rawEvents.Count; i++)
                {
                    KoreographyEvent evt = rawEvents[i];
                    int noteID = evt.GetIntValue();

                    //遍历所有音轨
                    for (int j = 0; j < noteLanes.Count; j++)
                    {
                        LaneController lane = noteLanes[j];
                        if (noteID > 4)
                        {
                            noteID = noteID - 4;
                            if (noteID > 4)
                            {
                                noteID = noteID - 4;
                            }
                        }
                        if (lane.DoesMatch(noteID))
                        {
                            lane.AddEventToLane(evt);
                            break;
                        }
                    }
                }
            }

            //SampleRate采样率，在音频资源里有。
            //命中窗口宽度，采样率*0.001*命中时长
            hitWindowRangeInSamples = (int)(SampleRate * hitWindowRangeInMS * 0.001f);
        }

        void Update()
        {
            Timer();

            if (isPauseState)
            {
                return;
            }

            if (hitLevelImage.gameObject.activeSelf)
            {
                if (hideHitLevelImageTimeVal > 0)
                {
                    hideHitLevelImageTimeVal -= Time.deltaTime;
                }
                else
                {
                    HideComboNumText();
                    HideHitLevelImage();
                }
            }

            if (gameStart)
            {
                if (!simpleMusicPlayer.IsPlaying && !gamePause)
                {
                    Debug.Log("游戏结束");
                    GameOver = true;
                    //gameOverUI.SetActive(true);
                }
            }

            if (Input.GetKey(KeyCode.U))
            {
                PauseMusic();
            }
            if (Input.GetKey(KeyCode.I))
            {
                PlayMusic();
            }
        }

        /// <summary>
        /// 初始化引导时间
        /// </summary>
        private void InitializeLeadIn()
        {
            if (leadInTime > 0)
            {
                leadInTimeLeft = leadInTime;
                timeLeftToPlay = leadInTime;
            }
            else
            {
                audioCom.Play();
            }
        }

        /// <summary>
        /// 计时器
        /// </summary>
        private void Timer()
        {
            if (timeLeftToPlay > 0)
            {
                timeLeftToPlay -= Time.unscaledDeltaTime;
                //Debug.Log("timeLeftToPlay:" + timeLeftToPlay);

                if (timeLeftToPlay <= 0)
                {
                    audioCom.Play();
                    gameStart = true;
                    timeLeftToPlay = 0;
                }
            }
            //倒数引导时间
            if (leadInTimeLeft > 0)
            {
                leadInTimeLeft = Mathf.Max(leadInTimeLeft - Time.unscaledDeltaTime, 0);
            }
        }

        /// <summary>
        /// 从对象池取出
        /// </summary>
        /// <returns></returns>
        public NoteObject GetFreshNoteObject()
        {
            NoteObject retObj;

            if (noteObjectPool.Count > 0)
            {
                retObj = noteObjectPool.Pop();
            }
            else
            {
                //资源源
                retObj = Instantiate(noteObject);
            }

            retObj.transform.position = _Camera.transform.position;
            retObj.gameObject.SetActive(true);
            retObj.enabled = true;

            return retObj;
        }

        /// <summary>
        /// 将音符对象放回对象池
        /// </summary>
        /// <param name="obj"></param>
        public void ReturnNoteObjectToPool(NoteObject obj)
        {
            if (obj != null)
            {
                obj.enabled = false;
                obj.gameObject.SetActive(false);
                noteObjectPool.Push(obj);
            }
        }

        /// <summary>
        /// 从对象池取出特效
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="effectObject"></param>
        /// <returns></returns>
        public GameObject GetFreshEffectObject(Stack<GameObject> stack, GameObject effectObject)
        {
            GameObject effectGo;

            if (stack.Count > 0)
            {
                Debug.Log("Pop");
                effectGo = stack.Pop();
            }
            else
            {
                Debug.Log("生成");
                effectGo = Instantiate(effectObject);
            }

            effectGo.SetActive(true);

            return effectGo;
        }

        /// <summary>
        /// 将特效放回对象池
        /// </summary>
        /// <param name="effectGo"></param>
        /// <param name="stack"></param>
        public void ReturnEffectGoToPool(GameObject effectGo, Stack<GameObject> stack)
        {
            if (effectGo != null)
            {
                effectGo.gameObject.SetActive(false);
                stack.Push(effectGo);
                Debug.Log("Stack: " + stack.Count);
            }
        }

        /// <summary>
        /// 显示命中等级对应的图片
        /// </summary>
        /// <param name="hitLevel">0：Miss，1：Great，2：Perfect</param>
        public void ChangeHitLevelSprite(int hitLevel)
        {
            hideHitLevelImageTimeVal = 1;
            hitLevelImage.sprite = hitLevelSprites[hitLevel];
            hitLevelImage.SetNativeSize();
            hitLevelImage.gameObject.SetActive(true);
            //hitLevelImageAnim.SetBool("IsNoteHittable", true);
            if (comboNum >= 5)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = comboNum.ToString();
                //comboTextAnim.SetBool("IsNoteHittable", true);

            }
            //hitLevelImageAnim.Play("UIAnimation");
        }

        /// <summary>
        /// 显示命中等级
        /// </summary>
        private void HideHitLevelImage()
        {
            hitLevelImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 显示Combo
        /// </summary>
        public void HideComboNumText()
        {
            comboText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 显示分数
        /// </summary>
        /// <param name="addNum"></param>
        public void UpdateScoreText(int addNum)
        {
            Score += addNum;
            scoreText.text = Score.ToString();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void PlayMusic()
        {
            if (!gameStart)
            {
                return;
            }
            simpleMusicPlayer.Play();
            gamePause = false;
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseMusic()
        {
            if (!gameStart)
            {
                return;
            }
            simpleMusicPlayer.Pause();
            gamePause = true;
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        public void Replay()
        {
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// 返回菜单
        /// </summary>
        public void ReturnToMain()
        {
            SceneManager.LoadScene(0);
        }
    }
}