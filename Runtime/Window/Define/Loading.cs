using UnityEngine;

namespace ZGame.Window
{
    [ResourceReference("Resources/Prefabs/Loading", 100)]
    public class Loading : UI_Bind_Loading
    {
        public Loading(GameObject gameObject) : base(gameObject)
        {
        }

        public void SetupProgress(float progress)
        {
            this.Slider_Slider.Component.value = progress;
        }
    }
}