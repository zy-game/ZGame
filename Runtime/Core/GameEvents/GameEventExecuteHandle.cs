using System;
using System.Collections.Generic;

namespace ZEngine
{
    class GameEventExecuteHandle : IExecuteHandle
    {
        public ExecuteStatus status { get; private set; }

        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return status == ExecuteStatus.Success;
        }

        public void Execute<T>(T args, params ISubscribe[] subscribes) where T : GameEventArgs<T>
        {
            if (subscribes is null || subscribes.Length is 0)
            {
                return;
            }

            for (int i = 0; i < subscribes.Length; i++)
            {
                if (status is not ExecuteStatus.Execute)
                {
                    return;
                }

                try
                {
                    ((ISubscribe<T>)subscribes[i]).Execute(args);
                    if (args.HasFree())
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    status = ExecuteStatus.Failed;
                    Engine.Console.Error(e);
                }
            }

            status = ExecuteStatus.Success;
            Engine.Class.Release(args);
        }

        public void Release()
        {
            status = ExecuteStatus.None;
        }
    }
}