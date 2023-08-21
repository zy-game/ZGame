using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Loading", UIOptions.Layer.Low)]
    public class Loading : UIWindow
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

        public IProgressSubscribeHandle GetProgressSubscribe()
        {
            return IProgressSubscribeHandle.Create(args => SetProgress(args));
        }

        public Loading SetProgress(float progress)
        {
            if (slider == null)
            {
                return this;
            }

            slider.value = progress;
            return this;
        }
    }
}