namespace ZEngine.World
{
    public interface IComponent : IReference
    {
    }

    public sealed class TransformComponent : IComponent
    {
        public void Release()
        {
        }
    }
}