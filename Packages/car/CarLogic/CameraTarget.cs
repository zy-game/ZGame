using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CameraTarget
	{
		[NonSerialized]
		public Camera camera;

		[NonSerialized]
		public Collider[] Areas;

		public CameraTarget()
		{
			if (Areas == null)
			{
				Areas = new Collider[0];
			}
		}
	}
}
