using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZGame.Window
{
    /// <summary>
    /// UI组件绑定
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    public class UIBind<T> : IDisposable where T : Component
    {
        public string name { get; private set; }
        public Transform transform { get; private set; }
        public GameObject gameObject { get; private set; }
        public T Component { get; private set; }
        private Action<object> _setupData;
        private Action<object> _onComponentValueChange;

        public UIBind(Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            this.name = transform.name;
            this.transform = transform;
            this.gameObject = transform.gameObject;
            this.Component = gameObject.GetComponent<T>();
            OnInitComponentEventCallback();
        }


        private void OnInitComponentEventCallback()
        {
            switch (Component)
            {
                case TextMeshProUGUI text:
                    _setupData = args => text.text = (string)args;
                    break;
                case RawImage rawImage:
                    _setupData = args => rawImage.texture = (Texture)args;
                    break;
                case Image image:
                    _setupData = args => image.sprite = (Sprite)args;
                    break;
                case Slider slider:
                    slider.onValueChanged.RemoveAllListeners();
                    slider.onValueChanged.AddListener(OnEvent);
                    _setupData = args => slider.SetValueWithoutNotify((float)args);
                    break;
                case TMP_InputField inputField:
                    inputField.onEndEdit.RemoveAllListeners();
                    inputField.onEndEdit.AddListener(OnEvent);
                    _setupData = args => inputField.SetTextWithoutNotify((string)args);
                    break;
                case Toggle toggle:
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(OnEvent);
                    _setupData = args => toggle.SetIsOnWithoutNotify((bool)args);
                    break;
                case Button button:
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => { OnEvent(default(T)); });
                    break;
                case TMP_Dropdown dropdown:
                    dropdown.onValueChanged.RemoveAllListeners();
                    dropdown.onValueChanged.AddListener(OnEvent);
                    _setupData = args => dropdown.SetValueWithoutNotify((int)args);
                    break;
                case ScrollRect scrollRect:
                    scrollRect.onValueChanged.RemoveAllListeners();
                    scrollRect.onValueChanged.AddListener(OnEvent);
                    break;
                case Scrollbar scrollbar:
                    scrollbar.onValueChanged.RemoveAllListeners();
                    scrollbar.onValueChanged.AddListener(OnEvent);
                    break;
            }
        }

        private void OnEvent<T2>(T2 args)
        {
            this._onComponentValueChange?.Invoke(args);
        }

        public void Setup(UnityAction action)
        {
            _onComponentValueChange = args => action();
        }

        public void Setup<T2>(UnityAction<T2> callback)
        {
            _onComponentValueChange = args => callback((T2)args);
        }

        public void Setup(object args)
        {
            _setupData?.Invoke(args);
        }

        public virtual void Dispose()
        {
            this.transform = null;
            name = String.Empty;
        }
    }
}