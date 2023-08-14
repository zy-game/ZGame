using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Msgbox", UIOptions.Layer.Pop)]
    public class UI_MsgBox : UIWindow, IAsyncWindow
    {
        private Action ok;
        private Action cancel;
        private bool complete;
        public object result { get; set; }

        public UI_MsgBox SetBox(string tips, string text, Action ok, Action cancel)
        {
            this.ok = ok;
            complete = false;
            this.cancel = cancel;
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Tips").GetComponent<TMP_Text>().text = tips;
            this.GetChild("ok").GetComponent<Button>().onClick.AddListener(OK);
            this.GetChild("cancel").GetComponent<Button>().onClick.AddListener(Cancel);
            // this.GetChild("ok").SetActive(ok is not null);
            this.GetChild("cancel").SetActive(cancel is not null);
            return this;
        }

        private void OK()
        {
            complete = true;
            ok?.Invoke();
            result = true;
            Engine.Window.Close<UI_MsgBox>();
        }

        private void Cancel()
        {
            complete = true;
            cancel?.Invoke();
            result = false;
            Engine.Window.Close<UI_MsgBox>();
        }


        public IEnumerator GetCoroutine()
        {
            yield return WaitFor.Create(() => complete is true);
        }
    }
}