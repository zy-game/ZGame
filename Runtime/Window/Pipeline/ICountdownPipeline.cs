using System;
using UnityEngine;

namespace ZEngine.Window
{
    public interface ICountdownPipeline : IUIBindPipeline
    {
        float progres { get; }
        float time { get; }
        float interval { get; }
        void Reset();

        public static ICountdownPipeline Create(UIWindow window, float count, float interval, Action<ICountdownPipeline> callback)
        {
            InternalCountdownPipelineHandle internalCountdownPipelineHandle = new InternalCountdownPipelineHandle();
            internalCountdownPipelineHandle.time = count;
            internalCountdownPipelineHandle.interval = interval;
            internalCountdownPipelineHandle.window = window;
            internalCountdownPipelineHandle.callback = callback;
            internalCountdownPipelineHandle.progres = 0;
            return internalCountdownPipelineHandle;
        }

        class InternalCountdownPipelineHandle : ICountdownPipeline
        {
            public float time { get; set; }
            public float progres { get; set; }
            public float interval { get; set; }
            public UIWindow window;
            public Action<ICountdownPipeline> callback;
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
                throw new NotImplementedException();
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