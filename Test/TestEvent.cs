using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;
using ZEngine.Resource;

public class TestGameGameEvent : GameEventArgs
{
    public void Dispose()
    {
    }
}

public class TestEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ISubscriber.Subscribe<GameEventArgs>(Test2);
        ISubscriber.Subscribe<TestGameGameEvent>(Test);
        ISubscriber.Execute(GameEventArgs.Create("Hello"));
        ISubscriber.Execute(new TestGameGameEvent());
        
    }

    void Test2(GameEventArgs args)
    {
        Engine.Console.Log(args.Dequeue<string>());
    }

    void Test(TestGameGameEvent args)
    {
        Engine.Console.Log("---------------------1");
    }

    // Update is called once per frame
    void Update()
    {
    }
}