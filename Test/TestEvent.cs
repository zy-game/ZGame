using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;
using ZEngine.Resource;

public class TestGameGameEventArgs : GameEventArgs<TestGameGameEventArgs>
{
    public override void Dispose()
    {
    }
}

public class TestGameGameEventArgs2 : GameEventArgs<TestGameGameEventArgs2>
{
    public override void Dispose()
    {
    }
}


public class TestEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // IAssetRequestExecute<GameObject> assetRequestExecute = Engine.Resource.LoadAsset<GameObject>("");
        // if (assetRequestExecute.result == null)
        // {
        //     Engine.Console.Error("加载：" + assetRequestExecute.path + "失败");
        //     return;
        // }
        //
        // Engine.Console.Error("加载：" + assetRequestExecute.path + "成功");
        // IAssetRequestExecuteHandle<GameObject> requestExecuteHandle = Engine.Resource.LoadAssetAsync<GameObject>("");
        // requestExecuteHandle.Subscribe(ISubscribeExecuteHandle<GameObject>.Create(args =>
        // {
        //     if (requestExecuteHandle.EnsureExecuteSuccessfuly() is false)
        //     {
        //         Engine.Console.Error("加载：" + assetRequestExecute.path + "失败");
        //         return;
        //     }
        //
        //     Engine.Console.Error("加载：" + assetRequestExecute.path + "成功");
        // }));

        TestGameGameEventArgs.Subscribe(Test);
        TestGameGameEventArgs.Execute(new TestGameGameEventArgs());

        TestGameGameEventArgs2.Subscribe(Test2);
        TestGameGameEventArgs2.Execute(new TestGameGameEventArgs2());
    }

    void Test(TestGameGameEventArgs args)
    {
        Engine.Console.Log("---------------------1");
    }

    void Test2(TestGameGameEventArgs2 args)
    {
        Engine.Console.Log("---------------------2");
    }

    // Update is called once per frame
    void Update()
    {
    }
}