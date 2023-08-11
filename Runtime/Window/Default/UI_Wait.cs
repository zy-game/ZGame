using System.Collections;
using TMPro;
using UnityEngine;

namespace ZEngine.Window
{
    [UIOptions("Resources/Wait", UIOptions.Layer.Pop)]
    public class UI_Wait : UIWindow
    {
        private ISubscribeHandle subsceibe;

        public void Wait(float time)
        {
            if (time == 0)
            {
                return;
            }

            IEnumerator Waiting()
            {
                yield return Timeout.Create(time);
                Engine.Window.Close<UI_Wait>();
            }

            Waiting().StartCoroutine();
        }

        public UI_Wait SetWait(string text, float timeout, ISubscribeHandle subscribe)
        {
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.subsceibe = subscribe;
            Wait(timeout);
            return this;
        }
    }
}