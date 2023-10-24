using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUITextBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<object>
    {
        public static IUITextBindPipeline Create(UIWindow window, string path)
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

            TextBindPipelineHandle textBindPipelineHandle = new TextBindPipelineHandle();
            textBindPipelineHandle.window = window;
            textBindPipelineHandle.path = path;
            textBindPipelineHandle.name = gameObject.name;
            textBindPipelineHandle.gameObject = gameObject;
            textBindPipelineHandle.text = tmpText;
            textBindPipelineHandle.Active();
            return textBindPipelineHandle;
        }

        public class TextBindPipelineHandle : IUITextBindPipeline
        {
            public TMP_Text text { get; set; }
            public string path { get; set; }
            public string name { get; set; }
            public bool actived { get; set; }
            public object value { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public event Action<object> callback;

            public void SetValue(object value)
            {
                SetValueWithoutNotify(value);
                Invoke(this);
            }

            public void Enable()
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.SetActive(true);
                Active();
            }

            public void Disable()
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.SetActive(false);
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

                this.value = value;
                text.text = value.ToString();
            }

            public void Dispose()
            {
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
                this.gameObject = null;
                this.value = default;
                this.text = null;
                this.actived = false;
            }

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void AddListener(Action<object> callback)
            {
                this.callback += callback;
            }

            public void RemoveListener(Action<object> callback)
            {
                this.callback -= callback;
            }

            public void Invoke(object args)
            {
                if (actived is false)
                {
                    return;
                }

                this.callback?.Invoke(args);
            }
        }
    }
}