using System;
using UnityEngine;

namespace ZGame.Window
{
    [ResourceReference("Resources/Prefabs/Loading", 100)]
    public class Loading : UI_Loading, IProgress<float>
    {
        public Loading(GameObject gameObject) : base(gameObject)
        {
        }

        public Loading SetupTitle(string title)
        {
            this.TextMeshProUGUI_TextTMP.Setup(title);
            return this;
        }

        public void SetupProgress(float progress)
        {
            this.Slider_Slider.Setup(progress);
        }

        public void Report(float value)
        {
            SetupProgress(value);
        }
    }
}