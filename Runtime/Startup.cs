using System;
using UnityEngine;
using ZEngine.World;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        Engine.Game.LaunchLogicSystem<LaunchLogicSystem>();
    }

    class LaunchLogicSystem : ILogicSystem
    {
    }
}