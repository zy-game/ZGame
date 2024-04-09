using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class EffectAnchor
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public EffectAnchor()
		{
			Position = new Vector3(0f, 0f, 0.7f);
			Rotation = new Quaternion(0f, 0f, 0f, 1f);
		}
	}
}
