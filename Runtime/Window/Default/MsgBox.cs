using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Msgbox", UIOptions.Layer.Top)]
    public class MsgBox : UIWindow, IAsyncWindow
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
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Tips").GetComponent<TMP_Text>().text = tips;
            this.GetChild("ok").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("cancel").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("ok").GetComponent<Button>().onClick.AddListener(OK);
            this.GetChild("cancel").GetComponent<Button>().onClick.AddListener(Cancel);
            this.GetChild("cancel").SetActive(cancel is not null);
            this.GetChild("okText").GetComponent<TMP_Text>().text = okText;
            this.GetChild("cancelText").GetComponent<TMP_Text>().text = cancelText;
            return this;
        }

        public UniTask<bool> SetBox(string tips, string text, string okText, string cancelText)
        {
            UniTaskCompletionSource<bool> uniTaskCompletionSource = new UniTaskCompletionSource<bool>();
            this.ok = () => uniTaskCompletionSource.TrySetResult(true);
            complete = false;
            this.cancel = () => uniTaskCompletionSource.TrySetResult(false);
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Tips").GetComponent<TMP_Text>().text = tips;
            this.GetChild("ok").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("cancel").GetComponent<Button>().onClick.RemoveAllListeners();
            this.GetChild("ok").GetComponent<Button>().onClick.AddListener(OK);
            this.GetChild("cancel").GetComponent<Button>().onClick.AddListener(Cancel);
            this.GetChild("cancel").SetActive(cancel is not null);
            this.GetChild("okText").GetComponent<TMP_Text>().text = okText;
            this.GetChild("cancelText").GetComponent<TMP_Text>().text = cancelText;
            return uniTaskCompletionSource.Task;
        }

        private void OK()
        {
            complete = true;
            ok?.Invoke();
            result = true;
            Launche.Window.Close<MsgBox>();
        }

        private void Cancel()
        {
            complete = true;
            cancel?.Invoke();
            result = false;
            Launche.Window.Close<MsgBox>();
        }


        public IEnumerator GetCoroutine()
        {
            yield return WaitFor.Create(() => complete is true);
        }
    }
}