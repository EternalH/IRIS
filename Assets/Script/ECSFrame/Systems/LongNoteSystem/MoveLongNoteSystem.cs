using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 长音符移动系统
    /// </summary>
    public class MoveLongNoteSystem : JobComponentSystem
    {
        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, LongNoteComponent>
        {
            public void Execute(ref Position position, ref LongNoteComponent longNote)
            {
                //LongNoteBasis.Instance.GetStopPos(LaneController.NoteNum - 1)
                Vector3 pos = new Vector3(-1.65f, -0.2f, -10);
                //if (LaneController.InLongNote)
                //{
                    float3 forceDir = LongNoteBasis.GetInstance().GetStopPos(LaneController.NoteNum - 1) - longNote.position;
                    forceDir = math.normalize(forceDir) * 1f;
                    longNote.Velocity = forceDir;

                    //longNote.position += longNote.Velocity * LongNoteBasis.Instance.moveSpeed;

                    position.Value = longNote.position;
                //}

            }

        }

        //系统会每帧调用这个函数
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            //初始化一个job
            var job = new JobProcess { };

            //开始job      
            return job.Schedule(this, inputDeps);
        }
    }
}