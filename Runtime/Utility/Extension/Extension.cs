using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

public class WaitFor : CustomYieldInstruction, IReference
{
    private Func<bool> m_Predicate;
    public override bool keepWaiting => !this.m_Predicate();

    public static WaitFor Create(Func<bool> func)
    {
        WaitFor wait = Engine.Class.Loader<WaitFor>();
        wait.m_Predicate = func;
        return wait;
    }

    public void Release()
    {
        m_Predicate = null;
    }
}


public sealed class Timeout : CustomYieldInstruction
{
    private float end;
    public override bool keepWaiting => UnityEngine.Time.realtimeSinceStartup > end;


    public Timeout Time(float time)
    {
        end = time;
        return this;
    }


    public static Timeout Create(float time)
    {
        return new Timeout()
        {
            end = UnityEngine.Time.realtimeSinceStartup + time
        };
    }
}


public static class Extension
{
    private static UniContent _content;

    class Template
    {
        public Coroutine coroutine;
        public IEnumerator enumerator;
    }

    public static int SpiltCount(this int lenght, int l)
    {
        int count = lenght / l;
        return count * l < lenght ? count + 1 : count;
    }

    public static T TryGetValue<T>(this object[] arr, int index, T def)
    {
        if (arr.Length > index)
        {
            return (T)arr[index];
        }

        return def;
    }

    public static Coroutine StartCoroutine(this IEnumerator enumerator)
    {
        EnsureContentInstance();

        IEnumerator Running()
        {
            yield return new WaitForEndOfFrame();
            yield return enumerator;
        }

        return _content.StartCoroutine(Running());
    }


    public static void StopCoroutine(this Coroutine coroutine)
    {
        EnsureContentInstance();
        _content.StopCoroutine(coroutine);
    }

    public static void Timer(this ISubscribeHandle subscribe, float interval)
    {
        EnsureContentInstance();
        _content.AddTimer(subscribe, interval);
    }

    public static void Stoping(this ISubscribeHandle subscribe)
    {
        EnsureContentInstance();
        _content.RemoveCallback(subscribe);
    }

    private static void EnsureContentInstance()
    {
        if (_content is not null)
        {
            return;
        }

        _content = new GameObject("__ENGINE_CONTENT__").AddComponent<UniContent>();
    }

    class UniContent : MonoBehaviour
    {
        private List<ISubscribeHandle> _update = new List<ISubscribeHandle>();
        private List<ISubscribeHandle> _lateUpdate = new List<ISubscribeHandle>();
        private List<ISubscribeHandle> _fixedUpdate = new List<ISubscribeHandle>();
        private List<Timer> timer = new List<Timer>();

        private void Update()
        {
            Execute(_update);
        }

        private void FixedUpdate()
        {
            Execute(_fixedUpdate);
        }

        private void LateUpdate()
        {
            Execute(_lateUpdate);
        }

        private void Execute(List<ISubscribeHandle> subscribes)
        {
            if (subscribes.Count is 0)
            {
                return;
            }

            for (int i = 0; i < subscribes.Count; i++)
            {
                subscribes[i].Execute((object)default);
            }
        }

        public void AddUpdate(ISubscribeHandle subscribe)
        {
            _update.Add(subscribe);
            if (timer.Count is 0)
            {
                return;
            }

            for (int i = 0; i < timer.Count; i++)
            {
                if (Time.realtimeSinceStartup <= timer[i].time)
                {
                    continue;
                }

                timer[i].Execute((object)default);
            }
        }

        public void AddFixedUpdate(ISubscribeHandle subscribe)
        {
            _fixedUpdate.Add(subscribe);
        }

        public void AddLateUpdate(ISubscribeHandle subscribe)
        {
            _lateUpdate.Add(subscribe);
        }

        public void AddTimer(ISubscribeHandle subscribe, float interval)
        {
            timer.Add(new Timer(subscribe, interval));
        }

        public void RemoveCallback(ISubscribeHandle subscribe)
        {
            _update.Remove(subscribe);
            _lateUpdate.Remove(subscribe);
            _fixedUpdate.Remove(subscribe);
            Timer temp = timer.Find(x => x.subscribe == subscribe);
            if (subscribe is not null)
            {
                timer.Remove(temp);
            }
        }

        struct Timer : ISubscribeHandle
        {
            public float time;
            public ISubscribeHandle subscribe;
            public object result { get; }

            public Timer(ISubscribeHandle subscribe, float time)
            {
                this.result = default;
                this.time = Time.realtimeSinceStartup + time;
                this.subscribe = subscribe;
            }

            public void Release()
            {
                Engine.Class.Release(subscribe);
                subscribe = default;
                time = 0;
            }


            public void Execute(object value)
            {
                subscribe.Execute(value);
            }

            public void Execute(Exception exception)
            {
                subscribe.Execute(exception);
            }
        }
    }
}