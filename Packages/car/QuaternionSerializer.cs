using System;
using UnityEngine;

[Serializable]
public struct QuaternionSerializer
{
	[NonSerialized]
	private Quaternion q;

	public const float Accuracy = 0.001f;

	public short sx;

	public short sy;

	public short sz;

	public short sw;

	public float x => 0.001f * (float)sx;

	public float y => 0.001f * (float)sy;

	public float z => 0.001f * (float)sz;

	public float w => 0.001f * (float)sw;

	public Quaternion Q
	{
		get
		{
			q.Set(x, y, z, w);
			return q;
		}
	}

	public void Fill(Quaternion q)
	{
		sx = (short)(q.x / 0.001f);
		sy = (short)(q.y / 0.001f);
		sz = (short)(q.z / 0.001f);
		sw = (short)(q.w / 0.001f);
	}
}
