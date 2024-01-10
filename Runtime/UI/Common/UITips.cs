using UnityEngine;
using ZGame.Window;

namespace UI
{
    public interface UITips : UIForm
    {
        void Setup(string content);

        public static void Show(string content)
        {
            UITips uiTips = UIManager.instance.Open<UITips>();
            if (uiTips is null)
            {
                Debug.Log("??");
                return;
            }

            uiTips.Setup(content);
        }
    }
}