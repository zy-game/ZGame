using System;
using System.Collections.Generic;

namespace ZGame.State
{
    /// <summary>
    /// 状态机
    /// </summary>
    public interface IStateMachine : IDisposable
    {
        /// <summary>
        /// 状态机名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        IStateProcess current { get; }

        /// <summary>
        /// 切换状态类型
        /// </summary>
        /// <param name="type"></param>
        void Switch(Type type);

        /// <summary>
        /// 轮询状态机
        /// </summary>
        void OnUpdate();

        class StateMachine : IStateMachine
        {
            public string name { get; }
            public IStateProcess current { get; private set; }
            private Dictionary<Type, IStateProcess> _handles = new Dictionary<Type, IStateProcess>();

            public StateMachine(string name)
            {
                this.name = name;
            }

            public void Switch(Type type)
            {
                if (_handles.TryGetValue(type, out IStateProcess handle) is false)
                {
                    handle = Activator.CreateInstance(type) as IStateProcess;
                    handle.OnAwake();
                    _handles.Add(type, handle);
                }

                current?.OnExit();
                current = handle;
                current?.OnEntry();
            }

            public void OnUpdate()
            {
                if (current is null)
                {
                    return;
                }

                current.OnUpdate();
            }

            public void Dispose()
            {
                current?.OnExit();
                current = null;
                foreach (IStateProcess handle in _handles.Values)
                {
                    handle.Dispose();
                }

                _handles.Clear();
            }
        }
    }
}