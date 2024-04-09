using System;
using UnityEngine;

[Serializable]
public class Int : IComparable
{
	[HideInInspector]
	[SerializeField]
	private int key = 12348978;

	[SerializeField]
	[HideInInspector]
	private int v;

	[SerializeField]
	[HideInInspector]
	private int keyB = 9838713;

	[SerializeField]
	[HideInInspector]
	private int vB;

	public Int(int a = 0)
	{
		key += (int)DateTime.Now.Ticks;
		v = (key + a) ^ key;
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
		return ((int)this).GetHashCode();
	}

	public override string ToString()
	{
		return ((int)this).ToString();
	}

	public string ToString(string format)
	{
		return ((int)this).ToString(format);
	}

	public static implicit operator int(Int a)
	{
		if (object.Equals(null, a))
		{
			return 0;
		}
		if (RaceMemModified.IsCheckedMem && !a.CheckVB())
		{
			if (RaceMemModified.OnMemModifiedEvent != null)
			{
				RaceMemModified.OnMemModifiedEvent();
			}
			return 0;
		}
		return (a.v ^ a.key) - a.key;
	}

	public static implicit operator Int(int a)
	{
		return new Int(a);
	}

	public static implicit operator Int(float a)
	{
		return new Int((int)a);
	}

	public static Int operator ++(Int a)
	{
		return new Int((int)a + 1);
	}

	public static Int operator --(Int a)
	{
		return new Int((int)a - 1);
	}

	public static bool operator ==(Int a, Int b)
	{
		return (int)a == (int)b;
	}

	public static bool operator !=(Int a, Int b)
	{
		return (int)a != (int)b;
	}

	public override bool Equals(object obj)
	{
		if (obj is int || obj is Int)
		{
			return (int)obj == (int)this;
		}
		return false;
	}

	public int CompareTo(object a)
	{
		return ((int)this).CompareTo((int)a);
	}

	public static int GetV(int k, int a)
	{
		return (k + a) ^ k;
	}

	public static int GetVB(int kB, int v)
	{
		return v ^ kB;
	}

	public static int I2i(int k, int v)
	{
		Int @int = new Int();
		@int.key = k;
		@int.v = v;
		return @int;
	}
}
