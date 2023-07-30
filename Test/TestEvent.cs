using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

public class TestGameGameEventArgs : GameEventArgs<TestGameGameEventArgs>
{
    public override void Release()
    {
    }
}

public class TestGameEventSubscribe : GameEventSubscrbe<TestGameGameEventArgs>
{
    public override void Execute(TestGameGameEventArgs args)
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }
}

public class TestEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestGameEventSubscribe subscribe = Engine.Class.Loader<TestGameEventSubscribe>();
        TestGameGameEventArgs.Subscribe(subscribe);
        TestGameGameEventArgs.Execute();
    }

    // Update is called once per frame
    void Update()
    {
    }
}