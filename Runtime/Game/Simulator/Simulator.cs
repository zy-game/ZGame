using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using TrueSync;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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

        // private const float JitterTimeFactor = 0.001f;
        private static FP lockedTimeStep = 0.05;
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

        /// <summary>
        /// 当前场景中玩家数量
        /// </summary>
        public int PlayerCount => users.Count;

        public static Simulator Create3D(SimulatorNetworkHandle simulatorNetworkHandle, FP LockedTime)
        {
            lockedTimeStep = LockedTime;
            Application.targetFrameRate = 20;
            QualitySettings.vSyncCount = 0;
            Time.fixedDeltaTime = 0.02f;
            Simulator simulator = GameFrameworkFactory.Spawner<Simulator>();
            simulator.recordable = GameFrameworkFactory.Spawner<Recordable>();
            simulator.users = new();
            simulator.networkHandle = simulatorNetworkHandle;
            simulator.networkHandle.AddEventListener(simulator.OnRiceEvent);
            simulator.m_fFrameLen = lockedTimeStep.AsFloat();
            return simulator;
        }

        public void Rollback(int frame)
        {
            recordable.Reset(frame);
        }

//累计运行的时间
        float m_fAccumilatedTime = 0;

        //下一个逻辑帧的时间
        float m_fNextGameTime = 0;

        //预定的每帧的时间长度
        float m_fFrameLen;


        //两帧之间的时间差
        float m_fInterpolation = 0;

        public void FixedUpdate()
        {
            PhysicsManager.instance.UpdateStep();
            if (state is not RoomState.Running)
            {
                return;
            }

            float deltaTime = UnityEngine.Time.deltaTime;
            /**************以下是帧同步的核心逻辑*********************/
            m_fAccumilatedTime = m_fAccumilatedTime + deltaTime;

            //如果真实累计的时间超过游戏帧逻辑原本应有的时间,则循环执行逻辑,确保整个逻辑的运算不会因为帧间隔时间的波动而计算出不同的结果
            while (m_fAccumilatedTime > m_fNextGameTime)
            {
                //计算下一个逻辑帧应有的时间
                m_fNextGameTime += m_fFrameLen;
                GetUserInput();
                SyncStep();
            }

            //计算两帧的时间差,用于运行补间动画
            m_fInterpolation = (m_fAccumilatedTime + m_fFrameLen - m_fNextGameTime) / m_fFrameLen;

            /**************帧同步的核心逻辑完毕*********************/
        }

        public void Update()
        {
            if (state is not RoomState.Running)
            {
                return;
            }
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
            GameFrameworkEntry.Logger.Log(join.position);
            PhysicsManager.InitializedGameObject(frameSyncComponent.transform.gameObject, join.position, join.rotation);

            if (join.uid == localUid)
            {
                Camera camera = GameFrameworkEntry.ECS.curWorld.SetSubCamera("follower", 1);
                GameFrameworkEntry.ECS.curWorld.SetSubCameraPositionAndRotation("follower", Vector3.zero, Quaternion.identity);
                CinemachineVirtualCamera virtualCamera = camera.gameObject.AddComponent<CinemachineVirtualCamera>();
                virtualCamera.LookAt = transformComponent.transform;
                virtualCamera.Follow = transformComponent.transform;
                //camera.GetComponent<UniversalAdditionalCameraData>()
                camera.orthographic = true;
                camera.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                CinemachineFramingTransposer transposer = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
                transposer.m_CameraDistance = 10;
            }
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