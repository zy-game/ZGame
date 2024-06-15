using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace ZGame.Game
{
    public class ECSWorld : World
    {
    }

    public class ECSManager : GameManager
    {
        private List<World> _worlds;

        /// <summary>
        /// 游戏世界
        /// </summary>
        public ECSWorld world { get; private set; }

        public override async void OnAwake(params object[] args)
        {
            _worlds = new List<World>();
            await LoadWorld<ECSWorld>();
            world = GetWorld<ECSWorld>();
        }

        public override void Release()
        {
            _worlds.ForEach(RefPooled.Free);
            _worlds.Clear();
        }

        protected override void Update()
        {
            for (int i = _worlds.Count - 1; i >= 0; i--)
            {
                _worlds[i].Update();
            }
        }

        protected override void FixedUpdate()
        {
            for (int i = _worlds.Count - 1; i >= 0; i--)
            {
                _worlds[i].FixedUpdate();
            }
        }

        protected override void LateUpdate()
        {
            for (int i = _worlds.Count - 1; i >= 0; i--)
            {
                _worlds[i].LateUpdate();
            }
        }

        protected override void OnDarwGizom()
        {
            for (int i = _worlds.Count - 1; i >= 0; i--)
            {
                _worlds[i].OnDarwGizom();
            }
        }

        public async UniTask<Status> LoadWorld(Assembly assembly, params object[] args)
        {
            var worldTypeList = assembly.GetAllSubClasses<World>();
            if (worldTypeList.Count == 0)
            {
                AppCore.Logger.LogError("Not found world");
                return Status.Fail;
            }

            foreach (var VARIABLE in worldTypeList)
            {
                if (await LoadWorld(VARIABLE, args) is not Status.Success)
                {
                    return Status.Fail;
                }
            }

            return Status.Success;
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async UniTask<Status> LoadWorld<T>(params object[] args) where T : World
        {
            return await LoadWorld(typeof(T), args);
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async UniTask<Status> LoadWorld(Type type, params object[] args)
        {
            World world = GetWorld(type);
            if (world != null)
            {
                return Status.Success;
            }

            AppCore.Logger.Log($"LoadWorld:{type.FullName}");
            world = (World)RefPooled.Alloc(type);
            _worlds.Add(world);
            if (await world.Awake(args) is not Status.Success)
            {
                return Status.Fail;
            }

            world.Enable();
            return Status.Success;
        }

        /// <summary>
        /// 获取世界
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWorld<T>() where T : World
        {
            return (T)GetWorld(typeof(T));
        }

        /// <summary>
        /// 获取世界
        /// </summary>
        /// <returns></returns>
        public World GetWorld(Type type)
        {
            return _worlds.Find(w => w.GetType() == type);
        }

        /// <summary>
        /// 卸载世界
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnloadWorld<T>() where T : World
        {
            UnloadWorld(typeof(T));
        }

        /// <summary>
        /// 卸载世界
        /// </summary>
        /// <param name="type"></param>
        public void UnloadWorld(Type type)
        {
            var world = GetWorld(type);
            if (world is null)
            {
                return;
            }

            world.Disable();
            RefPooled.Free(world);
            _worlds.Remove(world);
        }
    }
}