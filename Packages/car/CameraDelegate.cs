using System;
using UnityEngine;

public class CameraDelegate : MonoBehaviour
{
	public Action dOnPreCull;

	public Action dOnPreRender;

	public Action dOnPostRender;

	public void OnPreCull()
	{
		if (dOnPreCull != null)
		{
			dOnPreCull();
		}
	}

	public void OnPreRender()
	{
		if (dOnPreRender != null)
		{
			dOnPreRender();
		}
	}

	public void OnPostRender()
	{
		if (dOnPostRender != null)
		{
			dOnPostRender();
		}
	}
}
