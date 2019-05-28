using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 长音符组件
    /// </summary>
    public struct LongNoteComponent : IComponentData
    {
        public float3 position;
        public float lifeTime;

        public float3 Velocity { get; set; }
    }
}
