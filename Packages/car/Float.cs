using System;
using UnityEngine;

[Serializable]
public class Float
{
	[SerializeField]
	[HideInInspector]
	private int key = 4623158;

	[HideInInspector]
	[SerializeField]
	private int v;

	[HideInInspector]
	[SerializeField]
	private int keyB;

	[HideInInspector]
	[SerializeField]
	private int vB;

	public unsafe Float(float a = 0f)
	{
		key += (int)DateTime.Now.Ticks;
		int num = *(int*)(&a);
		v = (key + num) ^ key;
		UpdateVB();
	}

	private void UpdateVB()
	{
		keyB = (int)DateTime.Now.Ticks;
		vB = v ^ keyB;
	}

	private bool CheckVB()
	{
		return v == (vB ^ keyB);
	}

	public override int GetHashCode()
	{
		return ((float)this).GetHashCode();
	}

	public override string ToString()
	{
		return ((float)this).ToString();
	}

	public string ToString(string format)
	{
		return ((float)this).ToString(format);
	}

	public unsafe static implicit operator float(Float a)
	{
		if (object.Equals(null, a))
		{
			return 0f;
		}
		if (RaceMemModified.IsCheckedMem && !a.CheckVB())
		{
			if (RaceMemModified.OnMemModifiedEvent != null)
			{
				RaceMemModified.OnMemModifiedEvent();
			}
			return 0f;
		}
		int num = (a.v ^ a.key) - a.key;
		return *(float*)(&num);
	}

	public static implicit operator Float(float a)
	{
		return new Float(a);
	}

	public static implicit operator Float(int a)
	{
		return new Float(a);
	}

	public static bool operator ==(Float a, Float b)
	{
		return (float)a == (float)b;
	}

	public static bool operator !=(Float a, Float b)
	{
		return (float)a != (float)b;
	}

	public override bool Equals(object obj)
	{
		if (obj is float || obj is Float)
		{
			return (float)this == (float)obj;
		}
		return false;
	}

	public unsafe static int GetV(int k, float a)
	{
		int num = *(int*)(&a);
		return (k + num) ^ k;
	}

	public static int GetVB(int kB, int v)
	{
		return v ^ kB;
	}

	public static float F2f(int k, int v)
	{
		Float @float = new Float();
		@float.key = k;
		@float.v = v;
		@float.UpdateVB();
		return @float;
	}
}
