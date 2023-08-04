using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

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

    public static void Timer(this ISubscribeExecuteHandle subscribe, float interval)
    {
        EnsureContentInstance();
        _content.AddTimer(subscribe, interval);
    }

    public static void Stoping(this ISubscribeExecuteHandle subscribe)
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
        private List<ISubscribeExecuteHandle> _update = new List<ISubscribeExecuteHandle>();
        private List<ISubscribeExecuteHandle> _lateUpdate = new List<ISubscribeExecuteHandle>();
        private List<ISubscribeExecuteHandle> _fixedUpdate = new List<ISubscribeExecuteHandle>();
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

        private void Execute(List<ISubscribeExecuteHandle> subscribes)
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

        public void AddUpdate(ISubscribeExecuteHandle subscribe)
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

        public void AddFixedUpdate(ISubscribeExecuteHandle subscribe)
        {
            _fixedUpdate.Add(subscribe);
        }

        public void AddLateUpdate(ISubscribeExecuteHandle subscribe)
        {
            _lateUpdate.Add(subscribe);
        }

        public void AddTimer(ISubscribeExecuteHandle subscribe, float interval)
        {
            timer.Add(new Timer(subscribe, interval));
        }

        public void RemoveCallback(ISubscribeExecuteHandle subscribe)
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

        struct Timer : ISubscribeExecuteHandle
        {
            public float time;
            public ISubscribeExecuteHandle subscribe;
            public object result { get; }

            public Timer(ISubscribeExecuteHandle subscribe, float time)
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

            public IEnumerator ExecuteComplete(float timeout = 0)
            {
                yield return subscribe.ExecuteComplete(timeout);
            }
        }
    }
}