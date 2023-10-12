using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public abstract class UIWindow : IDisposable
    {
        private Dictionary<string, GameObject> childList = new Dictionary<string, GameObject>();
        private Dictionary<string, IUIBindPipeline> bindPipelines = new Dictionary<string, IUIBindPipeline>();
        public GameObject gameObject { get; private set; }

        internal void SetGameObject(GameObject value, IUIWindowOptions windowOptions)
        {
            this.gameObject = value;
            if (windowOptions is null)
            {
                return;
            }

            windowOptions.Initialize(this);
        }

        internal void SetBindPipeline(string path, IUIBindPipeline pipeline)
        {
            if (bindPipelines.ContainsKey(path))
            {
                return;
            }

            bindPipelines.Add(path, pipeline);
        }

        public virtual void OnEvent(string eventName, params object[] args)
        {
        }

        public virtual void OnNotifyChanged(string path, object args)
        {
            if (bindPipelines.TryGetValue(path, out IUIBindPipeline bindPipeline) is false)
            {
                return;
            }

            bindPipeline.OnChangeValue(args);
        }

        public void OnClick(string name, UnityAction callback)
        {
            GameObject temp = this.GetChild(name);
            if (temp == null)
            {
                return;
            }

            Button btn = temp.GetComponent<Button>();
            if (btn == null)
            {
                return;
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(callback);
        }

        public void OnClick<T>(string name, Action<T> callback, T args)
        {
            OnClick(name, () => callback(args));
        }

        public void OnInputFiled(string name, UnityAction<string> callback)
        {
            GameObject temp = this.GetChild(name);
            if (temp == null)
            {
                return;
            }

            TMP_InputField inputField = temp.GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                InputField m = temp.GetComponent<InputField>();
                if (m != null)
                {
                    m.onEndEdit.RemoveAllListeners();
                    m.onEndEdit.AddListener(callback);
                }

                return;
            }

            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener(callback);
        }

        public void OnSlider(string name, UnityAction<float> callback)
        {
            GameObject temp = this.GetChild(name);
            if (temp == null)
            {
                return;
            }

            Slider slider = temp.GetComponent<Slider>();
            if (slider == null)
            {
                return;
            }

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(callback);
        }

        public void OnToggle(string name, UnityAction<bool> callback)
        {
            GameObject temp = this.GetChild(name);
            if (temp == null)
            {
                return;
            }

            Toggle toggle = temp.GetComponent<Toggle>();
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(callback);
        }

        public void OnDropdown(string name, UnityAction<int> callback, params TMP_Dropdown.OptionData[] paramsList)
        {
            GameObject temp = this.GetChild(name);
            if (temp == null)
            {
                return;
            }

            TMP_Dropdown dropdown = temp.GetComponent<TMP_Dropdown>();
            if (dropdown == null)
            {
                return;
            }

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(callback);
        }


        public void OnAwake()
        {
            Awake();
        }

        public void OnEnable()
        {
            gameObject.SetActive(true);
            Enable();
        }

        public void OnDiable()
        {
            gameObject.SetActive(false);
            Disable();
        }

        public GameObject GetChild(string name)
        {
            if (childList.TryGetValue(name, out GameObject gameObject))
            {
                return gameObject;
            }

            Transform transform = gameObject.transform.Find(name);
            if (transform != null)
            {
                childList.Add(name, transform.gameObject);
            }

            return transform.gameObject;
        }

        public void Dispose()
        {
            Destroy();
            GameObject.DestroyImmediate(gameObject);
            childList.Clear();
            GC.SuppressFinalize(this);
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Enable()
        {
        }

        protected virtual void Disable()
        {
        }

        protected virtual void Destroy()
        {
        }
    }
}