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
        private bool complete;
        public object result { get; set; }

        public MsgBox SetBox(string tips, string text, Action ok, Action cancel, string okText, string cancelText)
        {
            this.ok = ok;
            complete = false;
            this.cancel = cancel;
            this.GetChild("Panel/Image/text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Panel/Image/Tips").GetComponent<TMP_Text>().text = tips;
            this.GetChild("Panel/Image/GameObject/ok").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("Panel/Image/GameObject/cancel").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("Panel/Image/GameObject/ok").GetComponent<Button>().onClick.AddListener(OK);
            this.GetChild("Panel/Image/GameObject/cancel").GetComponent<Button>().onClick.AddListener(Cancel);
            this.GetChild("Panel/Image/GameObject/cancel").SetActive(cancel is not null);
            this.GetChild("Panel/Image/GameObject/ok/okText").GetComponent<TMP_Text>().text = okText;
            this.GetChild("Panel/Image/GameObject/cancel/cancelText").GetComponent<TMP_Text>().text = cancelText;
            return this;
        }

        public UniTask<bool> SetBox(string tips, string text, string okText, string cancelText)
        {
            UniTaskCompletionSource<bool> uniTaskCompletionSource = new UniTaskCompletionSource<bool>();
            this.ok = () => uniTaskCompletionSource.TrySetResult(true);
            complete = false;
            this.cancel = () => uniTaskCompletionSource.TrySetResult(false);
            this.GetChild("Panel/Image/text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Panel/Image/Tips").GetComponent<TMP_Text>().text = tips;
            this.GetChild("Panel/Image/GameObject/ok").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("Panel/Image/GameObject/cancel").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("Panel/Image/GameObject/ok").GetComponent<Button>().onClick.AddListener(OK);
            this.GetChild("Panel/Image/GameObject/cancel").GetComponent<Button>().onClick.AddListener(Cancel);
            this.GetChild("Panel/Image/GameObject/cancel").SetActive(cancel is not null);
            this.GetChild("Panel/Image/GameObject/ok/okText").GetComponent<TMP_Text>().text = okText;
            this.GetChild("Panel/Image/GameObject/cancel/cancelText").GetComponent<TMP_Text>().text = cancelText;
            return uniTaskCompletionSource.Task;
        }

        private void OK()
        {
            complete = true;
            ok?.Invoke();
            result = true;
            ZGame.Window.Close<MsgBox>();
        }

        private void Cancel()
        {
            complete = true;
            cancel?.Invoke();
            result = false;
            ZGame.Window.Close<MsgBox>();
        }
    }
}