using System;
using UnityEngine;

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
        public LabelComponent label_name_title;

        public override void Awake()
        {
            base.Awake();
            label_name_title = new LabelComponent(this, "");
        }
    }

    public class TestUIHandle : TestUIInitlizedPipiline
    {
        public override void Awake()
        {
            base.Awake();
        }
    }
}