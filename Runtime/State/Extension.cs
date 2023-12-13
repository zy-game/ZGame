namespace ZGame.State
{
    public static partial class Extension
    {
        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="machine"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddState<T>(this StateMachine machine) where T : StateHandle
        {
            if (machine is null)
            {
                return;
            }

            machine.AddState(typeof(T));
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="machine"></param>
        /// <typeparam name="T"></typeparam>
        public static void Switch<T>(this StateMachine machine) where T : StateHandle
        {
            machine.Switch(typeof(T));
        }
    }
}