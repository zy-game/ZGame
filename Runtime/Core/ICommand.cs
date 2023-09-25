using System;
using System.Collections.Generic;
using ZEngine;

namespace ZEngine
{
    public interface ICommand : IDisposable
    {
        string command { get; }
        void OnComplate(object result);

        void Subscribe(ISubscriber subscriber);
        public static ICommand Create(string command, Action action) => ICommand<object>.CommonExecuteCommand.Create(command, action);

        public static ICommand Create(string command, Action<object> action) => ICommand<object>.CommonExecuteCommand.Create(command, action);

        public static ICommand<T> Create<T>(string command, Action<T> action) => ICommand<T>.CommonExecuteCommand.Create(command, action);
    }

    public interface ICommand<T> : ICommand
    {
        void OnComplate(T result);

        void Subscribe(ISubscriber<T> subscriber);

        class CommonExecuteCommand : ICommand<T>
        {
            private Action<T> callback;
            public string command { get; private set; }

            public void OnComplate(T result) => callback?.Invoke(result);
            public void Subscribe(ISubscriber<T> subscriber)
            {
                throw new NotImplementedException();
            }

            public void OnComplate(object result)
            {
                if (result is not T)
                {
                    Engine.Console.Error("未知类型", result.GetType(), typeof(T));
                    return;
                }

                OnComplate((T)result);
            }

            public void Subscribe(ISubscriber subscriber)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                callback = null;
                command = String.Empty;
                GC.SuppressFinalize(this);
            }

            public static CommonExecuteCommand Create(string command, Action action) => Create(command, _ => action());

            public static CommonExecuteCommand Create(string command, Action<T> action)
            {
                CommonExecuteCommand commonExecuteCommand = new CommonExecuteCommand();
                commonExecuteCommand.callback = action;
                commonExecuteCommand.command = command;
                return commonExecuteCommand;
            }
        }
    }
}