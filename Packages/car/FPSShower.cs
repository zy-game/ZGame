using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FPSShower : MonoBehaviour
{
	public bool ShowFPS = true;

	public bool ShowDelay = true;

	public float updateInterval = 0.5f;

	public float offsetY = 30f;

	private float lastInterval;

	private int frames;

	private float fps;

	private StringBuilder bd = new StringBuilder();

	public static Dictionary<string, string> Messages = new Dictionary<string, string>();

	public void Start()
	{
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	public void OnGUI()
	{
		bd.Remove(0, bd.Length);
		if (ShowFPS)
		{
			bd.Append("FPS: ").Append((int)fps).Append("\\")
				.Append(Application.targetFrameRate)
				.Append("\n");
		}
		int num = 0;
		foreach (string key in Messages.Keys)
		{
			bd.Append(key).Append(": ").Append(Messages[key])
				.Append("\n");
			num++;
		}
		GUI.Box(new Rect(Screen.width - 200, offsetY, 180f, 20 + num * 20), bd.ToString());
	}

	public void Update()
	{
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > lastInterval + updateInterval)
		{
			fps = (float)frames / (realtimeSinceStartup - lastInterval);
			frames = 0;
			lastInterval = realtimeSinceStartup;
			Messages["Resolution"] = Screen.width + "x" + Screen.height;
		}
	}
}
