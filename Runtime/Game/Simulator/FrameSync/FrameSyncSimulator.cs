using System;
using BEPUphysics;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using FixMath.NET;
using ZGame.Networking;

namespace ZGame.Game
{
    public partial class FrameSyncSimulator
    {
        internal static async UniTask<FrameSyncSimulator> Create(int cid, string ip, ushort port, int lockedTimeStep)
        {
            FrameSyncSimulator simulator = RefPooled.Spawner<FrameSyncSimulator>();
            UdpClient client = await CoreAPI.Network.Connect<UdpClient>(cid, ip, port, simulator);
            if (client is null || client.isConnected is false)
            {
                return default;
            }

            simulator.frameTimeStep = lockedTimeStep;
            simulator.physiceSpace = new Space();
            simulator._deltaTime = 1000f / lockedTimeStep;
            simulator.Subscribe<UserJoin>((int)SyncOpcode.PLAYER_JOIN, simulator.OnUserJoined);
            simulator.Subscribe<UserLeave>((int)SyncOpcode.PLAYER_LEAVE, simulator.OnUserLeave);
            simulator.Subscribe<UserReady>((int)SyncOpcode.PLAYER_READY, simulator.OnUserReady);
            simulator.Subscribe<GameStart>((int)SyncOpcode.GAME_START, simulator.OnGameStart);
            simulator.Subscribe<GameOver>((int)SyncOpcode.GAME_OVER, simulator.OnGameOver);
            simulator.Subscribe<FrameData>((int)SyncOpcode.UPDATE_FRAME_DATA, simulator.OnUpdateFrameData);
            return simulator;
        }
    }

    /// <summary>
    /// 帧同步模拟
    /// </summary>
    public partial class FrameSyncSimulator : MessageDispatcher, ISimulator
    {
        private static Fix64 timeStep = new Fix64(1000);
        private int frameTimeStep;
        private DateTime startTime;
        private DateTime lastTime;
        private TimeSpan interval;
        private DateTime overTime;
        private int localUid;
        private int frameCount;
        private Space physiceSpace;
        private Fix64 _deltaTime;


        public int id { get; set; }
        public string name { get; set; }
        public GameState state { get; set; }

        public Fix64 DeltaTime
        {
            get { return _deltaTime; }
        }

        public override void Release()
        {
            this.state = GameState.None;
        }

        public void Update()
        {
            while (DateTime.Now - lastTime > interval)
            {
                lastTime += interval;
                UpdateFrame(frameCount);
                physiceSpace.Update(_deltaTime);
                frameCount++;
            }
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }

        public void OnGUI()
        {
        }

        public void OnDrawGizmos()
        {
        }

        private void UpdateFrame(int frameCount)
        {
        }

        public void OnUserJoin(int uid)
        {
            throw new NotImplementedException();
        }

        public void OnUserLeave(int uid)
        {
            throw new NotImplementedException();
        }

        private void OnGameOver(GameOver decode)
        {
            overTime = DateTime.Now;
        }

        private void OnUpdateFrameData(FrameData decode)
        {
        }

        private void OnGameStart(GameStart decode)
        {
            startTime = DateTime.Now;
            lastTime = DateTime.Now;
            interval = TimeSpan.FromMilliseconds((int)frameTimeStep);
        }

        private void OnUserReady(UserReady decode)
        {
        }

        private void OnUserJoined(UserJoin join)
        {
        }

        private void OnUserLeave(UserLeave leave)
        {
        }
    }
}