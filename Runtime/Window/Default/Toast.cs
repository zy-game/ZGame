using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZEngine.Utility;

namespace ZEngine.Window
{
    [UIOptions("Resources/Toast", UIOptions.Layer.Top)]
    public class Toast : UIWindow
    {
        public Toast SetToast(string text)
        {
            Launche.Console.Log(Screen.height);
            GameObject handle = this.GetChild("GameObject");
            handle.transform.localScale = Vector3.zero;
            handle.transform.localPosition = new Vector3(Screen.width / 2, 0, 0);
            handle.transform.DOLocalMove(new Vector3(Screen.width / 2, (Screen.height / 4), 0), 0.3f).SetEase(Ease.OutBack);
            handle.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            this.GetChild("Text (TMP)").GetComponent<TMP_Text>().text = text;
            this.GetChild("Text (TMP)").GetComponent<AutoSize>().Refersh();
            WaitFor.Create(3f, () => Launche.Window.Close<Toast>());
            return this;
        }
    }
}