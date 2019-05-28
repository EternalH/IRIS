using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;

namespace IRIS.ECS
{
    /// <summary>
    /// 长音符生命周期系统
    /// </summary>
    public class DestroyLongNoteSyatem : JobComponentSystem
    {
        public class DestroyEntityBarrier : BarrierSystem
        { }
        public struct Group
        {
            public ComponentDataArray<LongNoteComponent> longNote;
        }

        [Inject] private Group _Group;

        [Inject] DestroyEntityBarrier barrier;

        struct JobProcess : IJobProcessComponentDataWithEntity<LongNoteComponent, Scale>
        {
            public EntityCommandBuffer.Concurrent entityCommandBuffer;
            public float deltaTime;

            public void Execute(Entity entity, int index, ref LongNoteComponent longNote, ref Scale scale)
            {
                if (longNote.position.z <= LongNoteBasis.GetInstance().GetHitPos(LaneController.NoteNum - 1).z + 1.0f &&
                    longNote.position.z >= LongNoteBasis.GetInstance().GetHitPos(LaneController.NoteNum - 1).z - 1.0f &&
                    LaneController.HitOrNot) 
                {
                    entityCommandBuffer.DestroyEntity(index, entity);
                }


                longNote.lifeTime = longNote.lifeTime - deltaTime;

                if (longNote.lifeTime <= 0)
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

