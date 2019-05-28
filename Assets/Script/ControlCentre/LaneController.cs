using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using SonicBloom.Koreo;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// IRIS
/// </summary>
namespace IRIS
{
    /// <summary>
    /// 音轨控制
    /// </summary>
    public class LaneController : MonoBehaviour
    {
        #region 可调节属性
        [Header("音轨使用的按键")]
        public KeyCode keyboardButton;

        [Header("音轨使用的虚拟按键")]
        public int keyCode;

        public AndroidInput androidInput;

        [Header("音轨对应事件编号")]
        public int laneID;
        #endregion

        #region 不可调节属性
        //对目标位置的键盘按下的视觉效果
        public Transform targetVisuals;

        //上下边界
        public Transform targetTopTrans;
        public Transform targetBottomTrans;

        //检测此音轨中的生成的下一个事件的索引
        int pendingEventIdx = 0;
        //点击目标是否为长音符
        public static bool HasLongNote { get; private set; }
        //是否正在处于长音符中
        public static bool InLongNote { get; private set; } = false;

        //长音符计时
        public float timeVal = 0;

        private RhythmGameController gameController;

        public GameObject downVisual;

        public GameObject longNoteHitEffectGo;

        private float3 mousePos;
        private float3 touchPos;
        private float3 touchPos_0;
        private float3 touchPos_1;

        private int _keyCode;

        public static bool HitOrNot { get; set; }

        public static int NoteNum { get; private set; } = 0;

        public List<GameObject> targets = new List<GameObject>();

        /// <summary>
        /// 音符移动的目标位置
        /// </summary>
        public Vector3 TargetPosition
        {
            get
            {
                return transform.position;
            }
        }

        /// <summary>
        /// 此音轨中的所有事件列表
        /// </summary>
        List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();

        /// <summary>
        /// 此音轨中当前活动的所有音符对象
        /// </summary>
        Queue<NoteObject> trackedNotes = new Queue<NoteObject>();

        #endregion

        #region 虚拟按键
        [DllImport("user32.dll", EntryPoint = "keybd_event")]

        public static extern void Keybd_event(
            byte bvk,//虚拟键值 ASCII码
            byte bScan,//0
            int dwFlags,//0为按下，1按住，2释放
            int dwExtraInfo//0
            );
        #endregion

        void Start()
        {
            Input.multiTouchEnabled = true;
        }

        void Update()
        {
            if (gameController.isPauseState)
            {
                return;
            }

            //清除无效音符
            ClearInvalidNotes();

            //检测新音符的生成
            CheckSpawnNext();

            //检测玩家的输入
            //触屏输入
            //CheckTouchInput();
            //鼠标输入
            //CheckMouseInput();
            //键盘输入
            CheckKeyboardInput();

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="controller"></param>
        public void Initialize(RhythmGameController controller)
        {
            gameController = controller;
        }

        /// <summary>
        /// 检测事件是否匹配当前编号的音轨
        /// </summary>
        /// <param name="noteID"></param>
        /// <returns></returns>
        public bool DoesMatch(int noteID)
        {
            return noteID == laneID;
        }

        /// <summary>
        /// 如果匹配，则把当前事件添加进音轨所持有的事件列表
        /// </summary>
        /// <param name="evt"></param>
        public void AddEventToLane(KoreographyEvent evt)
        {
            laneEvents.Add(evt);
        }

        /// <summary>
        /// 音符在音谱上产生的位置偏移量
        /// </summary>
        /// <returns></returns>
        public int GetSpawnSampleOffset()
        {
            //出生位置与目标点的位置
            float spawnDistToTarget = targetTopTrans.position.z - transform.position.z;

            //到达目标点的时间
            float spawnPosToTargetTime = spawnDistToTarget / (gameController.noteSpeed * 5.0f + 5.0f);

            return (int)spawnPosToTargetTime * gameController.SampleRate;
        }

        /// <summary>
        /// 清除无效音符
        /// </summary>
        public void ClearInvalidNotes()
        {
            while (trackedNotes.Count > 0 && trackedNotes.Peek().IsNoteMissed())
            {
                if (trackedNotes.Peek().isLongNoteEnd)
                {
                    HasLongNote = false;
                    timeVal = 0;
                    downVisual.SetActive(false);
                    longNoteHitEffectGo.SetActive(false);
                }
                gameController.comboNum = 0;
                gameController.HideComboNumText();
                gameController.ChangeHitLevelSprite(0);
                trackedNotes.Dequeue();
            }
        }

        /// <summary>
        /// 检测是否生成下一个新音符
        /// </summary>
        public void CheckSpawnNext()
        {
            int samplesToTarget = GetSpawnSampleOffset();

            int currentTime = gameController.DelayedSampleTime;

            while (pendingEventIdx < laneEvents.Count
                   && laneEvents[pendingEventIdx].StartSample < currentTime + samplesToTarget)
            {
                KoreographyEvent evt = laneEvents[pendingEventIdx];
                NoteNum = evt.GetIntValue();
                //Debug.Log("noteNum:" + noteNum);
                NoteObject newObj = gameController.GetFreshNoteObject();
                bool isLongNoteStart = false;
                bool isLongNoteEnd = false;
                if (NoteNum > 4)
                {
                    isLongNoteStart = true;
                    InLongNote = true;
                    NoteNum = NoteNum - 4;
                    CreateLongNote();
                    
                    if (NoteNum > 4)
                    {
                        isLongNoteEnd = true;
                        isLongNoteStart = false;
                        NoteNum = NoteNum - 4;
                        InLongNote = false;
                    }
                }
                //Debug.Log("NoteNum: " + NoteNum);
                newObj.Initialize(evt, NoteNum, this, gameController, isLongNoteStart, isLongNoteEnd);
                trackedNotes.Enqueue(newObj);
                //Debug.Log(trackedNotes.Count);
                pendingEventIdx++;
            }
        }

        /// <summary>
        /// 生成长音符
        /// </summary>
        void CreateLongNote()
        {
            float time = 0;
            time += Time.deltaTime;
            if (time >= 0.5f)
            {
                NoteObject obj = gameController.GetFreshNoteObject();
                obj.InitializeLongNote(NoteNum,this,gameController);
                time = 0;
            }

        }

        /// <summary>
        /// 检测UI按键按下
        /// </summary>
        public void CheckUIButtonDown()
        {
            CheckNoteHit();
            downVisual.SetActive(true);
            HitOrNot = true;
        }

        /// <summary>
        /// 检测UI按键持续按下
        /// </summary>
        public void CheckUIButtonstay()
        {                    
            //检测长音符
            if (HasLongNote)
            {
                InLongNote = true;
                HitOrNot = true;
                gameController.UpdateScoreText(10 * 2);
                if (timeVal >= 0.15f)
                {
                    //显示命中等级（Great Perfect）
                    if (!longNoteHitEffectGo.activeSelf)
                    {
                        gameController.ChangeHitLevelSprite(2);
                        CreateHitLongEffect();
                    }
                    timeVal = 0;
                }
                else
                {
                    timeVal += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 检测UI按键弹起
        /// </summary>
        public void CheckUIButtonUp()
        {
            downVisual.SetActive(false);
            //检测长音符
            if (HasLongNote)
            {
                InLongNote = false;
                longNoteHitEffectGo.SetActive(false);
                CheckNoteHit();
            }
        }

        /// <summary>
        /// 监测键盘输入
        /// </summary>
        void CheckKeyboardInput()
        {
            if (Input.GetKeyDown(keyboardButton))
            {
                CheckNoteHit();
                downVisual.SetActive(true);
                HitOrNot = true;
            }
            else if (Input.GetKey(keyboardButton))
            {
                //检测长音符
                if (HasLongNote)
                {
                    gameController.UpdateScoreText(10 * 2);
                    if (timeVal >= 0.15f)
                    {
                        //显示命中等级（Great Perfect）
                        if (!longNoteHitEffectGo.activeSelf)
                        {
                            gameController.ChangeHitLevelSprite(2);
                            CreateHitLongEffect();
                        }
                        timeVal = 0;
                    }
                    else
                    {
                        timeVal += Time.deltaTime;
                    }
                }
            }
            else if (Input.GetKeyUp(keyboardButton))
            {
                downVisual.SetActive(false);
                //检测长音符
                if (HasLongNote)
                {
                    longNoteHitEffectGo.SetActive(false);
                    CheckNoteHit();
                }
            }
        }

        /// <summary>
        /// 监测触屏输入
        /// </summary>
        void CheckTouchInput()
        {
            if (Input.touchCount <= 0)
            {
                return;
            }
            if (Input.touchCount == 1)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    touchPos = Camera.main.ScreenToWorldPoint(new float3(Input.touches[0].position.x, Input.touches[0].position.y, 8f));
                    float posX = touchPos.x;
                    float posY = touchPos.y;
                    //float posZ = touchPos.z;

                    //模拟按键 ACSII码
                    if (posX >= targets[0].transform.position.x - 0.35f && posX <= targets[0].transform.position.x + 0.35f &&
                        posY >= targets[0].transform.position.y - 0.35f && posY <= targets[0].transform.position.y + 0.35f)
                    {
                        _keyCode = 1;
                        //Keybd_event(81, 0, 0, 0);
                    }
                    if (posX >= targets[1].transform.position.x - 0.35f && posX <= targets[1].transform.position.x + 0.35f &&
                        posY >= targets[1].transform.position.y - 0.35f && posY <= targets[1].transform.position.y + 0.35f)
                    {
                        _keyCode = 2;
                        //Keybd_event(87, 0, 0, 0);
                    }
                    if (posX >= targets[2].transform.position.x - 0.35f && posX <= targets[2].transform.position.x + 0.35f &&
                        posY >= targets[2].transform.position.y - 0.35f && posY <= targets[2].transform.position.y + 0.35f)
                    {
                        _keyCode = 3;
                        //Keybd_event(69, 0, 0, 0);
                    }
                    if (posX >= targets[3].transform.position.x - 0.35f && posX <= targets[3].transform.position.x + 0.35f &&
                        posY >= targets[3].transform.position.y - 0.35f && posY <= targets[3].transform.position.y + 0.35f)
                    {
                        _keyCode = 4;
                        //Keybd_event(82, 0, 0, 0);
                    }

                    if (keyCode == _keyCode)
                    {
                        CheckNoteHit();
                        downVisual.SetActive(true);
                        HitOrNot = true;
                    }
                }
                else if (Input.touches[0].phase == TouchPhase.Stationary ||
                    Input.touches[0].phase == TouchPhase.Moved)
                {
                    touchPos = Camera.main.ScreenToWorldPoint(new float3(Input.touches[0].position.x, Input.touches[0].position.y, 8f));
                    float posX = touchPos.x;
                    float posY = touchPos.y;
                    //float posZ = touchPos.z;

                    //模拟按键 ACSII码
                    if (posX >= targets[0].transform.position.x - 0.35f && posX <= targets[0].transform.position.x + 0.35f &&
                        posY >= targets[0].transform.position.y - 0.35f && posY <= targets[0].transform.position.y + 0.35f)
                    {
                        _keyCode = 1;
                        //Keybd_event(81, 0, 1, 0);
                    }
                    if (posX >= targets[1].transform.position.x - 0.35f && posX <= targets[1].transform.position.x + 0.35f &&
                        posY >= targets[1].transform.position.y - 0.35f && posY <= targets[1].transform.position.y + 0.35f)
                    {
                        _keyCode = 2;
                        //Keybd_event(87, 0, 1, 0);
                    }
                    if (posX >= targets[2].transform.position.x - 0.35f && posX <= targets[2].transform.position.x + 0.35f &&
                        posY >= targets[2].transform.position.y - 0.35f && posY <= targets[2].transform.position.y + 0.35f)
                    {
                        _keyCode = 3;
                        //Keybd_event(69, 0, 1, 0);
                    }
                    if (posX >= targets[3].transform.position.x - 0.35f && posX <= targets[3].transform.position.x + 0.35f &&
                        posY >= targets[3].transform.position.y - 0.35f && posY <= targets[3].transform.position.y + 0.35f)
                    {
                        _keyCode = 4;
                        //Keybd_event(82, 0, 1, 0);
                    }

                    if (keyCode == _keyCode)
                    {
                        //检测长音符
                        if (HasLongNote)
                        {
                            InLongNote = true;
                            HitOrNot = true;
                            gameController.UpdateScoreText(10 * 2);
                            if (timeVal >= 0.15f)
                            {
                                //显示命中等级（Great Perfect）
                                if (!longNoteHitEffectGo.activeSelf)
                                {
                                    gameController.ChangeHitLevelSprite(2);
                                    CreateHitLongEffect();
                                }
                                timeVal = 0;
                            }
                            else
                            {
                                timeVal += Time.deltaTime;
                            }
                        }
                    }
                }
                else if (Input.touches[0].phase == TouchPhase.Ended ||
                    Input.touches[0].phase == TouchPhase.Canceled)
                {
                    _keyCode = 0;

                    downVisual.SetActive(false);
                    //检测长音符
                    if (HasLongNote)
                    {
                        InLongNote = false;
                        longNoteHitEffectGo.SetActive(false);
                        CheckNoteHit();
                    }
                }
            }
            if (Input.touchCount == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Input.touches[i].phase == TouchPhase.Began)
                    {
                        touchPos = Camera.main.ScreenToWorldPoint(new float3(Input.touches[i].position.x, Input.touches[i].position.y, 8f));
                        float posX = touchPos.x;
                        float posY = touchPos.y;
                        //float posZ = touchPos.z;
                        Debug.Log("posX: " + posX);
                        Debug.Log("posY: " + posX);
                        Debug.Log("posZ: " + posX);


                        //模拟按键 ACSII码
                        if (posX >= targets[0].transform.position.x - 0.35f && posX <= targets[0].transform.position.x + 0.35f &&
                            posY >= targets[0].transform.position.y - 0.35f && posY <= targets[0].transform.position.y + 0.35f)
                        {
                            _keyCode = 1;
                            //Keybd_event(81, 0, 0, 0);
                        }
                        if (posX >= targets[1].transform.position.x - 0.35f && posX <= targets[1].transform.position.x + 0.35f &&
                            posY >= targets[1].transform.position.y - 0.35f && posY <= targets[1].transform.position.y + 0.35f)
                        {
                            _keyCode = 2;
                            //Keybd_event(87, 0, 0, 0);
                        }
                        if (posX >= targets[2].transform.position.x - 0.35f && posX <= targets[2].transform.position.x + 0.35f &&
                            posY >= targets[2].transform.position.y - 0.35f && posY <= targets[2].transform.position.y + 0.35f)
                        {
                            _keyCode = 3;
                            //Keybd_event(69, 0, 0, 0);
                        }
                        if (posX >= targets[3].transform.position.x - 0.35f && posX <= targets[3].transform.position.x + 0.35f &&
                            posY >= targets[3].transform.position.y - 0.35f && posY <= targets[3].transform.position.y + 0.35f)
                        {
                            _keyCode = 4;
                            //Keybd_event(82, 0, 0, 0);
                        }

                        if (keyCode == _keyCode)
                        {
                            CheckNoteHit();
                            downVisual.SetActive(true);
                            HitOrNot = true;
                        }
                    }
                    else if (Input.touches[i].phase == TouchPhase.Stationary ||
                        Input.touches[i].phase == TouchPhase.Moved)
                    {
                        touchPos = Camera.main.ScreenToWorldPoint(new float3(Input.touches[0].position.x, Input.touches[i].position.y, 8f));
                        float posX = touchPos.x;
                        float posY = touchPos.y;
                        //float posZ = touchPos.z;

                        //模拟按键 ACSII码
                        if (posX >= targets[0].transform.position.x - 0.35f && posX <= targets[0].transform.position.x + 0.35f &&
                            posY >= targets[0].transform.position.y - 0.35f && posY <= targets[0].transform.position.y + 0.35f)
                        {
                            _keyCode = 1;
                            //Keybd_event(81, 0, 1, 0);
                        }
                        if (posX >= targets[1].transform.position.x - 0.35f && posX <= targets[1].transform.position.x + 0.35f &&
                            posY >= targets[1].transform.position.y - 0.35f && posY <= targets[1].transform.position.y + 0.35f)
                        {
                            _keyCode = 2;
                            //Keybd_event(87, 0, 1, 0);
                        }
                        if (posX >= targets[2].transform.position.x - 0.35f && posX <= targets[2].transform.position.x + 0.35f &&
                            posY >= targets[2].transform.position.y - 0.35f && posY <= targets[2].transform.position.y + 0.35f)
                        {
                            _keyCode = 3;
                            //Keybd_event(69, 0, 1, 0);
                        }
                        if (posX >= targets[3].transform.position.x - 0.35f && posX <= targets[3].transform.position.x + 0.35f &&
                            posY >= targets[3].transform.position.y - 0.35f && posY <= targets[3].transform.position.y + 0.35f)
                        {
                            _keyCode = 4;
                            //Keybd_event(82, 0, 1, 0);
                        }

                        if (keyCode == _keyCode)
                        {
                            //检测长音符
                            if (HasLongNote)
                            {
                                InLongNote = true;
                                HitOrNot = true;
                                gameController.UpdateScoreText(10 * 2);
                                if (timeVal >= 0.15f)
                                {
                                    //显示命中等级（Great Perfect）
                                    if (!longNoteHitEffectGo.activeSelf)
                                    {
                                        gameController.ChangeHitLevelSprite(2);
                                        CreateHitLongEffect();
                                    }
                                    timeVal = 0;
                                }
                                else
                                {
                                    timeVal += Time.deltaTime;
                                }
                            }
                        }
                    }
                    else if (Input.touches[i].phase == TouchPhase.Ended ||
                        Input.touches[i].phase == TouchPhase.Canceled)
                    {
                        _keyCode = 0;

                        downVisual.SetActive(false);
                        //检测长音符
                        if (HasLongNote)
                        {
                            InLongNote = false;
                            longNoteHitEffectGo.SetActive(false);
                            CheckNoteHit();
                        }
                    }
                }
            }

            ////射线检测
            //Touch touch;
            //touch = Input.GetTouch(0);
            //Ray ray = Camera.main.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y));
            //RaycastHit hit;
            //Physics.Raycast(ray, out hit);
            //if (hit.collider.tag == "TargetQ") 
            //{
            //    _keyCode = 1;
            //}
            //else if (hit.collider.tag == "TargetW")
            //{
            //    _keyCode = 2;
            //}
            //else if (hit.collider.tag == "TargetE")
            //{
            //    _keyCode = 3;
            //}
            //else if (hit.collider.tag == "TargetR")
            //{
            //    _keyCode = 4;
            //}
            //else
            //{
            //    _keyCode = 0;
            //}

            //if (keyCode == _keyCode)
            //{
            //    CheckNoteHit();
            //    downVisual.SetActive(true);
            //    HitOrNot = true;

            //    //检测长音符
            //    if (HasLongNote)
            //    {
            //        gameController.UpdateScoreText(10 * 2);
            //        if (timeVal >= 0.15f)
            //        {
            //            //显示命中等级（Great Perfect）
            //            if (!longNoteHitEffectGo.activeSelf)
            //            {
            //                gameController.ChangeHitLevelSprite(2);
            //                CreateHitLongEffect();
            //            }
            //            timeVal = 0;
            //        }
            //        else
            //        {
            //            timeVal += Time.deltaTime;
            //        }
            //    }
            //}
            //else
            //{
            //    downVisual.SetActive(false);
            //    //检测长音符
            //    if (HasLongNote)
            //    {
            //        longNoteHitEffectGo.SetActive(false);
            //        CheckNoteHit();
            //    }
            //}

        }

        /// <summary>
        /// 检测鼠标输入
        /// </summary>
        void CheckMouseInput()
        {
            //鼠标点击输入
            if (Input.GetMouseButtonDown(0))
            {
                mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, 8f));
                float posX = mousePos.x;
                float posY = mousePos.y;
                float posZ = mousePos.z;
                //HitOrNot = true;
                Debug.Log("posX:" + posX);
                Debug.Log("posY:" + posY);
                Debug.Log("posZ:" + posZ);

                //模拟按键 ACSII码
                if (posX >= targets[0].transform.position.x - 0.35f && posX <= targets[0].transform.position.x + 0.35f &&
                    posY >= targets[0].transform.position.y - 0.35f && posY <= targets[0].transform.position.y + 0.35f)
                {
                    _keyCode = 1;
                    //Keybd_event(81, 0, 0, 0);
                }
                if (posX >= targets[1].transform.position.x - 0.35f && posX <= targets[1].transform.position.x + 0.35f &&
                    posY >= targets[1].transform.position.y - 0.35f && posY <= targets[1].transform.position.y + 0.35f)
                {
                    _keyCode = 2;
                    //Keybd_event(87, 0, 0, 0);
                }
                if (posX >= targets[2].transform.position.x - 0.35f && posX <= targets[2].transform.position.x + 0.35f &&
                    posY >= targets[2].transform.position.y - 0.35f && posY <= targets[2].transform.position.y + 0.35f)
                {
                    _keyCode = 3;
                    //Keybd_event(69, 0, 0, 0);
                }
                if (posX >= targets[3].transform.position.x - 0.35f && posX <= targets[3].transform.position.x + 0.35f &&
                    posY >= targets[3].transform.position.y - 0.35f && posY <= targets[3].transform.position.y + 0.35f)
                {
                    _keyCode = 4;
                    //Keybd_event(82, 0, 0, 0);
                }

                //Debug.Log("_keyCode" + _keyCode);

                if (keyCode == _keyCode)
                {
                    CheckNoteHit();
                    downVisual.SetActive(true);
                    HitOrNot = true;
                }
            }
            if (Input.GetMouseButton(0))
            {
                mousePos = Input.mousePosition;
                //mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, 248f/*Camera.main.transform.position.y*/));
                float posX = mousePos.x;
                float posY = mousePos.y;

                //模拟按键 ACSII码
                if (posX >= targets[0].transform.position.x - 0.35f && posX <= targets[0].transform.position.x + 0.35f &&
                    posY >= targets[0].transform.position.y - 0.35f && posY <= targets[0].transform.position.y + 0.35f)
                {
                    _keyCode = 1;
                    //Keybd_event(81, 0, 1, 0);
                }
                if (posX >= targets[1].transform.position.x - 0.35f && posX <= targets[1].transform.position.x + 0.35f &&
                    posY >= targets[1].transform.position.y - 0.35f && posY <= targets[1].transform.position.y + 0.35f)
                {
                    _keyCode = 2;
                    //Keybd_event(87, 0, 1, 0);
                }
                if (posX >= targets[2].transform.position.x - 0.35f && posX <= targets[2].transform.position.x + 0.35f &&
                    posY >= targets[2].transform.position.y - 0.35f && posY <= targets[2].transform.position.y + 0.35f)
                {
                    _keyCode = 3;
                    //Keybd_event(69, 0, 1, 0);
                }
                if (posX >= targets[3].transform.position.x - 0.35f && posX <= targets[3].transform.position.x + 0.35f &&
                    posY >= targets[3].transform.position.y - 0.35f && posY <= targets[3].transform.position.y + 0.35f)
                {
                    _keyCode = 4;
                    //Keybd_event(82, 0, 1, 0);
                }

                if (keyCode == _keyCode)
                {
                    //检测长音符
                    if (HasLongNote)
                    {
                        InLongNote = true;
                        HitOrNot = true;
                        gameController.UpdateScoreText(10 * 2);
                        if (timeVal >= 0.15f)
                        {
                            //显示命中等级（Great Perfect）
                            if (!longNoteHitEffectGo.activeSelf)
                            {
                                gameController.ChangeHitLevelSprite(2);
                                CreateHitLongEffect();
                            }
                            timeVal = 0;
                        }
                        else
                        {
                            timeVal += Time.deltaTime;
                        }
                    }
                }

            }
            else if (Input.GetMouseButtonUp(0))
            {
                _keyCode = 0;

                downVisual.SetActive(false);
                //检测长音符
                if (HasLongNote)
                {
                    InLongNote = false;
                    longNoteHitEffectGo.SetActive(false);
                    CheckNoteHit();
                }

                //Keybd_event(81, 0, 2, 0);
                //Keybd_event(87, 0, 2, 0);
                //Keybd_event(69, 0, 2, 0);
                //Keybd_event(82, 0, 2, 0);
            }
        }

        /// <summary>
        /// 生成按下特效
        /// </summary>
        void CreateDownEffect()
        {
            GameObject downEffectGo = gameController.GetFreshEffectObject(gameController.downEffectObjectPool, gameController.downEffectGo);
            downEffectGo.transform.position = targetVisuals.position;
        }

        /// <summary>
        /// 生成短音符击中特效
        /// </summary>
        void CreateHitEffect()
        {
            GameObject hitEffectGo = gameController.GetFreshEffectObject(gameController.hitNoteEffectObjectPool, gameController.hitNoteEffectGo);
            hitEffectGo.transform.position = targetVisuals.position;
        }

        /// <summary>
        /// 生成长音符击中特效
        /// </summary>
        void CreateHitLongEffect()
        {
            longNoteHitEffectGo.SetActive(true);
            longNoteHitEffectGo.transform.position = targetVisuals.position;
        }

        /// <summary>
        /// 检测是否有击中音符对象
        /// 如果是,它将执行命中并删除
        /// </summary>
        public void CheckNoteHit()
        {
            //if (!gameController.gameStart)
            //{
            //    CreateDownEffect();
            //    return;
            //}
            if (trackedNotes.Count > 0)
            {
                NoteObject noteObject = trackedNotes.Peek();
                if (noteObject.hitOffset > -20000)
                {
                    trackedNotes.Dequeue();
                    //Debug.Log(trackedNotes.Count);
                    int hitLevel = noteObject.IsNoteHittable();
                    gameController.ChangeHitLevelSprite(hitLevel);
                    if (hitLevel > 0)
                    {
                        //更新分数
                        gameController.UpdateScoreText(100 * hitLevel);
                        if (noteObject.isLongNote)
                        {
                            HasLongNote = true;
                            CreateHitLongEffect();
                            //noteObject.OnHitLongNote();
                        }
                        else if (noteObject.isLongNoteEnd)
                        {
                            HasLongNote = false;
                        }
                        else
                        {
                            CreateHitEffect();
                        }

                        //增加连接数
                        gameController.comboNum++;
                        //Debug.Log("comboNum:" + gameController.comboNum);
                    }
                    else
                    {
                        //未击中
                        //断掉玩家命中连接数
                        gameController.HideComboNumText();
                        gameController.comboNum = 0;
                    }
                    noteObject.OnHit();
                }
                else
                {
                    CreateDownEffect();
                }
            }
            else
            {
                CreateDownEffect();
            }
        }

        /// <summary>
        /// 检测长音符
        /// </summary>
        public void CheckLongNote()
        {
            if(HasLongNote)
            {
                NoteObject noteObject = trackedNotes.Peek();
                trackedNotes.Dequeue();
                gameController.ReturnNoteObjectToPool(noteObject);
            }
        }
    }
}