using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public abstract class GameWindow : IDisposable
    {
        public string name { get; }
        public GameObject gameObject { get; internal set; }


        public virtual void Awake()
        {
        }

        public virtual void Enable()
        {
        }

        public virtual void Disable()
        {
        }

        public virtual void Dispose()
        {
        }
    }

    public abstract class TestUIInitlizedPipiline : GameWindow
    {
        public UIBind<Text> label_name_title { get; private set; }
        public UIBind<Button> btn_switch;

        public override void Awake()
        {
            base.Awake();
            OnBindUIComponent();
            OnRefreshLabelText();
            OnEventRegister();
        }

        private void OnBindUIComponent()
        {
            label_name_title = new UIBind<Text>(this.gameObject.transform.Find("xxx"));
            btn_switch = new UIBind<Button>(this.gameObject.transform.Find("xxx"));
        }

        private void OnRefreshLabelText()
        {
            label_name_title.Setup(Engine.Localization.GetLanguage(100000));
        }

        private void OnEventRegister()
        {
            btn_switch.SetCallback(new Action(on_invoke_SwitchClickEvent));
        }

        protected virtual void on_invoke_SwitchClickEvent()
        {
        }
    }

    public class TestUIHandle : TestUIInitlizedPipiline
    {
        public override void Awake()
        {
            base.Awake();
        }

        protected override void on_invoke_SwitchClickEvent()
        {
        }
    }
}