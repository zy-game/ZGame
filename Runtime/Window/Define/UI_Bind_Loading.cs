using ZGame.Window;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Createing with 2023/11/15 12:04
/// by DESKTOP-N4DE5SO
/// </summary>
namespace ZGame.Window
{
	public class UI_Bind_Loading : UIBase
	{
		public UIBind<RectTransform> RectTransform_Slider;
		public UIBind<Slider> Slider_Slider;
		public UIBind<RectTransform> RectTransform_Background;
		public UIBind<CanvasRenderer> CanvasRenderer_Background;
		public UIBind<Image> Image_Background;
		public UIBind<RectTransform> RectTransform_TextTMP;
		public UIBind<CanvasRenderer> CanvasRenderer_TextTMP;
		public UIBind<TextMeshProUGUI> TextMeshProUGUI_TextTMP;

		public UI_Bind_Loading(GameObject gameObject) : base(gameObject)
		{
		}

		public override void Awake()
		{
			OnBind();
			OnEventRegister();
		}

		protected virtual void OnBind()
		{
			if(this.gameObject == null)
			{
				return;
			}
			RectTransform_Slider = new UIBind<RectTransform>(this.gameObject.transform.Find("Panel/Slider"));
			Slider_Slider = new UIBind<Slider>(this.gameObject.transform.Find("Panel/Slider"));
			RectTransform_Background = new UIBind<RectTransform>(this.gameObject.transform.Find("Panel/Slider/Background"));
			CanvasRenderer_Background = new UIBind<CanvasRenderer>(this.gameObject.transform.Find("Panel/Slider/Background"));
			Image_Background = new UIBind<Image>(this.gameObject.transform.Find("Panel/Slider/Background"));
			RectTransform_TextTMP = new UIBind<RectTransform>(this.gameObject.transform.Find("Panel/Text (TMP)"));
			CanvasRenderer_TextTMP = new UIBind<CanvasRenderer>(this.gameObject.transform.Find("Panel/Text (TMP)"));
			TextMeshProUGUI_TextTMP = new UIBind<TextMeshProUGUI>(this.gameObject.transform.Find("Panel/Text (TMP)"));
		}

		protected virtual void OnEventRegister()
		{
			Slider_Slider?.Setup(new Action<float>(on_invoke_ValueChangeEvent_Slider_Slider));
		}

		protected virtual void on_invoke_ValueChangeEvent_Slider_Slider(float value)
		{
		}

		public override void Dispose()
		{
			base.Dispose();
			RectTransform_Slider?.Dispose();
			Slider_Slider?.Dispose();
			RectTransform_Background?.Dispose();
			CanvasRenderer_Background?.Dispose();
			Image_Background?.Dispose();
			RectTransform_TextTMP?.Dispose();
			CanvasRenderer_TextTMP?.Dispose();
			TextMeshProUGUI_TextTMP?.Dispose();
		}
	}
}
