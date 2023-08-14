using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Loading", UIOptions.Layer.Pop)]
    public class UI_Loading : UIWindow
    {
        private Slider slider;

        protected override void Awake()
        {
            slider = this.GetChild("Slider").GetComponent<Slider>();
        }

        public UI_Loading SetInfo(string info)
        {
            GetChild("Text (TMP)").GetComponent<TMP_Text>().text = info;
            return this;
        }

        public ISubscribeHandle<float> GetProgressSubscribe()
        {
            return ISubscribeHandle.Create<float>(args => SetProgress(args));
        }

        public UI_Loading SetProgress(float progress)
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