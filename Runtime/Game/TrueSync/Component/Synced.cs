using BEPUphysics.Entities;

namespace ZGame.Game.LockStep
{
    public class Synced : IComponent
    {
        public uint uid;
        public Entity entity;
        public Command command;

        public void Release()
        {
        }
    }
}