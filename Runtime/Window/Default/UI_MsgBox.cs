using System;
using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Msgbox", UIOptions.Layer.Pop)]
    public class UI_MsgBox : UIWindow
    {
        private Action ok;
        Action cancel;

        public UI_MsgBox SetBox(string tips, string text, Action ok, Action cancel)
        {
            this.ok = ok;
            this.cancel = cancel;
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.GetChild("Tips").GetComponent<TMP_Text>().text = tips;
            this.GetChild("ok").GetComponent<Button>().onClick.AddListener(OK);
            this.GetChild("cancel").GetComponent<Button>().onClick.AddListener(Cancel);
            this.GetChild("ok").SetActive(ok is not null);
            this.GetChild("cancel").SetActive(ok is not null);
            return this;
        }

        private void OK()
        {
            ok?.Invoke();
            Engine.Window.Close<UI_Wait>();
        }

        private void Cancel()
        {
            cancel?.Invoke();
            Engine.Window.Close<UI_Wait>();
        }
    }
}