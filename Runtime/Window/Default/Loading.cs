using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Loading", UIOptions.Layer.Low)]
    public class Loading : UIWindow, IProgressHandle
    {
        private Slider slider;

        public override void Awake()
        {
            slider = this.GetChild("Panel/Slider").GetComponent<Slider>();
        }

        public Loading SetInfo(string info)
        {
            GetChild("Panel/Text (TMP)").GetComponent<TMP_Text>().text = info;
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
    }
}