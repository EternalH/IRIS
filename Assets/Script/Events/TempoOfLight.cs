using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;

/// <summary>
/// IRIS·Event
/// </summary>
namespace IRIS.Event
{
    /// <summary>
    /// 灯光节奏
    /// </summary>
    public class TempoOfLight : MonoBehaviour
    {
        [EventID]
        public string eventID;

        public List<GameObject> lights = new List<GameObject>();

        void Start()
        {
            Koreographer.Instance.RegisterForEventsWithTime(eventID, AdjustLight);
        }

        void OnDestroy()
        {
            if (Koreographer.Instance != null)
            {
                Koreographer.Instance.UnregisterForAllEvents(this);
            }
        }

        void AdjustLight(KoreographyEvent evt, int sampleTime, int sampleDelta, DeltaSlice deltaSlice)
        {
            if (evt.HasIntPayload())
            {
                if (evt.GetIntValue() == 1)
                {
                    lights[0].SetActive(true);
                    lights[1].SetActive(true);
                    lights[2].SetActive(false);
                    lights[3].SetActive(false);
                }
                else if (evt.GetIntValue() == 2)
                {
                    lights[0].SetActive(false);
                    lights[1].SetActive(false);
                    lights[2].SetActive(true);
                    lights[3].SetActive(true);
                }
                else if (evt.GetIntValue() == 3)
                {
                    lights[0].SetActive(true);
                    lights[1].SetActive(true);
                    lights[2].SetActive(true);
                    lights[3].SetActive(true);
                }
                else
                {
                    lights[0].SetActive(false);
                    lights[1].SetActive(false);
                    lights[2].SetActive(false);
                    lights[3].SetActive(false);
                }

            }
        }
    }



}

