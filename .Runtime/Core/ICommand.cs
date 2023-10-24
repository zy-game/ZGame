using System;
using System.Collections.Generic;

namespace ZEngine
{
    public interface ICommand : IDisposable
    {
        string command { get; }

        T GetValue<T>();

        public static ICommand Create(string command, params object[] args)
        {
            return new ConsoleCommand(command, args);
        }

        class ConsoleCommand : ICommand
        {
            public string command { get; }
            private Queue<object> data;

            public ConsoleCommand(string command, params object[] args)
            {
                this.command = command;
                data = new Queue<object>();
                foreach (var VARIABLE in args)
                {
                    data.Enqueue(VARIABLE);
                }
            }

            public void Dispose()
            {
                data.Clear();
            }


            public T GetValue<T>()
            {
                if (data.Count == 0)
                {
                    return default;
                }

                return (T)data.Dequeue();
            }
        }
    }
}