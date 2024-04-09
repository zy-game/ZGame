using System;
using UnityEngine;

public class TriggerDelegate : MonoBehaviour
{
	public Action<Collider> AcOnTriggerEnter;

	public Action<Collider> AcOnTriggerStay;

	public Action<Collider> AcOnTriggerExit;

	public void OnTriggerEnter(Collider c)
	{
		if (AcOnTriggerEnter != null)
		{
			AcOnTriggerEnter(c);
		}
	}

	public void OnTriggerStay(Collider c)
	{
		if (AcOnTriggerStay != null)
		{
			AcOnTriggerStay(c);
		}
	}

	public void OnTriggerExit(Collider c)
	{
		if (AcOnTriggerExit != null)
		{
			AcOnTriggerExit(c);
		}
	}
}
