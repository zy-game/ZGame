using System;
using System.Collections;
using UnityEngine;

public class WaitFor : CustomYieldInstruction, IDisposable
{
    private Func<bool> m_Predicate;
    public override bool keepWaiting => Check();

    private bool Check()
    {
        bool state = m_Predicate();
        if (state)
        {
            this.Dispose();
        }

        return state;
    }

    public static void Create(Func<bool> func, Action complate)
    {
        IEnumerator Waiting()
        {
            WaitFor waitFor = Create(func);
            yield return waitFor;
            complate?.Invoke();
        }

        Waiting().StartCoroutine();
    }

    public static WaitFor Create(Func<bool> func)
    {
        WaitFor wait = Activator.CreateInstance<WaitFor>();
        wait.m_Predicate = func;
        return wait;
    }

    public static WaitFor Create(float time)
    {
        WaitFor wait = Activator.CreateInstance<WaitFor>();
        float end = UnityEngine.Time.realtimeSinceStartup + time;
        wait.m_Predicate = () => UnityEngine.Time.realtimeSinceStartup > end;
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

    public void Dispose()
    {
        m_Predicate = null;
    }
}