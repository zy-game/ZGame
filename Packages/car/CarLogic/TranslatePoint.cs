using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class TranslatePoint
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 CameraOffset;

		[NonSerialized]
		public Vector3 Normal;

		public float Weight;

		public TranslatePoint()
		{
			Rotation = Quaternion.Euler(0f, 0f, 1f);
		}
	}
}
