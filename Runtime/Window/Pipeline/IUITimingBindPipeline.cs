using System;
using TMPro;
using UnityEngine;

namespace ZEngine.Window
{
    public enum TimingType : byte
    {
        None,
        Countdown,
        //后续再加吧
    }

    /// <summary>
    /// 计时器组件
    /// </summary>
    public interface IUITimingBindPipeline : IUIComponentBindPipeline
    {
        float progres { get; }
        float time { get; }
        float interval { get; }
        float current { get; }
        TimingType type { get; }
        void Restart();
        IUITimingBindPipeline SetInterval(float interval);
        IUITimingBindPipeline SetTimingType(TimingType type);

        public static IUITimingBindPipeline Create(UIWindow window, string path, float time, float interval, string text)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            TMP_Text tmpText = gameObject.GetComponent<TMP_Text>();
            if (tmpText == null)
            {
                return default;
            }

            InternalCountdownPipelineHandle internalCountdownPipelineHandle = new InternalCountdownPipelineHandle();
            internalCountdownPipelineHandle.window = window;
            internalCountdownPipelineHandle.path = path;
            internalCountdownPipelineHandle.name = gameObject.name;
            internalCountdownPipelineHandle.gameObject = gameObject;
            internalCountdownPipelineHandle.text = tmpText;
            internalCountdownPipelineHandle.time = time;
            internalCountdownPipelineHandle.interval = interval;
            internalCountdownPipelineHandle.Inactive();
            return internalCountdownPipelineHandle;
        }

        class InternalCountdownPipelineHandle : IUITimingBindPipeline
        {
            public string name { get; set; }
            public bool actived { get; set; }
            public string path { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public TMP_Text text { get; set; }


            public float progres { get; set; }
            public float time { get; set; }
            public float interval { get; set; }
            public float current { get; set; }
            public string defaultText { get; set; }
            public float internalTime { get; set; }
            public TimingType type { get; set; }
            private float lastTimingTime;

            public void Active()
            {
                this.actived = true;
                BehaviourSingleton.OnUpdate(Update);
            }

            public void Inactive()
            {
                this.actived = false;
                BehaviourSingleton.RemoveUpdate(Update);
            }


            public void Enable()
            {
                if (this.gameObject == null)
                {
                    return;
                }

                this.gameObject.SetActive(true);
                Active();
            }

            public void Disable()
            {
                if (this.gameObject == null)
                {
                    return;
                }

                this.gameObject.SetActive(false);
                Inactive();
            }

            public void SetValueWithoutNotify(object value)
            {
                if (gameObject == null)
                {
                    return;
                }

                if (text == null)
                {
                    return;
                }

                text.text = value.ToString();
            }

            private void Update()
            {
                if (actived is false || Time.realtimeSinceStartup - lastTimingTime < interval)
                {
                    return;
                }

                this.internalTime += this.interval;
                this.current = type == TimingType.Countdown ? this.time - this.internalTime : this.internalTime;
                if (this.current <= 0 || this.current >= time)
                {
                    SetValueWithoutNotify(this.defaultText);
                    return;
                }

                SetValueWithoutNotify(this.current);
            }

            public void Restart()
            {
                progres = 0;
                this.internalTime = 0;
                this.current = type == TimingType.Countdown ? this.time - this.internalTime : this.internalTime;
                SetValueWithoutNotify(this.current);
                Active();
            }

            public IUITimingBindPipeline SetInterval(float interval)
            {
                this.interval = interval;
                return this;
            }

            public IUITimingBindPipeline SetTimingType(TimingType type)
            {
                this.type = type;
                return this;
            }

            public void Dispose()
            {
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
                this.gameObject = null;
                this.time = 0;
                this.actived = false;
                this.interval = 0;
                this.current = 0;
                this.progres = 0;
                this.text = null;
                this.defaultText = String.Empty;
            }
        }
    }
}