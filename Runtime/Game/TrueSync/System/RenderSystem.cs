using BEPUConvertion;
using NotImplementedException = System.NotImplementedException;

namespace ZGame.Game.LockStep.System
{
    public class RenderSystem : ILateUpdateSystem
    {
        public uint priority { get; }


        public virtual void DoAwake(World world, params object[] args)
        {
        }

        public virtual void DoLateUpdate(World world)
        {
            foreach (var gameEntity in world.GetEntities())
            {
                if (gameEntity.TryGetComponent<Renderer>(out var renderer) is false)
                {
                    continue;
                }

                BEPUutilities.Vector3 pos = renderer.entity.Position;
                UnityEngine.Vector3 unityPosition = MathConverter.Convert(pos);
                unityPosition -= renderer.offset;
                renderer.transform.position = unityPosition;
                BEPUutilities.Quaternion rot = renderer.entity.Orientation;
                UnityEngine.Quaternion r = MathConverter.Convert(rot);
                renderer.transform.rotation = r;
            }
        }

        public virtual void Release()
        {
        }
    }
}