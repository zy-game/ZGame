using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Resource;

namespace ZGame.Window
{
    public sealed class UIManager : Singleton<UIManager>
    {
        private Dictionary<Type, UIBase> _windows = new Dictionary<Type, UIBase>();


        protected void OnAwake()
        {
            
        }

        public UIBase Open(Type type)
        {
            if (_windows.TryGetValue(type, out UIBase gameWindow))
            {
                return gameWindow;
            }

            ResourceReference reference = type.GetCustomAttribute<ResourceReference>();
            if (reference is null || reference.path.IsNullOrEmpty())
            {
                Debug.LogError("没找到资源引用");
                return default;
            }

            ResHandle resObject = ResourceManager.instance.LoadAsset(reference.path);
            if (resObject is null)
            {
                return default;
            }

            Debug.Log("加载UI：" + type.Name);
            gameWindow = (UIBase)Activator.CreateInstance(type, new object[] { resObject.Instantiate() });
            LayerManager.instance.TrySetup(gameWindow.gameObject, reference.layer, Vector3.zero, Vector3.zero, Vector3.one);
            gameWindow.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            _windows.Add(type, gameWindow);
            gameWindow.Awake();
            Active(type);
            return gameWindow;
        }

        public UIBase GetWindow(Type type)
        {
            if (_windows.TryGetValue(type, out UIBase gameWindow))
            {
                return gameWindow;
            }

            return default;
        }

        public bool HasShow(Type type)
        {
            return _windows.ContainsKey(type);
        }

        public void Active(Type type)
        {
            if (_windows.TryGetValue(type, out UIBase gameWindow))
            {
                gameWindow.gameObject.SetActive(true);
                gameWindow.Enable();
            }
        }

        public void Inactive(Type type)
        {
            if (_windows.TryGetValue(type, out UIBase gameWindow))
            {
                gameWindow.gameObject.SetActive(false);
                gameWindow.Disable();
            }
        }

        public void Close(Type type)
        {
            if (_windows.TryGetValue(type, out UIBase gameWindow))
            {
                gameWindow.Disable();
                gameWindow.Dispose();
                GameObject.DestroyImmediate(gameWindow.gameObject);
                _windows.Remove(type);
            }
        }

        public void Dispose()
        {
            foreach (var VARIABLE in _windows.Values)
            {
                VARIABLE.Disable();
                VARIABLE.Dispose();
                GameObject.DestroyImmediate(VARIABLE.gameObject);
            }

            _windows.Clear();
        }
    }
}