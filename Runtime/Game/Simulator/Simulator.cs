using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using FixMath.NET;
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
    public class Simulator : IMessageHandler
    {
        private static Fix64 lockedTimeStep = Fix64.FromRaw(10);


        /// <summary>
        /// 创建模拟器
        /// </summary>
        /// <param name="LockedTime"></param>
        /// <returns></returns>
        public static async UniTask<Simulator> Create3DSimulator(Fix64 LockedTime, string hosting, ushort port)
        {
            lockedTimeStep = LockedTime;
            Application.targetFrameRate = 20;
            QualitySettings.vSyncCount = 0;
            Time.fixedDeltaTime = 0.02f;
            var simulator = new Simulator();
            await GameFrameworkEntry.Network.Connect<UdpClient>(hosting, port, simulator);
            return simulator;
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void Release()
        {
        }

        public void Active(INetClient client)
        {
            throw new NotImplementedException();
        }

        public void Inactive(INetClient client)
        {
            throw new NotImplementedException();
        }

        public void Receive(INetClient client, IByteBuffer message)
        {
            throw new NotImplementedException();
        }

        public void Exception(INetClient client, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}