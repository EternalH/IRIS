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
    /// 音符生命周期系统
    /// </summary>
    public class DestroyNoteSyatem : JobComponentSystem
    {
        public class DestroyEntityBarrier : BarrierSystem
        { }
        public struct Group
        {
            public ComponentDataArray<NoteComponent> note;
        }

        [Inject] private Group _Group;

        [Inject] DestroyEntityBarrier barrier;

        struct JobProcess : IJobProcessComponentDataWithEntity<NoteComponent, Scale>
        {
            public EntityCommandBuffer.Concurrent entityCommandBuffer;
            public float deltaTime;

            public void Execute(Entity entity, int index, ref NoteComponent note, ref Scale scale)
            {
                note.lifeTime = note.lifeTime - deltaTime;


                if (note.lifeTime > 0.5 && note.lifeTime <= 2)
                {
                    //do not change
                }
                else if (note.lifeTime > 0.2 && note.lifeTime <= 0.25)
                {
                    scale.Value = new float3(scale.Value.x, 1f, (0.25f - note.lifeTime) * 1000);
                }
                else if (note.lifeTime > 0.1 && note.lifeTime <= 0.2)
                {
                    //do not change
                }
                else if (note.lifeTime > 0 && note.lifeTime <= 0.1)
                {
                    scale.Value = new float3(scale.Value.x, 1f, (note.lifeTime) * 500);
                }
                else if (note.lifeTime <= 0)
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

