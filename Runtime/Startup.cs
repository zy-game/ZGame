using System;
using UnityEngine;
using ZEngine;
using ZEngine.World;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        Engine.Game.LaunchLogicSystem<LaunchLogicSystem>();
    }

    class LaunchLogicSystem : ILogicSystemExecuteHandle
    {
        private Status _status;

        public void Release()
        {
            throw new NotImplementedException();
        }

        public bool EnsureExecuteSuccessfuly()
        {
            throw new NotImplementedException();
        }

        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}