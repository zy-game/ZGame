using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;

public static class Extension
{
    private static UniContent _content;

    public static int SpiltCount(this int lenght, int l)
    {
        int count = lenght / l;
        return count * l < lenght ? count + 1 : count;
    }

    public static Action Create(Action action)
    {
        return new Action(action);
    }


    public static Coroutine Startup(this IEnumerator enumerator)
    {
        EnsureContentInstance();
        return _content.StartCoroutine(enumerator);
    }
    
    public static void Stoping(this IEnumerator coroutine)
    {
        EnsureContentInstance();
        _content.StopCoroutine(coroutine);
    }

    public static void Stoping(this Coroutine coroutine)
    {
        EnsureContentInstance();
        _content.StopCoroutine(coroutine);
    }

    public static void Timer(this ISubscribe subscribe, float interval)
    {
        EnsureContentInstance();
        _content.AddTimer(subscribe, interval);
    }

    public static void Untimer(this ISubscribe subscribe)
    {
        EnsureContentInstance();
        _content.RemoveCallback(subscribe);
    }

    public static void BindToUpdate(this ISubscribe subscribe)
    {
        EnsureContentInstance();
        _content.AddUpdate(subscribe);
    }

    public static void BindToLateUpdate(this ISubscribe subscribe)
    {
        EnsureContentInstance();
        _content.AddLateUpdate(subscribe);
    }

    public static void BindToFixedUpdate(this ISubscribe subscribe)
    {
        EnsureContentInstance();
        _content.AddFixedUpdate(subscribe);
    }

    public static void Stoping(this ISubscribe subscribe)
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
        private List<ISubscribe> _update = new List<ISubscribe>();
        private List<ISubscribe> _lateUpdate = new List<ISubscribe>();
        private List<ISubscribe> _fixedUpdate = new List<ISubscribe>();
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

        private void Execute(List<ISubscribe> subscribes)
        {
            if (subscribes.Count is 0)
            {
                return;
            }

            for (int i = 0; i < subscribes.Count; i++)
            {
                subscribes[i].Execute();
            }
        }

        public void AddUpdate(ISubscribe subscribe)
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

                timer[i].Execute();
            }
        }

        public void AddFixedUpdate(ISubscribe subscribe)
        {
            _fixedUpdate.Add(subscribe);
        }

        public void AddLateUpdate(ISubscribe subscribe)
        {
            _lateUpdate.Add(subscribe);
        }

        public void AddTimer(ISubscribe subscribe, float interval)
        {
            timer.Add(new Timer(subscribe, interval));
        }

        public void RemoveCallback(ISubscribe subscribe)
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

        struct Timer : ISubscribe
        {
            public float time;
            public ISubscribe subscribe;

            public Timer(ISubscribe subscribe, float time)
            {
                this.time = Time.realtimeSinceStartup + time;
                this.subscribe = subscribe;
            }

            public void Release()
            {
                Engine.Class.Release(subscribe);
                subscribe = default;
                time = 0;
            }

            public void Execute(params object[] args)
            {
                this.time = Time.realtimeSinceStartup + time;
                subscribe.Execute(args);
            }
        }
    }
}