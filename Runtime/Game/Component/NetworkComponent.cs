namespace ZGame.Game
{
    public class NetworkComponent : IComponent
    {
        public uint id;
        public uint uid;

        public virtual void Release()
        {
        }
    }
}