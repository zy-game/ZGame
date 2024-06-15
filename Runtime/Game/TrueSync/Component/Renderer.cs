using BEPUphysics.Entities;
using BEPUutilities;

namespace ZGame.Game.LockStep
{
    public class Renderer : IComponent
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public uint uid;

        /// <summary>
        /// 物理实体对象
        /// </summary>
        public Entity entity;
        
        public TransformComponent transform;

        public UnityEngine.Vector3 offset;

        public void Release()
        {
        }
    }
}