using System.Collections;
using TMPro;
using UnityEngine;
using ZGame;
using ZGame.Window;

namespace UI
{
    public sealed class UIWait : UIBase
    {
        private Coroutine _coroutine;

        public UIWait(GameObject gameObject) : base(gameObject)
        {
        }


        public override void Enable(params object[] args)
        {
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("content") is false)
                {
                    continue;
                }

                VARIABLE.SetText(args[0].ToString());
            }

            float time = (float)args[1];
            if (time <= 0)
            {
                return;
            }

            _coroutine = UIManager.instance.StartCoroutine(CheckTimeout(time));
        }

        private IEnumerator CheckTimeout(float time)
        {
            yield return new WaitForSeconds(time);
            UIManager.instance.Close<UIWait>();
        }

        public override void Dispose()
        {
            if (_coroutine == null)
            {
                return;
            }

            UIManager.instance.StopCoroutine(_coroutine);
        }


        public static void Show(string s, float timeout = 0)
        {
            string resPath = $"Resources/{BasicConfig.instance.curEntry.entryName}/Waiting";
            UIManager.instance.Open<UIWait>(resPath, s, timeout);
        }

        public static void Hide()
        {
            UIManager.instance.Close<UIWait>();
        }
    }
}