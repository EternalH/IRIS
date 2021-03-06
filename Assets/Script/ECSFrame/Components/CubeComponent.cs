﻿using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 方块组件
    /// </summary>
    public struct CubeComponent : IComponentData
    {
        public float3 position;
        public float3 acceration;
        public float lifeTime;

        public float Mass { get; set; }
        public float Radius { get; set; }
        public float MaxLength { get; set; }
        public float3 Velocity { get; set; }
    }
}
