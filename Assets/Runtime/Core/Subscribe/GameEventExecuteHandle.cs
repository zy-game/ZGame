using System;

namespace ZEngine
{
    class GameEventExecuteHandle : IGameExecuteHandle
    {
        public float progress { get; internal set; }
        public ExecuteStatus status { get; private set; }

        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Execute<T>(T args, IGameCancelToken cancelToken, params ISubscribe[] subscribes) where T : IEventArgs
        {
            if (subscribes is null || subscribes.Length is 0)
            {
                return;
            }

            ExecuteStatus s = ExecuteStatus.Execute;
            for (int i = 0; i < subscribes.Length; i++)
            {
                try
                {
                    GameEventSubscribe<T> eventSubscribe = (GameEventSubscribe<T>)subscribes[i];
                    eventSubscribe.Execute(args);
                }
                catch (Exception e)
                {
                    s = ExecuteStatus.Failed;
                    Engine.Console.Error(e);
                }

                progress = (float)i / subscribes.Length;
            }

            status = s == ExecuteStatus.Failed ? s : ExecuteStatus.Success;
        }

        public void Release()
        {
            progress = 0f;
            status = ExecuteStatus.None;
        }
    }
}