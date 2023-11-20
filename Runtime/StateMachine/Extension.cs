namespace ZGame.State
{
    public static partial class Extension
    {
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="machine"></param>
        /// <typeparam name="T"></typeparam>
        public static void Switch<T>(this IStateMachine machine) where T : IStateProcess
        {
            machine.Switch(typeof(T));
        }
    }
}