using System.Collections;
using TMPro;
using UnityEngine;
using ZGame;
using ZGame.Window;

namespace UI
{
    public sealed class UITips : UIBase
    {
        public UITips(GameObject gameObject) : base(gameObject)
        {
        }

        public override void Enable(params object[] args)
        {
            string content = args[0].ToString();
            float timeout = (float)args[1];
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("content") is false)
                {
                    continue;
                }

                VARIABLE.SetText(content);
            }

            UIManager.instance.StartCoroutine(CheckTimeout(timeout));
        }

        private IEnumerator CheckTimeout(float time)
        {
            yield return new WaitForSeconds(time);
            UIManager.instance.Close<UITips>();
        }
        

        public static void Show(string content, float timeout = 5)
        {
            string resPath = $"Resources/{BasicConfig.instance.curEntry.entryName}/Tips";
            UIManager.instance.Open<UITips>(resPath, content);
        }
    }
}