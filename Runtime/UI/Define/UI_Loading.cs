using ZGame.Window;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Createing with 2023/11/22 21:18
/// by DESKTOP-GSKJB7D
/// </summary>
namespace ZGame.Window
{
    public class UIBind_Loading : UIBase
    {
        public RectTransform RectTransform_Slider;
        public Slider Slider_Slider;
        public RectTransform RectTransform_Background;
        public CanvasRenderer CanvasRenderer_Background;
        public Image Image_Background;
        public RectTransform RectTransform_TextTMP;
        public CanvasRenderer CanvasRenderer_TextTMP;
        public TextMeshProUGUI TextMeshProUGUI_TextTMP;

        public UIBind_Loading(GameObject gameObject) : base(gameObject)
        {
        }

        public override void Awake()
        {
            OnBind();
            OnEventRegister();
        }

        protected virtual void OnBind()
        {
            if (this.gameObject == null)
            {
                return;
            }

            RectTransform_Slider = this.gameObject.transform.Find("Panel/Slider").GetComponent<RectTransform>();
            Slider_Slider = this.gameObject.transform.Find("Panel/Slider").GetComponent<Slider>();
            RectTransform_Background = this.gameObject.transform.Find("Panel/Slider/Background").GetComponent<RectTransform>();
            CanvasRenderer_Background = this.gameObject.transform.Find("Panel/Slider/Background").GetComponent<CanvasRenderer>();
            Image_Background = this.gameObject.transform.Find("Panel/Slider/Background").GetComponent<Image>();
            RectTransform_TextTMP = this.gameObject.transform.Find("Panel/Text (TMP)").GetComponent<RectTransform>();
            CanvasRenderer_TextTMP = this.gameObject.transform.Find("Panel/Text (TMP)").GetComponent<CanvasRenderer>();
            TextMeshProUGUI_TextTMP = this.gameObject.transform.Find("Panel/Text (TMP)").GetComponent<TextMeshProUGUI>();
        }

        protected virtual void OnEventRegister()
        {
            Slider_Slider?.onValueChanged.RemoveAllListeners();
            Slider_Slider?.onValueChanged.AddListener(on_handle_Slider_Slider);
        }

        protected virtual void on_handle_Slider_Slider(float value)
        {
        }

        public void on_setup_TextTMP(string text)
        {
            if (TextMeshProUGUI_TextTMP == null)
            {
                return;
            }

            TextMeshProUGUI_TextTMP.text = text;
        }

        public void on_setup_SliderValue(float value)
        {
            if (Slider_Slider == null)
            {
                return;
            }

            Slider_Slider.value = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            RectTransform_Slider = null;
            Slider_Slider = null;
            RectTransform_Background = null;
            CanvasRenderer_Background = null;
            Image_Background = null;
            RectTransform_TextTMP = null;
            CanvasRenderer_TextTMP = null;
            TextMeshProUGUI_TextTMP = null;
        }
    }
}