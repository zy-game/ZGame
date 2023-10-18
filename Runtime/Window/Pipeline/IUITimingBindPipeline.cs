using System;
using UnityEngine;

namespace ZEngine.Window
{
    /// <summary>
    /// 计时器组件
    /// </summary>
    public interface IUITimingBindPipeline : IUIComponentBindPipeline
    {
        float progres { get; }
        float time { get; }
        float interval { get; }
        void Reset();

        public static IUITimingBindPipeline Create(UIWindow window, float count, float interval, Action<IUITimingBindPipeline> callback)
        {
            return new InternalCountdownPipelineHandle()
            {
                time = count,
                interval = interval,
                window = window,
                callback = callback,
                progres = 0
            };
        }

        class InternalCountdownPipelineHandle : IUITimingBindPipeline
        {
            public float time { get; set; }
            public float progres { get; set; }
            public float interval { get; set; }
            public UIWindow window { get; set; }
            public Action<IUITimingBindPipeline> callback;
            public string path { get; }
            public string name { get; }
            public object value { get; }
            public GameObject gameObject { get; }

            public void Active()
            {
            }

            public void Inactive()
            {
            }

            public void OnChange(object args)
            {
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}