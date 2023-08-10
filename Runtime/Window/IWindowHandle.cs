using System;
using UnityEngine;

namespace ZEngine.Window
{
    public interface ITimwout
    {
        ITimwout Timeout(float time);
    }


    public abstract class UIWindow : IReference
    {
        public GameObject gameObject { get; private set; }

        internal void SetGameObject(GameObject value)
        {
            this.gameObject = value;
            
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

        public UIWindow SetText(string child, string text)
        {
            return this;
        }

        public GameObject GetChild(string name)
        {
            throw new NotImplementedException();
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
            return this;
        }
    }

    [UIOptions("Resources/Loading", UIOptions.Layer.Pop)]
    public class UI_Loading : UIWindow
    {
    }
}