namespace ZGame.Game
{
    public abstract class Stateable : Playable
    {
        public Stateable(string name) : base(name)
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void Active()
        {
        }

        public virtual void Inactive()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}