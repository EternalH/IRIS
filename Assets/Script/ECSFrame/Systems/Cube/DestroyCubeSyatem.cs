using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 方块生命周期系统
    /// </summary>
    public class DestroyCubeSyatem : JobComponentSystem
    {
        public class DestroyEntityBarrier : BarrierSystem
        { }
        public struct Group
        {
            public ComponentDataArray<CubeComponent> cube;
        }

        [Inject] private Group _Group;

        [Inject] DestroyEntityBarrier barrier;

        struct JobProcess : IJobProcessComponentDataWithEntity<CubeComponent, Scale>
        {
            public EntityCommandBuffer.Concurrent entityCommandBuffer;
            public float deltaTime;

            public void Execute(Entity entity, int index, ref CubeComponent cube, ref Scale scale)
            {
                cube.lifeTime = cube.lifeTime - deltaTime;


                if (cube.lifeTime > 0.5 && cube.lifeTime <= 2)
                {
                    //do not change
                }
                else if (cube.lifeTime > 0.2 && cube.lifeTime <= 0.25)
                {
                    scale.Value = new float3(scale.Value.x, 1f, (0.25f - cube.lifeTime) * 1000);
                }
                else if (cube.lifeTime > 0.1 && cube.lifeTime <= 0.2)
                {
                    //do not change
                }
                else if (cube.lifeTime > 0 && cube.lifeTime <= 0.1)
                {
                    scale.Value = new float3(scale.Value.x, 1f, (cube.lifeTime) * 500);
                }
                else if (cube.lifeTime <= 0)
                {
                    entityCommandBuffer.DestroyEntity(index, entity);
                }
            }
        }

       protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer.Concurrent entityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            //初始化一个job
            var job = new JobProcess
            {
                entityCommandBuffer = entityCommandBuffer,
                deltaTime = Time.deltaTime
            };

            //开始job      
            return job.Schedule(this, inputDeps);
        }
    }
}

