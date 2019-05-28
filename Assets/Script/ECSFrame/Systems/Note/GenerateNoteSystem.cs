using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 音符生成系统
    /// </summary>
    public class GenerateNoteSystem : ComponentSystem
    {
        public static EntityArchetype NoteArchetype;
        private static RenderMesh noteRenderer;

        /// <summary>
        /// 运行时初始化加载方法(运行前）
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            NoteArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Rotation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(NoteComponent),
                typeof(ForceComponent)
            );
        }

        /// <summary>
        /// 运行时初始化加载方法(运行后)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            noteRenderer = GetLookFromPrototype("NotePrototype");
        }

        public static RenderMesh GetLookFromPrototype(string protoName)
        {
            var proto = GameObject.Find(protoName);
            var result = proto.GetComponent<RenderMeshComponent>().Value;
            return result;
        }

        /// <summary>
        /// 生成音符
        /// </summary>
        public void GenerateNote()
        {
            if (LaneController.HitOrNot)
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < CubeBasis.GetInstance().createNum_Cube; i++)
                {
                    Entity note = entityManager.CreateEntity(NoteArchetype);

                    float3 randomVel = UnityEngine.Random.onUnitSphere;
                    //float3 randomVel = UnityEngine.Random.value;

                    float3 mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, 248/*Camera.main.transform.position.z*/));
                    mousePos.z = 248.0f;
                    float3 initialPosition = new float3(mousePos.x + randomVel.x, mousePos.y + randomVel.y, mousePos.z + randomVel.z);

                    entityManager.SetComponentData(note, new Position { Value = initialPosition });
                    //entityManager.SetComponentData(note, new Rotation { Value = Quaternion.Euler(90, 0, 0) });
                    entityManager.SetComponentData(note, new Scale { Value = new float3(0.75f, 0.75f, 0.75f) });

                    NoteComponent n = new NoteComponent
                    {
                        position = initialPosition,
                        acceration = float3.zero,
                        lifeTime = CubeBasis.GetInstance().lifeTime_Cube,

                        Mass = CubeBasis.GetInstance().mass,
                        Radius = CubeBasis.GetInstance().radius,
                        MaxLength = CubeBasis.GetInstance().maxLength,
                        Velocity = new float3(0, 0, randomVel.z * 100f),
                    };

                    entityManager.SetComponentData(note, n);

                    float4 v = new float4(-960f, -540f, 960f, 540f);

                    ForceComponent f = new ForceComponent { mouseMass = 50f, bound = v, frictionCoe = 0.1f };

                    entityManager.SetComponentData(note, f);

                    entityManager.SetSharedComponentData(note, noteRenderer);
                }
                LaneController.HitOrNot = false;
            }
        }

        protected override void OnUpdate()
        {
            GenerateNote();
        }
    }
}

