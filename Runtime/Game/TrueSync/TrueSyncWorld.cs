using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Cysharp.Threading.Tasks;
using FixMath.NET;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Networking;
using Space = BEPUphysics.Space;
using Vector3 = BEPUutilities.Vector3;

namespace ZGame.Game.LockStep
{
    /// <summary>
    /// 房间状态
    /// </summary>
    [Flags]
    public enum RoomState
    {
        /// <summary>
        /// 空闲阶段
        /// </summary>
        None,

        /// <summary>
        /// 等待就绪阶段，一般是等所有玩家场景加载完毕
        /// </summary>
        Prepare,

        /// <summary>
        /// 比赛开始
        /// </summary>
        Running,

        /// <summary>
        /// 玩家结算,结算阶段
        /// </summary>
        Balance,

        /// <summary>
        /// 比赛结束
        /// </summary>
        End
    }

    public enum InputID : byte
    {
        Vertical,
        Horizontal,
        UP_DOWN,
        LEFT_RIGHT,
        SPACE,
    }

    /// <summary>
    /// 帧同步世界，所有处于当前世界的实体都按照帧同步方式处理
    /// </summary>
    public class TrueSyncWorld : World
    {
        /*需要同步几乎所有的状态，连顺序都不能有一点错。
         我是看github上作者回的帖子说，一个一个同步工作量很大，如果不是同步量特别大的，就直接保留好快照，
         然后直接把space清了，然后重建一个space，就能保证两边客户端的状态一定一样了
         */
        private int _rid;
        private int _cid;
        private uint _selfId;
        private string _name;
        private Space _space;
        private Random _random;
        private RoomState _state;
        private byte _maxPreCount;
        private long _authorityFrameID;
        private long _predictionFrameID;
        private List<FrameData> _frameDatas;
        private TimeSpan _gameStartSinceTime;
        private int snapshotFrameInterval = 1;
        private Dictionary<uint, bool> _racers;

        /// <summary>
        /// 比赛玩家ID列表
        /// </summary>
        public uint[] racers
        {
            get { return _racers.Keys.ToArray(); }
        }

        /// <summary>
        /// 网络连接ID
        /// </summary>
        public int cid
        {
            get { return _cid; }
        }

        /// <summary>
        /// 房间ID
        /// </summary>
        public int rid
        {
            get { return _rid; }
        }

        /// <summary>
        /// 房间名
        /// </summary>
        public string name
        {
            get { return _name; }
        }

        /// <summary>
        /// 物理空间
        /// </summary>
        public Space space
        {
            get { return _space; }
        }

        /// <summary>
        /// 游戏是否开始
        /// </summary>
        public RoomState state
        {
            get { return _state; }
        }

        /// <summary>
        /// 当前权威帧
        /// </summary>
        public long AuthorityFrameID
        {
            get { return _authorityFrameID; }
        }

        /// <summary>
        /// 当前预测帧
        /// </summary>
        public long PredictionFrameID
        {
            get { return _predictionFrameID; }
        }

        /// <summary>
        /// 预测帧最大数
        /// </summary>
        public byte PredicLimitCount
        {
            get { return _maxPreCount; }
        }

        /// <summary>
        /// 游戏总耗时
        /// </summary>
        public TimeSpan gameSinceStartup
        {
            get { return realtimeSinceStartup - _gameStartSinceTime; }
        }

        protected override async UniTask<Status> DoAwake(params object[] args)
        {
            _maxPreCount = 10;
            _authorityFrameID = -1;
            _predictionFrameID = -1;
            SetFrameRate(100);
            _space = new Space();
            _state = RoomState.None;
            _frameDatas = new(100);
            _racers = new Dictionary<uint, bool>();
            _space.ForceUpdater.Gravity = new Vector3(0, 0, 0);
            AppCore.Events.Subscribe<NetworkEventArgs>(NetEvent.Recived, OnRecive);
            _cid = await AppCore.Network.Connect<UdpClient>("127.0.0.1", 8090);
            return Status.Success;
        }

        public void AddPhysiceEntity(Entity entity)
        {
            _space.Add(entity);
        }

        public override void Release()
        {
            base.Release();
            _racers.Clear();
            AppCore.Events.Subscribe<NetworkEventArgs>(NetEvent.Recived, OnRecive);
        }


        public void SetGravity(Fix64 gravity)
        {
            _space.ForceUpdater.Gravity = new Vector3(0, gravity, 0);
        }


        protected override void DoUpdate()
        {
            if (_state is not RoomState.Running)
            {
                return;
            }

            // OnCollectPlayerOperation();
            // OnPerdictionFrameOperation();
            // DoRender();
        }

        /// <summary>
        /// 收集玩家操作并发送到服务器
        /// </summary>
        // private void OnCollectPlayerOperation()
        // {
        //     var command = DoCollectPlayerInput();
        //     using (MSG_UserInput userInput = RefPooled.Alloc<MSG_UserInput>())
        //     {
        //         userInput.command = command.Clone();
        //         AppCore.Network.Write(_cid, MSGPacket.Serialize((int)MSG_LockStep.CS_PLAYER_INPUT, userInput));
        //     }
        // }

        private FrameData GetFrameData(long frameID)
        {
            if (frameID < 0)
            {
                return default;
            }

            int index = (int)(frameID % _frameDatas.Capacity);
            if (frameID <= _authorityFrameID)
            {
                return _frameDatas[index];
            }

            FrameData predict = _frameDatas[index];
            FrameData authority = GetFrameData(_authorityFrameID);
            authority.CopyTo(predict);
            // predict.Set(_selfId, DoCollectPlayerInput());
            return predict;
        }

        /// <summary>
        /// 预测帧
        /// </summary>
        // public void OnPerdictionFrameOperation()
        // {
        //     if (_predictionFrameID - _authorityFrameID >= _maxPreCount)
        //     {
        //         return;
        //     }
        //
        //     _predictionFrameID++;
        //     DoSimulator(GetFrameData(_predictionFrameID));
        //     _space.Update(fixedDeltaTime);
        // }

        private void OnRecvieFrameData(MSG_Frame msg)
        {
            if (_state is not RoomState.Running)
            {
                AppCore.Logger.Log("游戏未开始");
                return;
            }

            _authorityFrameID = msg.frame.frameID;
            if (_authorityFrameID > _predictionFrameID) // 服务器的帧比预测的帧还快,那么只需要吧服务器的帧数据覆盖就行了
            {
                FrameData authorityFrameData = GetFrameData(_authorityFrameID);
                msg.frame.CopyTo(authorityFrameData);
            }
            else
            {
                FrameData predictionFrameData = GetFrameData(_authorityFrameID);
                if (predictionFrameData == msg.frame) // 如果服务器的帧比预测帧慢，那么需要对比当前服务器的帧数据
                {
                    return;
                }

                //直接从快照中回滚，现在还没有做快照，先从帧数据中回滚
                // AppCore.Logger.Log("预测帧和服务器帧不一致,开始回滚：" + _authorityFrameID);
                // msg.frame.CopyTo(predictionFrameData);
                // DoSimulator(predictionFrameData);
                // _space.Update(fixedDeltaTime);
                //用服务器的帧数据重做预测帧
                for (var i = _authorityFrameID + 1; i < _predictionFrameID; i++)
                {
                    FrameData predict = GetFrameData(i);
                    foreach (var VARIABLE in msg.frame.commands)
                    {
                        if (VARIABLE.uid != _selfId)
                        {
                            Command command = msg.frame.Get(VARIABLE.uid);
                            predict.Set(command.uid, command);
                        }
                    }
                }
            }
        }


        private void OnRoomInfo(MSG_RoomInfo msg)
        {
            _rid = msg.rid;
            _name = msg.rName;
            _random = new Random(msg.seed);
            AppCore.Logger.Log($"房间信息：{_rid} {_name}");
            DoRefreshRoomInfo(msg);
        }

        /// <summary>
        /// 玩家进入游戏
        /// </summary>
        /// <param name="msg"></param>
        private void OnJoinGame(MSG_UserInfo msg)
        {
            if (_state is not RoomState.None)
            {
                AppCore.Logger.LogError("游戏已经开始，无法加入");
                return;
            }

            if (_racers.ContainsKey(msg.uid))
            {
                AppCore.Logger.Log("玩家已在房间中");
                return;
            }

            AppCore.Logger.Log($"玩家进入游戏:{msg.name}");
            _racers.Add(msg.uid, false);
            DoUserJoin(msg);
        }

        /// <summary>
        /// 玩家离开游戏
        /// </summary>
        /// <param name="uid">玩家ID</param>
        /// <param name="isKick">是否掉线</param>
        private void OnLeaveGame(MSG_UserLeave msg)
        {
            if (_racers.ContainsKey(msg.uid) is false)
            {
                return;
            }

            _racers.Remove(msg.uid);
            DestroyEntity(msg.uid);
            DoUserLeave(msg.uid);
            AppCore.Logger.Log($"玩家离开游戏:{msg.uid}");
        }

        /// <summary>
        /// 玩家准备
        /// </summary>
        /// <param name="uid"></param>
        private void OnUserReady(MSG_UserReady msg)
        {
            if (_state is not RoomState.None)
            {
                AppCore.Logger.Log("游戏已经开始，不能准备");
                return;
            }

            if (_racers.ContainsKey(msg.uid) is false)
            {
                AppCore.Logger.Log("非参赛玩家不能准备:" + msg.uid);
                return;
            }

            _racers[msg.uid] = true;
            DoReady(msg.uid);
            AppCore.Logger.Log($"玩家准备:{msg.uid}");
        }

        /// <summary>
        /// 游戏开始
        /// </summary>
        private void OnGameStart(MSG_GameStart msg)
        {
            if (_state is RoomState.Running)
            {
                AppCore.Logger.Log("游戏已经开始，不能开始");
                return;
            }

            for (int i = 0; i < 100; i++)
            {
                _frameDatas.Add(FrameData.Create(i, _racers.Keys));
            }

            _gameStartSinceTime = realtimeSinceStartup;
            DoGameStart();
            AppCore.Logger.Log("游戏开始");
            _state = RoomState.Running;
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        private void OnGameOver(MSG_GameOver msg)
        {
            if (_state is not RoomState.Running)
            {
                AppCore.Logger.Log("游戏未开始，不能结束");
                return;
            }

            _state = RoomState.Balance;
            DoGameOver();
            AppCore.Logger.Log("游戏结束");
        }

        /// <summary>
        /// 准备完毕，开始加载游戏
        /// </summary>
        private async void OnStartLoadGame(MSG_LoadGame loadGame)
        {
            _state = RoomState.Prepare;
            await DoLoadGameScene();
            using (var msg = RefPooled.Alloc<MSG_LoadComplete>())
            {
                msg.rid = _rid;
                msg.uid = _selfId;
                AppCore.Network.Write(_cid, MSGPacket.Serialize((int)MSG_LockStep.CS_LOADCOMPLETE, msg));
            }
        }

        private void OnRecive(NetworkEventArgs args)
        {
            if (args.packet is null)
            {
                AppCore.Logger.Log("[CLIENT] We received message:null");
                return;
            }

            switch ((MSG_LockStep)args.packet.opcode)
            {
                case MSG_LockStep.SC_FRAME:
                    OnRecvieFrameData(args.packet.Decode<MSG_Frame>());
                    break;
                case MSG_LockStep.SC_ROOM_INFO:
                    OnRoomInfo(args.packet.Decode<MSG_RoomInfo>());
                    break;
                case MSG_LockStep.SC_GAME_OVER:
                    OnGameOver(args.packet.Decode<MSG_GameOver>());
                    break;
                case MSG_LockStep.SC_PLAYER_JOIN:
                    OnJoinGame(args.packet.Decode<MSG_UserInfo>());
                    break;
                case MSG_LockStep.SC_GAME_START:
                    OnGameStart(args.packet.Decode<MSG_GameStart>());
                    break;
                case MSG_LockStep.SC_PLAYER_LEVAE:
                    OnLeaveGame(args.packet.Decode<MSG_UserLeave>());
                    break;
                case MSG_LockStep.SC_PLAYER_READY:
                    OnUserReady(args.packet.Decode<MSG_UserReady>());
                    break;
                case MSG_LockStep.SC_LOADGAME:
                    OnStartLoadGame(args.packet.Decode<MSG_LoadGame>());
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="uid"></param>
        public void OnJoin(uint uid)
        {
            using (var msg = RefPooled.Alloc<MSG_UserJoin>())
            {
                msg.uid = _selfId = uid;
                AppCore.Network.Write(_cid, MSGPacket.Serialize((int)MSG_LockStep.CS_PLAYER_JOIN, msg));
            }
        }

        /// <summary>
        /// 准备
        /// </summary>
        public void OnReady()
        {
            using (var msg = RefPooled.Alloc<MSG_UserReady>())
            {
                msg.rid = _rid;
                msg.uid = _selfId;
                AppCore.Network.Write(_cid, MSGPacket.Serialize((int)MSG_LockStep.CS_PLAYER_READY, msg));
            }
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        public void OnLeave()
        {
            using (var msg = RefPooled.Alloc<MSG_UserLeave>())
            {
                msg.rid = _rid;
                msg.uid = _selfId;
                AppCore.Network.Write(_cid, MSGPacket.Serialize((int)MSG_LockStep.CS_PLAYER_LEAVE, msg));
            }
        }

        /// <summary>
        /// 刷新房间信息
        /// </summary>
        /// <param name="info"></param>
        protected virtual void DoRefreshRoomInfo(MSG_RoomInfo info)
        {
        }

        // /// <summary>
        // /// 收集玩家操作
        // /// </summary>
        // /// <returns></returns>
        // protected virtual Command DoCollectPlayerInput()
        // {
        //     return default;
        // }
        //
        // /// <summary>
        // /// 开始模拟帧
        // /// </summary>
        // /// <param name="frameData"></param>
        // protected virtual void DoSimulator(FrameData frameData)
        // {
        // }
        //
        // /// <summary>
        // /// 处理渲染，将物理引擎数据同步到Unity
        // /// </summary>
        // protected virtual void DoRender()
        // {
        // }

        /// <summary>
        /// 玩家进入
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void DoUserJoin(MSG_UserInfo msg)
        {
        }

        /// <summary>
        /// 玩家离开
        /// </summary>
        /// <param name="uid"></param>
        protected virtual void DoUserLeave(uint uid)
        {
        }

        /// <summary>
        /// 玩家准备
        /// </summary>
        /// <param name="uid"></param>
        protected virtual void DoReady(uint uid)
        {
        }

        /// <summary>
        /// 游戏开始
        /// </summary>
        protected virtual void DoGameStart()
        {
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        protected virtual void DoGameOver()
        {
        }

        /// <summary>
        /// 加载游戏场景
        /// </summary>
        protected virtual UniTask DoLoadGameScene()
        {
            return UniTask.CompletedTask;
        }
    }
}