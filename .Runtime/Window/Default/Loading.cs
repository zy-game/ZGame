using TMPro;
using UnityEngine.UI;

namespace ZEngine.Window
{
    [UIOptions("Resources/Loading", UIOptions.Layer.Low)]
    public class Loading : UIWindow, IProgressHandle
    {
        // private Slider slider;
        private IUITextBindPipeline textBindPipeline;
        private IUISliderBindPipeline sliderBindPipeline;

        public override void Awake()
        {
            textBindPipeline = this.BindText("Panel/Text (TMP)");
            sliderBindPipeline = this.BindSlider("Panel/Slider", 0);
        }

        public IProgressHandle SetInfo(string text)
        {
            textBindPipeline.SetValue(text);
            return this;
        }

        public IProgressHandle SetProgress(float progress)
        {
            if (sliderBindPipeline is not null)
            {
                sliderBindPipeline.SetValue(progress);
            }

            return this;
        }
    }
}