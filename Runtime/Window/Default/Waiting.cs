using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ZEngine.Window
{
    [UIOptions("Resources/Wait", UIOptions.Layer.Top)]
    public class Waiting : UIWindow
    {
        private Action subsceibe;

        protected override void Destroy()
        {
            base.Destroy();
        }

        public void Timeout(float time)
        {
            if (time == 0)
            {
                return;
            }

            IEnumerator Waiting()
            {
                yield return WaitFor.Create(time);
                subsceibe?.Invoke();
                Launche.Window.Close<Waiting>();
            }

            Waiting().StartCoroutine();
        }

        public Waiting SetWait(string text, float timeout, Action subscribe)
        {
            this.GetChild("text").GetComponent<TMP_Text>().text = text;
            this.subsceibe = subscribe;
            this.GetChild("Image").transform
                .DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(int.MaxValue, LoopType.Restart);
            Timeout(timeout);
            return this;
        }
    }
}