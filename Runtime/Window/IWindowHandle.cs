using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ZEngine.Window
{
    public interface ITimwout
    {
        ITimwout Timeout(float time);
    }


    public abstract class UIWindow : IReference
    {
        private Dictionary<string, GameObject> childList = new Dictionary<string, GameObject>();
        public GameObject gameObject { get; private set; }

        internal void SetGameObject(GameObject value)
        {
            this.gameObject = value;
            foreach (var VARIABLE in this.gameObject.GetComponentsInChildren<RectTransform>(true))
            {
                if (childList.ContainsKey(VARIABLE.name))
                {
                    continue;
                }

                childList.Add(VARIABLE.name, VARIABLE.gameObject);
            }
        }

        public virtual void OnAwake()
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDiable()
        {
        }

        public GameObject GetChild(string name)
        {
            if (childList.TryGetValue(name, out GameObject gameObject))
            {
                return gameObject;
            }

            return default;
        }

        public void Release()
        {
        }
    }

    [UIOptions("Resources/Toast", UIOptions.Layer.Pop)]
    public class UI_Toast : UIWindow
    {
        public UI_Toast SetToast(string text)
        {
            return this;
        }
    }

    [UIOptions("Resources/Wait", UIOptions.Layer.Pop)]
    public class UI_Wait : UIWindow, ITimwout
    {
        public ITimwout Timeout(float time)
        {
            return this;
        }

        public UI_Wait SetWait(string text, float timeout)
        {
            return this;
        }
    }

    [UIOptions("Resources/Msgbox", UIOptions.Layer.Pop)]
    public class UI_MsgBox : UIWindow
    {
        public UI_MsgBox SetBox(string tips, string text, Action ok, Action cancel)
        {
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Tips").GetComponent<TMP_Text>().text = tips;
            return this;
        }
    }

    [UIOptions("Resources/Loading", UIOptions.Layer.Pop)]
    public class UI_Loading : UIWindow
    {
    }
}