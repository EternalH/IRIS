using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using IRIS;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 长音符生成系统
    /// </summary>
    public class GenerateLongNoteSystem : ComponentSystem
    {
        public static EntityArchetype LongNoteArchetype;
        private static RenderMesh longNoteRenderer;
        private LaneController lane;

        void Start()
        {
            lane = new LaneController();
        }

        /// <summary>
        /// 运行时初始化加载方法(运行前）
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            LongNoteArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Rotation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(LongNoteComponent)
            );
        }

        /// <summary>
        /// 运行时初始化加载方法(运行后)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            longNoteRenderer = GetLookFromPrototype("LongNotePrototype");
        }

        public static RenderMesh GetLookFromPrototype(string protoName)
        {
            var proto = GameObject.Find(protoName);
            var result = proto.GetComponent<RenderMeshComponent>().Value;
            return result;
        }

        /// <summary>
        /// 生成长音符
        /// </summary>
        public void GenerateLongNote()
        {
            if (LaneController.InLongNote) 
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();

                for (int i = 0; i < LongNoteBasis.GetInstance().createNum_perMS; i++)
                {
                    Entity longNote = entityManager.CreateEntity(LongNoteArchetype);

                    float3 randomVel = UnityEngine.Random.onUnitSphere;
                    //float3 randomVel = UnityEngine.Random.value;
                    float3 longNotePos = LongNoteBasis.GetInstance().GetCreatePos(LaneController.NoteNum - 1);
                    float3 initialPosition = new float3(longNotePos.x + randomVel.x, longNotePos.y + randomVel.y, longNotePos.z - 12f);

                    entityManager.SetComponentData(longNote, new Position { Value = initialPosition });
                    //entityManager.SetComponentData(cube, new Rotation { Value = Quaternion.Euler(90, 0, 0) });
                    entityManager.SetComponentData(longNote, new Scale { Value = new float3(0.25f, 0.25f, 0.25f) });

                    LongNoteComponent l = new LongNoteComponent
                    {
                        position = initialPosition,
                        lifeTime = LongNoteBasis.GetInstance().lifeTime,

                        Velocity = new float3(0, 0, -1f),
                    };

                    entityManager.SetComponentData(longNote, l);

                    entityManager.SetSharedComponentData(longNote, longNoteRenderer);
                }
            }
        }

        protected override void OnUpdate()
        {
            GenerateLongNote();
        }
    }
}

