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

    class LaunchLogicSystem : ILogicSystem
    {
        private Status _status;

        Status IExecute.status
        {
            get => _status;
            set => _status = value;
        }

        public void Release()
        {
            throw new NotImplementedException();
        }


        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}