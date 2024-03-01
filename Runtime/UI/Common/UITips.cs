using System.Collections;
using TMPro;
using UnityEngine;

namespace ZGame.UI
{
    [ResourceReference("Resources/Tips")]
    [UIOptions(UILayer.Notification, SceneType.Addition, CacheType.Permanent)]
    public sealed class UITips : UIBase
    {
        public UITips(GameObject gameObject) : base(gameObject)
        {
        }

        public override void Enable(params object[] args)
        {
            base.Enable(args);
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

            BehaviourScriptable.instance.StartCoroutine(CheckTimeout(timeout));
        }

        private IEnumerator CheckTimeout(float time)
        {
            yield return new WaitForSeconds(time);
            UIManager.instance.Inactive<UITips>();
        }


        public static void Show(string content, float timeout = 5)
        {
            UIManager.instance.Active<UITips>(new object[] { content, timeout });
        }
    }
}