using System;
using System.Collections;
using UnityEngine;

public class WaitFor : CustomYieldInstruction, IReference
{
    private Func<bool> m_Predicate;
    private float end;
    public override bool keepWaiting => !this.m_Predicate();

    public static WaitFor Create(Func<bool> func)
    {
        WaitFor wait = Engine.Class.Loader<WaitFor>();
        wait.m_Predicate = func;
        return wait;
    }

    public static WaitFor Create(float time)
    {
        WaitFor wait = Engine.Class.Loader<WaitFor>();
        wait.end = UnityEngine.Time.realtimeSinceStartup + time;
        wait.m_Predicate = () => UnityEngine.Time.realtimeSinceStartup > wait.end;
        return wait;
    }

    public static void Create(float time, Action callback)
    {
        IEnumerator Start()
        {
            yield return WaitFor.Create(time);
            callback?.Invoke();
        }

        Start().StartCoroutine();
    }

    public static void WaitFormFrameEnd(Action callback)
    {
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        Start().StartCoroutine();
    }

    public void Release()
    {
        m_Predicate = null;
    }
}