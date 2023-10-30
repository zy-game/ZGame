using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Resource;

namespace ZGame.Window
{
    public sealed class WindowManager : IManager
    {
        private Dictionary<int, ILayer> _layers = new Dictionary<int, ILayer>();
        private Dictionary<Type, GameWindow> _windows = new Dictionary<Type, GameWindow>();


        public GameWindow Open(Type type)
        {
            if (_windows.TryGetValue(type, out GameWindow gameWindow))
            {
                return gameWindow;
            }

            PrefabReference reference = type.GetCustomAttribute<PrefabReference>();
            if (reference is null || reference.path.IsNullOrEmpty())
            {
                Debug.LogError("没找到资源引用");
                return default;
            }

            AssetObjectHandle resObject = CoreApi.Resource.LoadAsset(reference.path);
            if (resObject is null)
            {
                return default;
            }

            gameWindow = (GameWindow)Activator.CreateInstance(type);
            gameWindow.gameObject = resObject.Instantiate();
            if (_layers.TryGetValue(reference.layer, out ILayer layer) is false)
            {
                _layers.Add(reference.layer, layer = ILayer.Create("UICanvas", Camera.main, reference.layer));
            }

            layer.Setup(gameWindow.gameObject, Vector3.zero, Vector3.zero, Vector3.one);
            gameWindow.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            gameWindow.Awake();
            gameWindow.Enable();
            return gameWindow;
        }

        public GameWindow GetWindow(Type type)
        {
            if (_windows.TryGetValue(type, out GameWindow gameWindow))
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
            if (_windows.TryGetValue(type, out GameWindow gameWindow))
            {
                gameWindow.gameObject.SetActive(true);
                gameWindow.Enable();
            }
        }

        public void Inactive(Type type)
        {
            if (_windows.TryGetValue(type, out GameWindow gameWindow))
            {
                gameWindow.gameObject.SetActive(false);
                gameWindow.Disable();
            }
        }

        public void Close(Type type)
        {
            if (_windows.TryGetValue(type, out GameWindow gameWindow))
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