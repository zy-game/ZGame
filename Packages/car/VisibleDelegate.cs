using System;
using UnityEngine;

public class VisibleDelegate : MonoBehaviour
{
	public Action<bool> AcOnVisibleChange;

	public void OnBecameVisible()
	{
		if (AcOnVisibleChange != null)
		{
			AcOnVisibleChange(obj: true);
		}
	}

	public void OnBecameInvisible()
	{
		if (AcOnVisibleChange != null)
		{
			AcOnVisibleChange(obj: false);
		}
	}
}
