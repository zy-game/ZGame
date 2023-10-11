using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Loading", UIOptions.Layer.Low)]
    public class Loading : UIWindow, IGameProgressHandle
    {
        private Slider slider;

        protected override void Awake()
        {
            slider = this.GetChild("Slider").GetComponent<Slider>();
        }

        public Loading SetInfo(string info)
        {
            GetChild("Text (TMP)").GetComponent<TMP_Text>().text = info;
            return this;
        }


        public void SetTextInfo(string text)
        {
            SetInfo(text);
        }

        public void SetProgress(float progress)
        {
            if (slider == null)
            {
                return;
            }

            slider.value = progress;
        }

        public void SetTextAndProgress(string text, float progress)
        {
            SetTextInfo(text);
            SetProgress(progress);
        }
    }
}