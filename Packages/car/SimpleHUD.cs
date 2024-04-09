using UnityEngine;

[AddComponentMenu("CarLogic/Simple HUD")]
public class SimpleHUD : MonoBehaviour
{
	private Transform me;

	public Transform TargetCamera;

	public bool InFixedUpdate;

	public void Start()
	{
		me = base.transform;
		if (!TargetCamera && (bool)Camera.main)
		{
			TargetCamera = Camera.main.transform;
		}
	}

	public void Update()
	{
		if (!InFixedUpdate && (bool)TargetCamera)
		{
			me.rotation = TargetCamera.rotation;
		}
	}

	public void FixedUpdate()
	{
		if (InFixedUpdate && (bool)TargetCamera)
		{
			me.rotation = TargetCamera.rotation;
		}
	}
}
