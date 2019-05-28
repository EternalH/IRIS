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
    /// 方块生成系统
    /// </summary>
    public class GenerateCubeSystem : ComponentSystem
    {
        public static EntityArchetype CubeArchetype;
        private static RenderMesh cubeRenderer;

        /// <summary>
        /// 运行时初始化加载方法(运行前）
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            CubeArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Rotation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(CubeComponent),
                typeof(ForceComponent)
            );
        }

        /// <summary>
        /// 运行时初始化加载方法(运行后)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            cubeRenderer = GetLookFromPrototype("CubePrototype");
        }

        public static RenderMesh GetLookFromPrototype(string protoName)
        {
            var proto = GameObject.Find(protoName);
            var result = proto.GetComponent<RenderMeshComponent>().Value;
            return result;
        }

        /// <summary>
        /// 生成方块
        /// </summary>
        public void GenerateCube()
        {
            if (!LaneController.HitOrNot && Input.GetMouseButtonDown(0))
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < CubeBasis.GetInstance().createNum_Cube; i++)
                {
                    Entity cube = entityManager.CreateEntity(CubeArchetype);

                    float3 randomVel = UnityEngine.Random.onUnitSphere;
                    //float3 randomVel = UnityEngine.Random.value;

                    float3 mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, 257/*Camera.main.transform.position.z*/));
                    mousePos.z = 257;
                    float3 initialPosition = new float3(mousePos.x + randomVel.x, mousePos.y + randomVel.y, mousePos.z + randomVel.z);

                    entityManager.SetComponentData(cube, new Position { Value = initialPosition });
                    //entityManager.SetComponentData(cube, new Rotation { Value = Quaternion.Euler(90, 0, 0) });
                    entityManager.SetComponentData(cube, new Scale { Value = new float3(0.75f, 0.75f, 0.75f) });

                    CubeComponent c = new CubeComponent
                    {
                        position = initialPosition,
                        acceration = float3.zero,
                        lifeTime = CubeBasis.GetInstance().lifeTime_Cube,

                        Mass = CubeBasis.GetInstance().mass,
                        Radius = CubeBasis.GetInstance().radius,
                        MaxLength = CubeBasis.GetInstance().maxLength,
                        Velocity = new float3(randomVel.x * 100, randomVel.y * 100, randomVel.z * 100f),
                    };

                    entityManager.SetComponentData(cube, c);

                    float4 v = new float4(-960f, -540f, 960f, 540f);

                    ForceComponent f = new ForceComponent { mouseMass = 50f, bound = v, frictionCoe = 0.1f };

                    entityManager.SetComponentData(cube, f);

                    entityManager.SetSharedComponentData(cube, cubeRenderer);
                }
                //LaneController.HitOrNot = false;
            }
            if (LaneController.HitOrNot)
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < CubeBasis.GetInstance().createNum_LongCube; i++)
                {
                    Entity cube = entityManager.CreateEntity(CubeArchetype);

                    float3 randomVel = UnityEngine.Random.onUnitSphere;
                    //float3 randomVel = UnityEngine.Random.value;

                    float3 mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, 257/*Camera.main.transform.position.z*/));
                    mousePos.z = 257;
                    float3 initialPosition = new float3(mousePos.x + randomVel.x, mousePos.y + randomVel.y, mousePos.z + randomVel.z);

                    entityManager.SetComponentData(cube, new Position { Value = initialPosition });
                    entityManager.SetComponentData(cube, new Rotation { Value = Quaternion.Euler(0, 0, 0) });
                    entityManager.SetComponentData(cube, new Scale { Value = new float3(1.0f, 1.0f, 1.0f) });

                    CubeComponent c = new CubeComponent
                    {
                        position = initialPosition,
                        acceration = float3.zero,
                        lifeTime = CubeBasis.GetInstance().lifeTime_LongCube,

                        Mass = CubeBasis.GetInstance().mass,
                        Radius = CubeBasis.GetInstance().radius,
                        MaxLength = CubeBasis.GetInstance().maxLength,
                        Velocity = new float3(0f, 0f, randomVel.z * 100f),
                    };

                    entityManager.SetComponentData(cube, c);

                    float4 v = new float4(-960f, -540f, 960f, 540f);

                    ForceComponent f = new ForceComponent { mouseMass = 50f, bound = v, frictionCoe = 0.1f };

                    entityManager.SetComponentData(cube, f);

                    entityManager.SetSharedComponentData(cube, cubeRenderer);
                }
                LaneController.HitOrNot = false;
            }
        }

        protected override void OnUpdate()
        {
            GenerateCube();
        }
    }
}

