using UnityEngine;
using Unity.Entities;

/// <summary>
/// IRIS·ECS
/// </summary>
namespace IRIS.ECS
{
    /// <summary>
    /// System控制类
    /// </summary>
    public class SystemController : MonoBehaviour
    {
        private void Awake()
        {
            StopSystem();
        }

        public static void StopSystem()
        {
            World.Active.GetExistingManager<GenerateCubeSystem>().Enabled = false;
            World.Active.GetExistingManager<GenerateLongNoteSystem>().Enabled = false;
            World.Active.GetExistingManager<GenerateNoteSystem>().Enabled = false;

            World.Active.GetExistingManager<MoveCubeSystem>().Enabled = false;
            World.Active.GetExistingManager<MoveLongNoteSystem>().Enabled = false;
            World.Active.GetExistingManager<MoveNoteSystem>().Enabled = false;

            World.Active.GetExistingManager<DestroyCubeSyatem>().Enabled = false;
            World.Active.GetExistingManager<DestroyLongNoteSyatem>().Enabled = false;
            World.Active.GetExistingManager<DestroyNoteSyatem>().Enabled = false;
        }

        public static void StartSystem()
        {
            World.Active.GetExistingManager<GenerateCubeSystem>().Enabled = true;
            World.Active.GetExistingManager<GenerateLongNoteSystem>().Enabled = true;

            World.Active.GetExistingManager<MoveCubeSystem>().Enabled = true;
            World.Active.GetExistingManager<MoveLongNoteSystem>().Enabled = true;

            World.Active.GetExistingManager<DestroyCubeSyatem>().Enabled = true;
            World.Active.GetExistingManager<DestroyLongNoteSyatem>().Enabled = true;
        }
    }
}

