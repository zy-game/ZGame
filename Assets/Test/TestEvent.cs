using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

public class TestGameEventArgs : GameEventArgs<TestGameEventArgs>
{
}

public class TestGameEventSubscribe : GameEventSubscribe<TestGameEventArgs>
{
    protected override void Execute(TestGameEventArgs eventArgs)
    {
        Engine.Console.Log("=========================");
    }
}

public class TestEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestGameEventSubscribe subscribe = Engine.Reference.Dequeue<TestGameEventSubscribe>();
        TestGameEventArgs.Subscribe(subscribe);
        TestGameEventArgs.Execute();
    }

    // Update is called once per frame
    void Update()
    {
    }
}