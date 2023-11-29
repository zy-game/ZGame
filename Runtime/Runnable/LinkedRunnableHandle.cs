namespace ZGame.Runnable
{
    public abstract class LinkedRunnableHandle<T> : RunnableHandle where T : RunnableHandle, new()
    {
        protected T next { get; }

        public LinkedRunnableHandle()
        {
            next = new T();
        }

        public override bool IsCompletion()
        {
            if (next is null)
            {
                return true;
            }

            return next.IsCompletion();
        }
    }
}