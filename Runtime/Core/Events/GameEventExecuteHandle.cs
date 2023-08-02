using System;
using System.Collections.Generic;

namespace ZEngine
{
    class GameEventExecuteHandle : IExecute
    {
        private Status _status;

        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return _status == Status.Success;
        }

        public void Execute<T>(T args, params ISubscribe[] subscribes) where T : GameEventArgs<T>
        {
            if (subscribes is null || subscribes.Length is 0)
            {
                return;
            }

            for (int i = 0; i < subscribes.Length; i++)
            {
                if (_status is not Status.Execute)
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
                    _status = Status.Failed;
                    Engine.Console.Error(e);
                }
            }

            _status = Status.Success;
            Engine.Class.Release(args);
        }

        public void Release()
        {
            _status = Status.None;
        }
    }
}