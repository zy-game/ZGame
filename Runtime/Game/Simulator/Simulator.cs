using System.Collections.Generic;
using System.Linq;
using TrueSync;
using UnityEngine;
using ZGame.Networking;

namespace ZGame.Game
{
    public class Simulator : IReferenceObject
    {
        private long tick;
        private List<UserData> users;
        private Recordable recordable;
        private const float JitterTimeFactor = 0.001f;
        private static FP lockedTimeStep;
        private FP tsDeltaTime = 0;

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

        public static Simulator Create3D()
        {
            PhysicsManager.instance = new Physics3DSimulator();
            PhysicsManager.instance.Gravity = new TSVector(0, -10, 0);
            PhysicsManager.instance.SpeculativeContacts = true;
            PhysicsManager.instance.LockedTimeStep = 0.0167;
            PhysicsManager.instance.Init();
            Simulator simulator = GameFrameworkFactory.Spawner<Simulator>();
            simulator.recordable = GameFrameworkFactory.Spawner<Recordable>();
            simulator.users = new();
            SimulatorNetworkHandle.instance.AddEventListener(simulator.OnRiceEvent);
            return simulator;
        }

        public void Rollback(long frame)
        {
            tick = frame;
        }

        private bool isReady()
        {
            return users.All(x => x.isReady);
        }

        public void FixedUpdate()
        {
            if (isReady() is false)
            {
                return;
            }

            tsDeltaTime += UnityEngine.Time.deltaTime;

            if (tsDeltaTime >= (lockedTimeStep - JitterTimeFactor))
            {
                tsDeltaTime = 0;
                tick++;
                Sync();
                GetUserInput();
                PhysicsManager.instance.UpdateStep();
            }
        }

        private void Sync()
        {
            SyncData syncData = recordable.GetFrameData(tick);
            ISyncSystem system = GameFrameworkEntry.ECS.GetSystem<ISyncSystem>();
            if (system is null)
            {
                return;
            }

            system.Sync(syncData);
        }

        private void GetUserInput()
        {
            IInputSystem system = GameFrameworkEntry.ECS.GetSystem<IInputSystem>();
            if (system is null)
            {
                return;
            }

            SyncData syncData = GameFrameworkFactory.Spawner<SyncData>();
            syncData.AddFrameData(system.GetInputData());
            recordable.AddFrameData(syncData);
            SimulatorNetworkHandle.instance.OpRaiseEvent((int)SyncCode.SYNC, SyncData.Encode(syncData));
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
                case SyncCode.SYNC:
                    recordable.AddFrameData(SyncData.Decode(data));
                    break;
                case SyncCode.READY:
                    OnUserReady(Ready.Decode(data));
                    break;
            }
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
            transformComponent.gameObject = GameFrameworkEntry.VFS.GetGameObjectSync("Assets/Prefabs/Cube.prefab");
            FrameSyncComponent frameSyncComponent = transformComponent.gameObject.GetComponent<FrameSyncComponent>();
            frameSyncComponent.GameObject = transformComponent.gameObject;
            frameSyncComponent.transform = transformComponent.gameObject.GetComponent<TSTransform>();
            frameSyncComponent.rigidBody = transformComponent.gameObject.GetComponent<TSRigidBody>();
            users.Add(new UserData()
            {
                uid = join.uid,
                GameObject = transformComponent.gameObject,
                isReady = false
            });
            PhysicsManager.InitializedGameObject(frameSyncComponent.GameObject, join.position, join.rotation);
        }

        public void Update()
        {
        }

        public void Release()
        {
            SimulatorNetworkHandle.instance.OpRaiseEvent((int)SyncCode.LEAVE, Leave.Encode(new Leave() { uid = 1 }));
        }
    }
    
    // byte[] bytes = Join.Create(1, "Assets/Prefabs/Cube.prefab", TSVector.zero, TSQuaternion.identity);
    // SimulatorNetworkHandle.instance.OpRaiseEvent((byte)SyncCode.JOIN, bytes);
    //
    // await UniTask.Delay(1000);
    // SimulatorNetworkHandle.instance.OpRaiseEvent((byte)SyncCode.READY, Ready.Encode(new Ready() { uid = 1 }));
}