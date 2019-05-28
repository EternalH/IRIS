using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IRIS
/// </summary>
namespace IRIS
{
    /// <summary>
    /// 特效自销毁
    /// </summary>
    public class DestoryEffect : MonoBehaviour
    {
        #region 属性
        public RhythmGameController gameController;

        public bool isHitted;

        public float animationTime;
        #endregion

        private void OnEnable()
        {
            Invoke("ReturnToPool", animationTime);
        }

        /// <summary>
        /// 返回对象池
        /// </summary>
        void ReturnToPool()
        {
            if (isHitted)
            {
                gameController.ReturnEffectGoToPool(gameObject, gameController.hitNoteEffectObjectPool);
                gameObject.SetActive(false);
            }
            else
            {
                gameController.ReturnEffectGoToPool(gameObject, gameController.downEffectObjectPool);
                gameObject.SetActive(false);
            }
        }
    }
}

