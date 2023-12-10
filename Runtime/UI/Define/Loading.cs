using System;
using UnityEngine;

namespace ZGame.Window
{
    [ResourceReference("Resources/Prefabs/Loading", 100)]
    public class Loading : UI_Loading, ILoadingHandle
    {
        public Loading(GameObject gameObject) : base(gameObject)
        {
        }

        public void SetTitle(string title)
        {
            this.TextMeshProUGUI_TextTMP.Setup(title);
        }

        public void Report(float value)
        {
            this.Slider_Slider.Setup(value);
        }
    }

    public interface ILoadingHandle : IDisposable, IProgress<float>
    {
        void SetTitle(string title);
    }
}