using System.Collections.Generic;

namespace ZGame.State
{
    /// <summary>
    /// 状态机管理器
    /// </summary>
    public sealed class StateMachineManager : SingletonBehaviour<StateMachineManager>
    {
        private List<StateMachine> _machines = new List<StateMachine>();
        private StateMachine _default;
        public StateMachine Default => _default;

        protected override void OnAwake()
        {
            _default = new StateMachine("DEFAULT_MACHINE");
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <returns></returns>
        public StateMachine Create(string name)
        {
            StateMachine machine = GetMachine(name);
            if (machine is not null)
            {
                return machine;
            }

            machine = new StateMachine(name);
            _machines.Add(machine);
            return machine;
        }

        /// <summary>
        /// 移除状态机
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            StateMachine machine = GetMachine(name);
            if (machine is null)
            {
                return;
            }

            _machines.Remove(machine);
        }

        /// <summary>
        /// 获取状态机
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <returns></returns>
        public StateMachine GetMachine(string name)
        {
            return _machines.Find(x => x.name.Equals(name));
        }

        protected override void OnUpdate()
        {
            _default.OnUpdate();
            for (int i = _machines.Count - 1; i >= 0; i--)
            {
                _machines[i].OnUpdate();
            }
        }

        public void Dispose()
        {
            _default.Dispose();
            _machines.ForEach(x => x.Dispose());
            _machines.Clear();
        }
    }
}