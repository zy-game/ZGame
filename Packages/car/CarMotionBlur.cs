using UnityEngine;

[AddComponentMenu("Image Effects/Motion Blur")]
public class CarMotionBlur : ImageEffectBase
{
	public float blurAmount = 0.8f;

	public Texture BlurTexture;

	public bool extraBlur;

	private RenderTexture accumTexture;

	public new void Start()
	{
		base.Start();
		if (!base.enabled)
		{
			Object.Destroy(this);
		}
	}

	public new void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			Object.Destroy(accumTexture);
		}
		else
		{
			Object.DestroyImmediate(accumTexture);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (accumTexture == null || accumTexture.width != source.width || accumTexture.height != source.height)
		{
			Object.DestroyImmediate(accumTexture);
			accumTexture = new RenderTexture(source.width, source.height, 0);
			accumTexture.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable;
			ImageEffects.Blit(source, accumTexture);
		}
		if (extraBlur)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
			ImageEffects.Blit(accumTexture, temporary);
			ImageEffects.Blit(temporary, accumTexture);
			RenderTexture.ReleaseTemporary(temporary);
		}
		blurAmount = Mathf.Clamp(blurAmount, 0f, 0.92f);
		base.material.SetTexture("_MainTex", accumTexture);
		base.material.SetTexture("_BlurTex", BlurTexture);
		base.material.SetFloat("_AccumOrig", blurAmount);
		Graphics.Blit(source, accumTexture, base.material);
		Graphics.Blit(accumTexture, destination);
	}
}
