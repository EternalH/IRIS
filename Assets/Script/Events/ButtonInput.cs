using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// IRIS·Event
/// </summary>
namespace IRIS.Event
{
    /// <summary>
    /// 检测按键输入
    /// </summary>
    public class ButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject game;
        public GameObject lane;

        private bool isDown = false;

        void Update()
        {
            if (isDown)
            {
                lane.GetComponent<LaneController>().CheckUIButtonstay();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lane.GetComponent<LaneController>().CheckUIButtonDown();
            isDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            lane.GetComponent<LaneController>().CheckUIButtonUp();
            isDown = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}

