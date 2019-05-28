using UnityEngine;

namespace IRIS.ECS
{
    /// <summary>
    /// 方块数据
    /// </summary>
    public class CubeBasis : MonoBehaviour
    {
        [Header("生成方块数量")]
        public int createNum_Cube = 100;
        [Header("生成长方块数量")]
        public int createNum_LongCube = 500;
        [Header("方块生命周期")]
        public float lifeTime_Cube = 5.0f;
        [Header("方块生命周期")]
        public float lifeTime_LongCube = 0.5f;
        [Header("质量")]
        public float mass = 1.0f;
        [Header("半径")]
        public float radius = 1.0f;
        [Header("最大速度模长")]
        public float maxLength = 10.0f;

        private static CubeBasis Instance;

        public static CubeBasis GetInstance()
        {
            if (Instance == null)
            {
                Instance = new CubeBasis();
            }
            return Instance;
        }
        void Awake()
        {
            Instance = this;
        }

    }
}

