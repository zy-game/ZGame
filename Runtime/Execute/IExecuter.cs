using System;

namespace ZGame.Execute
{
    public interface IExecutePipeline : IEntity
    {
        void Execute(params object[] args);
    }

    public interface IExecuteManager : IEntity
    {
        public void Execute<T>(params object[] args) where T : IExecutePipeline
            => Execute(typeof(T), args);

        public void Execute(string typeName, params object[] args)
            => Execute(AppDomain.CurrentDomain.GetTypeForThat(typeName));

        void Execute(Type type, params object[] args);
    }
}