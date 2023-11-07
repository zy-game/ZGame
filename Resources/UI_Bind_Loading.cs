using ZGame.Window;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Createing with 2023/11/7 17:49
/// by DESKTOP-N4DE5SO
/// </summary>
namespace Test
{
    public abstract class UI_Bind_Loading : GameWindow
    {
        public UIBind<Slider> Slider_Slider;
        public UIBind<RectTransform> RectTransform_Slider;
        public UIBind<RectTransform> RectTransform_Background;
        public UIBind<CanvasRenderer> CanvasRenderer_Background;
        public UIBind<Image> Image_Background;
        public UIBind<RectTransform> RectTransform_TextTMP;
        public UIBind<TextMeshProUGUI> TextMeshProUGUI_TextTMP;


        public override void Awake()
        {
            OnBind();
            OnEventRegister();
            OnRefresh();
        }

        private void OnBind()
        {
            if (this.gameObject == null)
            {
                return;
            }

            Slider_Slider = new UIBind<Slider>(this.gameObject.transform.Find("Panel/Slider"));
            RectTransform_Slider = new UIBind<RectTransform>(this.gameObject.transform.Find("Panel/Slider"));
            RectTransform_Background = new UIBind<RectTransform>(this.gameObject.transform.Find("Panel/Slider/Background"));
            CanvasRenderer_Background = new UIBind<CanvasRenderer>(this.gameObject.transform.Find("Panel/Slider/Background"));
            Image_Background = new UIBind<Image>(this.gameObject.transform.Find("Panel/Slider/Background"));
            RectTransform_TextTMP = new UIBind<RectTransform>(this.gameObject.transform.Find("Panel/Text (TMP)"));
            TextMeshProUGUI_TextTMP = new UIBind<TextMeshProUGUI>(this.gameObject.transform.Find("Panel/Text (TMP)"));
        }

        private void OnEventRegister()
        {
            Slider_Slider?.SetCallback(new Action<float>(on_invoke_ValueChangeEvent_Slider_Slider));
        }

        protected virtual void on_invoke_ValueChangeEvent_Slider_Slider(float value)
        {
        }

        public void OnRefresh()
        {
        }
    }
}