using System;
using UnityEngine;

[Serializable]
public class Bool
{
	[SerializeField]
	[HideInInspector]
	private int key = 132156452;

	[HideInInspector]
	[SerializeField]
	private int v;

	[HideInInspector]
	[SerializeField]
	private int keyB;

	[HideInInspector]
	[SerializeField]
	private int vB;

	public Bool(bool a = false)
	{
		key += (int)DateTime.Now.Ticks;
		v = ~key + ((!a) ? 999 : 0);
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
		return ((bool)this).GetHashCode();
	}

	public override string ToString()
	{
		return ((bool)this).ToString();
	}

	public static implicit operator bool(Bool a)
	{
		if (object.Equals(null, a))
		{
			return false;
		}
		if (RaceMemModified.IsCheckedMem && !a.CheckVB())
		{
			if (RaceMemModified.OnMemModifiedEvent != null)
			{
				RaceMemModified.OnMemModifiedEvent();
			}
			return false;
		}
		return ~a.key == a.v;
	}

	public static implicit operator Bool(bool a)
	{
		return new Bool(a);
	}

	public static bool operator ==(Bool a, Bool b)
	{
		return (bool)a == (bool)b;
	}

	public static bool operator !=(Bool a, Bool b)
	{
		return (bool)a != (bool)b;
	}

	public override bool Equals(object obj)
	{
		if (obj is bool || obj is Bool)
		{
			return (bool)this == (bool)obj;
		}
		return false;
	}

	public static int GetV(int k, bool a)
	{
		if (!a)
		{
			return ~k + 999;
		}
		return ~k;
	}

	public static int GetVB(int kB, int v)
	{
		return v ^ kB;
	}

	public static bool B2b(int k, int v)
	{
		Bool @bool = new Bool();
		@bool.key = k;
		@bool.v = v;
		return @bool;
	}
}
