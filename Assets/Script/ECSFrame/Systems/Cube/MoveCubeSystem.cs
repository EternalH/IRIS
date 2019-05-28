using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// 方块移动系统
    /// </summary>
    public class MoveCubeSystem : JobComponentSystem
    {
        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, CubeComponent, ForceComponent>
        {
            public bool isForceOn;
            public ForceComponent.ForceMode forceMode;
            public float3 mousePosition;

            public void Execute(ref Position position, ref CubeComponent cube, ref ForceComponent forcefield)
            {
                if (isForceOn)
                {
                    float3 f = forcefield.CastForce(ref mousePosition, ref cube, forceMode);
                    ApplyForce(ref cube, f);
                }

                if (math.length(cube.Velocity) >= 0.1f)
                {
                    ApplyForce(ref cube, CalculateFriction(forcefield.frictionCoe, ref cube));
                }
                else
                {
                    Stop(ref cube);
                }

                cube.Velocity += cube.acceration;

                if (math.length(cube.Velocity) > cube.MaxLength)
                {
                    cube.Velocity = math.normalize(cube.Velocity);
                    cube.Velocity *= cube.MaxLength;
                }

                //CheckEdge(ref forcefield, ref cube);

                cube.position += cube.Velocity;

                position.Value = cube.position;

                cube.acceration *= 0;
            }

            public void ApplyForce(ref CubeComponent b, float3 force)
            {
                //F = ma
                b.acceration = b.acceration + (force / b.Mass);
            }

            public void Stop(ref CubeComponent b)
            {
                b.Velocity *= 0;
            }

            float3 CalculateFriction(float coe, ref CubeComponent b)
            {
                float3 friction = b.Velocity;
                friction *= -1;
                friction = math.normalize(friction);
                friction *= coe;

                return friction;
            }

            /// <summary>
            /// 边缘传送
            /// </summary>
            /// <param name="forcefield"></param>
            /// <param name="b"></param>
            public void CheckEdge(ref ForceComponent forcefield, ref CubeComponent b)
            {
                if (forcefield.bound.z == 0) return;

                if (b.position.x > forcefield.bound.z)
                {
                    b.position.x = forcefield.bound.x;
                }
                else if (b.position.x < forcefield.bound.x)
                {
                    b.position.x = forcefield.bound.z;
                }

                if (b.position.z > forcefield.bound.w)
                {
                    b.position.z = forcefield.bound.y;
                }
                else if (b.position.z < forcefield.bound.y)
                {
                    b.position.z = forcefield.bound.w;
                }
            }
        }

        bool _isForceOn = false;
        ForceComponent.ForceMode _forceMode = ForceComponent.ForceMode.PULL;
        float3 mousePos;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ////鼠标检测
            //if (Input.GetMouseButtonDown(0))
            //{
            //    _isForceOn = true;
            //    _forceMode = ForceComponent.ForceMode.PULL;
            //}

            //if (Input.GetMouseButtonUp(0))
            //{
            //    _isForceOn = false;
            //}

            ////right - push
            //if (Input.GetMouseButtonDown(1))
            //{
            //    _isForceOn = true;
            //    _forceMode = ForceComponent.ForceMode.PUSH;
            //}

            //if (Input.GetMouseButtonUp(1))
            //{
            //    _isForceOn = false;
            //}

            //触屏检测
            if (Input.touchCount >= 1) 
            {
                if (Input.touches[0].phase == TouchPhase.Stationary || Input.touches[0].phase == TouchPhase.Moved) 
                {
                    _isForceOn = true;
                    _forceMode = ForceComponent.ForceMode.PULL;
                }

                if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled) 
                {
                    _isForceOn = false;
                }

                //right - push
                if (Input.touches[0].phase == TouchPhase.Began) 
                {
                    _isForceOn = true;
                    _forceMode = ForceComponent.ForceMode.PUSH;
                }

                if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
                {
                    _isForceOn = false;
                }
            }


            //mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.touches[0].position.x, Input.touches[0].position.y,  248f/*Camera.main.transform.position.y*/));
            //mousePos.z = 257.0f;
            mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, 257f/*Camera.main.transform.position.y*/));
            mousePos.z = 257.0f;


            //初始化一个job
            var job = new JobProcess { isForceOn = _isForceOn, forceMode = _forceMode, mousePosition = mousePos };

            //开始job      
            return job.Schedule(this, inputDeps);
        }

    }
}