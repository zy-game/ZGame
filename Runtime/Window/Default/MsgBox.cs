using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Msgbox", UIOptions.Layer.Top)]
    public class MsgBox : UIWindow
    {
        private Action ok;
        private Action cancel;
        private IUITextBindPipeline infoTextBindPipeline;
        private IUITextBindPipeline titleTextBindPipeline;
        private IUIButtonBindPipeline okButtonBindPipeline;
        private IUIButtonBindPipeline cancelButtonBindPipeline;
        private IUITextBindPipeline okButtonTextBindPipeline;
        private IUITextBindPipeline cancelButtonTextBindpipeline;

        public override void Awake()
        {
            infoTextBindPipeline = IUITextBindPipeline.Create(this, "Panel/Image/text");
            titleTextBindPipeline = IUITextBindPipeline.Create(this, "Panel/Image/Tips");
            okButtonBindPipeline = IUIButtonBindPipeline.Create(this, "Panel/Image/GameObject/ok");
            cancelButtonBindPipeline = IUIButtonBindPipeline.Create(this, "Panel/Image/GameObject/cancel");
            okButtonTextBindPipeline = IUITextBindPipeline.Create(this, "Panel/Image/GameObject/ok/okText");
            cancelButtonTextBindpipeline = IUITextBindPipeline.Create(this, "Panel/Image/GameObject/cancel/cancelText");
        }

        public MsgBox SetBox(string tips, string text, Action ok, Action cancel, string okText, string cancelText)
        {
            this.ok = ok;
            this.cancel = cancel;
            infoTextBindPipeline.SetValue(text);
            titleTextBindPipeline.SetValue(tips);
            okButtonTextBindPipeline.SetValue(okText);
            cancelButtonTextBindpipeline.SetValue(cancelText);
            okButtonBindPipeline.AddListener(OK);
            cancelButtonBindPipeline.AddListener(Cancel);
            if (cancel is null)
            {
                cancelButtonBindPipeline.Disable();
            }

            return this;
        }

        public UniTask<bool> SetBox(string tips, string text, string okText, string cancelText)
        {
            UniTaskCompletionSource<bool> uniTaskCompletionSource = new UniTaskCompletionSource<bool>();
            SetBox(tips, text, () => uniTaskCompletionSource.TrySetResult(true), () => uniTaskCompletionSource.TrySetResult(false), okText, cancelText);
            return uniTaskCompletionSource.Task;
        }

        private void OK(IUIButtonBindPipeline buttonBindPipeline)
        {
            ok?.Invoke();
            ZGame.Window.Close<MsgBox>();
        }

        private void Cancel(IUIButtonBindPipeline buttonBindPipeline)
        {
            cancel?.Invoke();
            ZGame.Window.Close<MsgBox>();
        }
    }
}