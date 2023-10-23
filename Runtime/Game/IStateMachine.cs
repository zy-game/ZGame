using System;
using System.Collections.Generic;

namespace ZEngine.Game
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
        IState curState { get; }

        /// <summary>
        /// 状态机持有者
        /// </summary>
        IGameEntity player { get; }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="name"></param>
        void SwitchState(string name);

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state"></param>
        void AddState(IState state);

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="name"></param>
        void RemoveState(string name);

        /// <summary>
        /// 轮询状态机
        /// </summary>
        void Update();

        public static IStateMachine Create(IGameEntity player)
        {
            return new GameStateMachine(player);
        }

        class GameStateMachine : IStateMachine
        {
            public string name { get; }
            public IState curState { get; set; }
            public IGameEntity player { get; }

            public List<IState> states;

            public GameStateMachine(IGameEntity player)
            {
                this.player = player;
                this.name = player.guid.ToString();
                this.states = new List<IState>();
            }

            public void SwitchState(string name)
            {
                if (curState is not null && curState.name == name)
                {
                    return;
                }

                IState state = this.states.Find(x => x.name == name);
                if (state is null)
                {
                    return;
                }

                if (curState is not null)
                {
                    curState.OnExit();
                    curState = null;
                }

                state.OnEntry();
                curState = state;
            }

            public void AddState(IState state)
            {
                this.states.Add(state);
            }

            public void RemoveState(string name)
            {
                IState state = this.states.Find(x => x.name == name);
                if (state is null)
                {
                    return;
                }

                if (state == curState)
                {
                    ZGame.Console.Log("不能移除当前正在执行的状态，请先切换至其它状态后在移除");
                    return;
                }

                this.states.Remove(state);
            }

            public void Update()
            {
                if (this.curState is null)
                {
                    return;
                }

                this.curState.OnUpdate();
            }

            public void Dispose()
            {
                curState?.OnExit();
                states.ForEach(x => x.Dispose());
                states.Clear();
            }
        }
    }
}