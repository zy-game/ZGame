using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUISliderBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<float>, IEventBindPipeline<IUISliderBindPipeline>
    {
        public static IUISliderBindPipeline Create(UIWindow window, string path)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            Slider slider = gameObject.GetComponent<Slider>();
            if (slider == null)
            {
                return default;
            }

            SliderBindPipelineHandle sliderBindPipelineHandle = new SliderBindPipelineHandle();
            sliderBindPipelineHandle.window = window;
            sliderBindPipelineHandle.path = path;
            sliderBindPipelineHandle.name = gameObject.name;
            sliderBindPipelineHandle.gameObject = gameObject;
            sliderBindPipelineHandle.slider = slider;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(args => sliderBindPipelineHandle.SetValue(args));
            sliderBindPipelineHandle.Active();
            return sliderBindPipelineHandle;
        }

        public static IUISliderBindPipeline Create(UIWindow window, string path, float value, Action<IUISliderBindPipeline, object> callback)
        {
            IUISliderBindPipeline sliderBindPipeline = Create(window, path);
            sliderBindPipeline.SetValueWithoutNotify(value);
            sliderBindPipeline.AddListener(callback);
            return sliderBindPipeline;
        }

        class SliderBindPipelineHandle : IUISliderBindPipeline
        {
            public string path { get; set; }
            public string name { get; set; }
            public bool actived { get; set; }
            public float value { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public Slider slider { get; set; }
            public event Action<IUISliderBindPipeline, object> Callback;

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void SetValue(float value)
            {
                SetValueWithoutNotify(value);
                Invoke(value);
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

                if (slider == null)
                {
                    return;
                }

                this.value = (float)value;
                slider.SetValueWithoutNotify(this.value);
            }

            public void Dispose()
            {
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
                this.gameObject = null;
                this.Callback = null;
                this.value = 0f;
                this.slider = null;
                this.actived = false;
            }

            public void AddListener(Action<IUISliderBindPipeline, object> callback)
            {
                this.Callback += callback;
            }

            public void RemoveListener(Action<IUISliderBindPipeline, object> callback)
            {
                this.Callback -= callback;
            }

            public void Invoke(object args)
            {
                if (this.actived is false)
                {
                    return;
                }

                this.Callback?.Invoke(this, args);
            }
        }
    }
}