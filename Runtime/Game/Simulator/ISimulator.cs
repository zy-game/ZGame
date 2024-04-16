using FixMath.NET;

namespace ZGame.Game
{
    public interface ISimulator : IReference
    {
        /// <summary>
        /// 对局id
        /// </summary>
        int id { get; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 帧率步长
        /// </summary>
        Fix64 DeltaTime { get; }

        /// <summary>
        /// 游戏状态
        /// </summary>
        GameState state { get; }


        void Update();
        void FixedUpdate();
        void LateUpdate();
        void OnGUI();
        void OnDrawGizmos();
        void OnUserJoin(int uid);
        void OnUserLeave(int uid);
        
    }

    public class InputSystem
    {
        public static void Trigger(byte type, Fix64 value)
        {
        }

        public static UserInput[] GetUserInput(int frame)
        {
            return null;
        }
    }
}