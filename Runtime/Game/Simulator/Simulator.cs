using System.Collections.Generic;
using System.Linq;
using TrueSync;
using UnityEngine;
using ZGame.Networking;

namespace ZGame.Game
{
    public enum RoomState
    {
        Ready,
        Running,
        End
    }

    // await GameFrameworkEntry.ECS.curWorld.OnStartSimulator("127.0.0.1", 8090);
    // uint uid = Crc32.GetCRC32Str(Guid.NewGuid().ToString());
    // GameFrameworkEntry.ECS.curWorld.simulator.OnJoinGame(uid, "Assets/Prefabs/player/player_1.prefab", TSVector.zero, TSQuaternion.identity);
    // await UniTask.Delay(1000);
    // GameFrameworkEntry.ECS.curWorld.simulator.OnReady();
    public class Simulator : IReferenceObject
    {
        private List<UserData> users;
        private Recordable recordable;
        private const float JitterTimeFactor = 0.001f;
        private static FP lockedTimeStep;
        private FP tsDeltaTime = 0;
        private uint localUid;
        private SimulatorNetworkHandle networkHandle;
        private RoomState state = RoomState.Ready;

        class UserData
        {
            public uint uid;
            public GameObject GameObject;
            public bool isReady;
        }

        public static FP DeltaTime
        {
            get { return lockedTimeStep; }
        }

        public static Simulator Create3D(SimulatorNetworkHandle simulatorNetworkHandle)
        {
            Simulator simulator = GameFrameworkFactory.Spawner<Simulator>();
            simulator.recordable = GameFrameworkFactory.Spawner<Recordable>();
            simulator.users = new();
            simulator.networkHandle = simulatorNetworkHandle;
            simulator.networkHandle.AddEventListener(simulator.OnRiceEvent);
            return simulator;
        }

        public void Rollback(int frame)
        {
            recordable.Reset(frame);
        }


        public void FixedUpdate()
        {
            if (state is not RoomState.Running)
            {
                return;
            }

            tsDeltaTime += UnityEngine.Time.deltaTime;

            if (tsDeltaTime < (lockedTimeStep - JitterTimeFactor))
            {
                return;
            }

            tsDeltaTime = 0;
            GetUserInput();
            SyncStep();
            PhysicsManager.instance.UpdateStep();
        }

        public void Update()
        {
        }

        private void SyncStep()
        {
            ISyncSystem system = GameFrameworkEntry.ECS.GetSystem<ISyncSystem>();
            if (system is null)
            {
                GameFrameworkEntry.Logger.Log("没有找到同步系统");
                return;
            }

            system.Sync(recordable.GetFrameData());
        }

        private void GetUserInput()
        {
            IInputSystem system = GameFrameworkEntry.ECS.GetSystem<IInputSystem>();
            if (system is null)
            {
                return;
            }

            FrameData frameData = GameFrameworkFactory.Spawner<FrameData>();
            InputData inputData = system.GetInputData();
            inputData.owner = localUid;
            frameData.SetInputData(inputData);
            networkHandle.OpRaiseEvent((int)SyncCode.SYNC, FrameData.Encode(frameData));
        }

        private void OnRiceEvent(int eventId, byte[] data)
        {
            switch ((SyncCode)eventId)
            {
                case SyncCode.JOIN:
                    OnUserJoin(Join.Decode(data));
                    break;
                case SyncCode.LEAVE:
                    OnUserLevae(Leave.Decode(data));
                    break;
                case SyncCode.READY:
                    OnUserReady(Ready.Decode(data));
                    break;
                case SyncCode.START:
                    OnGameStart(StartGame.Decode(data));
                    break;
                case SyncCode.SYNC:
                    OnSync(FrameData.Decode(data));
                    break;
                case SyncCode.END:
                    OnGameOver(GameOver.Decode(data));
                    break;
            }
        }

        private void OnGameOver(GameOver gameOver)
        {
            state = RoomState.End;
            GameFrameworkEntry.Logger.Log("游戏结束");
        }

        private void OnGameStart(StartGame startGame)
        {
            state = RoomState.Running;
            GameFrameworkEntry.Logger.Log("游戏开始");
        }

        private void OnSync(FrameData frameData)
        {
            if (frameData is null)
            {
                return;
            }

            recordable.AddFrameData(frameData);
        }

        private void OnUserReady(Ready ready)
        {
            if (ready is null || users.Exists(x => x.uid == ready.uid) is false)
            {
                return;
            }

            UserData user = users.Find(x => x.uid == ready.uid);
            user.isReady = !user.isReady;
        }

        private void OnUserLevae(Leave leave)
        {
            if (leave is null || users.Exists(x => x.uid == leave.uid) is false)
            {
                return;
            }

            UserData user = users.Find(x => x.uid == leave.uid);
            GameObject.DestroyImmediate(user.GameObject);
            users.Remove(user);
        }

        private void OnUserJoin(Join join)
        {
            if (join is null || users.Exists(x => x.uid == join.uid))
            {
                return;
            }


            Entity entity = GameFrameworkEntry.ECS.CreateEntity();
            TransformComponent transformComponent = entity.AddComponent<TransformComponent>();
            transformComponent.id = entity.id;
            transformComponent.gameObject = GameFrameworkEntry.VFS.GetGameObjectSync(join.path);
            FrameSyncComponent frameSyncComponent = entity.AddComponent<FrameSyncComponent>();
            frameSyncComponent.id = join.uid;
            frameSyncComponent.transform = transformComponent.gameObject.GetComponent<TSTransform>();
            frameSyncComponent.rigidBody = transformComponent.gameObject.GetComponent<TSRigidBody>();
            users.Add(new UserData()
            {
                uid = join.uid,
                GameObject = transformComponent.gameObject,
                isReady = false
            });
            PhysicsManager.InitializedGameObject(frameSyncComponent.transform.gameObject, join.position, join.rotation);
        }


        public void OnJoinGame(uint uid, string path, TSVector position, TSQuaternion rotation)
        {
            localUid = uid;
            byte[] bytes = Join.Create(uid, path, position, rotation);
            networkHandle.OpRaiseEvent((byte)SyncCode.JOIN, bytes);
        }

        public void OnReady()
        {
            networkHandle.OpRaiseEvent((byte)SyncCode.READY, Ready.Encode(new Ready() { uid = localUid }));
        }

        public void OnLeaveGame()
        {
            networkHandle.OpRaiseEvent((byte)SyncCode.LEAVE, Leave.Encode(new Leave() { uid = localUid }));
        }

        public void Release()
        {
            OnLeaveGame();
        }
    }
}