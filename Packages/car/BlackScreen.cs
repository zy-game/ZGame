using UnityEngine;

[ExecuteInEditMode]
public class BlackScreen : MonoBehaviour
{
	private Texture2D CameraFade;

	private Rect drawRect;

	private Color curColor = Color.black;

	[Range(0f, 1f)]
	public float Alpha;

	public void Awake()
	{
		drawRect.Set(0f, 0f, Screen.width * 2, Screen.height * 2);
		curColor.a = 0f;
		CameraFade = new Texture2D(1, 1);
		CameraFade.SetPixel(0, 0, curColor);
		CameraFade.Apply();
	}

	public void Update()
	{
		if (curColor.a != Alpha)
		{
			curColor.a = Alpha;
			CameraFade.SetPixel(0, 0, curColor);
			CameraFade.Apply();
		}
	}

	public void OnGUI()
	{
		GUI.depth = 9999;
		GUI.DrawTexture(drawRect, CameraFade);
	}
}
