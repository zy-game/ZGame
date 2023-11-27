using System.Collections.Generic;

namespace ZGame.State
{
    /// <summary>
    /// 状态机管理器
    /// </summary>
    public sealed class StateMachineManager : SingletonBehaviour<StateMachineManager>
    {
        private List<IStateMachine> _machines = new List<IStateMachine>();
        private IStateMachine _default;
        public IStateMachine Default => _default;

        protected override void OnAwake()
        {
            _default = new IStateMachine.StateMachine("DEFAULT_MACHINE");
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <returns></returns>
        public IStateMachine Create(string name)
        {
            IStateMachine machine = GetMachine(name);
            if (machine is not null)
            {
                return machine;
            }

            machine = new IStateMachine.StateMachine(name);
            _machines.Add(machine);
            return machine;
        }

        /// <summary>
        /// 移除状态机
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            IStateMachine machine = GetMachine(name);
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
        public IStateMachine GetMachine(string name)
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