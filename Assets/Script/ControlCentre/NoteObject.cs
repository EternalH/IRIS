using UnityEngine;
using SonicBloom.Koreo;

/// <summary>
/// IRIS
/// </summary>
namespace IRIS
{
    /// <summary>
    /// 音符节点
    /// </summary>
    public class NoteObject : MonoBehaviour
    {
        #region 音符属性
        public SpriteRenderer visuals;

        public Sprite[] noteSprites;

        public bool isLongNote;

        public bool isLongNoteEnd;

        private int spriteNum;

        KoreographyEvent trackedEvent;

        LaneController laneController;

        RhythmGameController gameController;

        //偏移量
        public int hitOffset;

        #endregion

        void Start()
        {

        }

        void Update()
        {
            if (gameController.isPauseState)
            {
                return;
            }

            UpdatePosition();
            GetHitOffset();
            if (transform.position.z <= laneController.targetBottomTrans.position.z)
            {
                gameController.ReturnNoteObjectToPool(this);
                ResetNote();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="evt">KoreographyEvent对象</param>
        /// <param name="noteNum">音符数量</param>
        /// <param name="laneCont">音轨控制类</param>
        /// <param name="gameCont">游戏控制类</param>
        /// <param name="isLongStart">长音符头</param>
        /// <param name="isLongEnd">长音符尾</param>
        public void Initialize(KoreographyEvent evt, int noteNum, LaneController laneCont,
            RhythmGameController gameCont, bool isLongStart, bool isLongEnd)
        {
            trackedEvent = evt;
            laneController = laneCont;
            gameController = gameCont;
            isLongNote = isLongStart;
            isLongNoteEnd = isLongEnd;
            spriteNum = noteNum;
            if (isLongNote)
            {
                spriteNum += 4;
            }
            else if (isLongNoteEnd)
            {
                spriteNum += 8;
            }
            visuals.sprite = noteSprites[spriteNum - 1];
        }

        public void InitializeLongNote(int noteNum, LaneController laneCont, RhythmGameController gameCont)
        {
            laneController = laneCont;
            gameController = gameCont;
            int spriteNum = noteNum + 4;
            visuals.sprite = noteSprites[spriteNum - 1];
        }

        /// <summary>
        /// 将Note对象重置
        /// </summary>
        private void ResetNote()
        {
            trackedEvent = null;
            laneController = null;
            gameController = null;
        }

        /// <summary>
        /// 返回对象池
        /// </summary>
        void ReturnToPool()
        {
            gameController.ReturnNoteObjectToPool(this);
            ResetNote();
        }

        /// <summary>
        /// 击中音符对象
        /// </summary>
        public void OnHit()
        {
            ReturnToPool();
        }

        /// <summary>
        /// 击中长音符对象
        /// </summary>
        public void OnHitLongNote()
        {
            if(LaneController.InLongNote)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        void UpdatePosition()
        {
            Vector3 pos = laneController.TargetPosition;

            pos.z -= (gameController.DelayedSampleTime - trackedEvent.StartSample) / (float)gameController.SampleRate * (gameController.noteSpeed * 5.0f + 5.0f);

            transform.position = pos;
        }

        /// <summary>
        /// 得到点击偏移量
        /// </summary>
        void GetHitOffset()
        {
            int curTime = gameController.DelayedSampleTime;
            int noteTime = trackedEvent.StartSample;
            int hitWindow = gameController.HitWindowSampleWidth;
            hitOffset = hitWindow - Mathf.Abs(noteTime - curTime);
        }

        /// <summary>
        /// 当前音符是否已经Miss
        /// </summary>
        /// <returns></returns>
        public bool IsNoteMissed()
        {
            bool bMissed = true;
            if (enabled)
            {
                int curTime = gameController.DelayedSampleTime;
                int noteTime = trackedEvent.StartSample;
                int hitWindow = gameController.HitWindowSampleWidth;

                bMissed = curTime - noteTime > hitWindow;
            }
            return bMissed;
        }

        /// <summary>
        /// 音符的命中等级
        /// </summary>
        /// <returns></returns>
        public int IsNoteHittable()
        {
            int hitLevel = 0;
            if (hitOffset <= 0)
            {
                if (hitOffset > -6000 && hitOffset <= 0)
                {
                    hitLevel = 2;
                }
                else if (hitOffset >= -14000 && hitOffset <= -6000)
                {
                    hitLevel = 1;
                }
            }
            else
            {
                enabled = false;
            }

            return hitLevel;
        }
    }
}
