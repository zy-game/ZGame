namespace ZGame
{
    public class SubModule : ISubModule
    {
        public virtual void Active(params object[] args)
        {
        }

        public virtual void Inactive()
        {
        }

        public virtual object DOAction(string actionName, params object[] args)
        {
            return default;
        }

        public virtual T DOAction<T>(string actionName, params object[] args)
        {
            return (T)this.DOAction(actionName, args);
        }

        public virtual void Dispose()
        {
        }
    }
}