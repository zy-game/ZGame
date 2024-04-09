using System;
using UnityEngine;

[Serializable]
public struct Vector3Serializer
{
	[NonSerialized]
	private Vector3 v3;

	public float x;

	public float y;

	public float z;

	public Vector3 V3
	{
		get
		{
			v3.Set(x, y, z);
			return v3;
		}
	}

	public void Fill(Vector3 v3)
	{
		x = v3.x;
		y = v3.y;
		z = v3.z;
	}
}
