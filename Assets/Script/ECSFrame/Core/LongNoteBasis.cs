using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 长音符数据
    /// </summary>
    public class LongNoteBasis : MonoBehaviour
    {
        [Header("初始位置")]
        public List<Transform> createPos = new List<Transform>();
        [Header("击中位置")]
        public List<float3> hitPos = new List<float3>();
        [Header("终点位置")]
        public List<float3> stopPos = new List<float3>();
        [Header("生成数量（每50毫秒）")]
        public int createNum_perMS = 1;
        [Header("移动速度")]
        public float moveSpeed = 0.35f;
        [Header("生命周期")]
        public float lifeTime = 5.0f;

        private static LongNoteBasis Instance;

        public static LongNoteBasis GetInstance()
        {
            if (Instance == null)
            {
                Instance = new LongNoteBasis();
            }
            return Instance;
        }
        void Awake()
        {
            Instance = this;
        }

        public Vector3 GetCreatePos(int i)
        {
            return createPos[i].position;
        }

        public float3 GetHitPos(int i)
        {
            return hitPos[i];
        }

        public float3 GetStopPos(int i)
        {
            return stopPos[i];
        }
    }
}
