using System;
using ZGame.Window;

namespace ZGame.Window
{
    public interface UILoading : UIBase, IProgress<float>
    {
        void SetTitle(string title);

        public static UILoading OnExecute(IExecute execute, params object[] args)
        {
            return default;
        }

        public static UILoading OnExecute(Type type, params object[] args)
        {
            if (typeof(IExecute).IsAssignableFrom(type) is false)
            {
                return default;
            }

            IExecute execute = (IExecute)Activator.CreateInstance(type);
            return OnExecute(execute, args);
        }

        public static UILoading OnExecute<T>(params object[] args) where T : IExecute
        {
            return OnExecute(typeof(T), args);
        }
    }

    public interface IExecute : IDisposable
    {
    }
}