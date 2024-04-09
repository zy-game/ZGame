using System;
using System.Collections.Generic;
using UnityEngine;

public class Underline
{
	public static Action After(int count, Action callback)
	{
		if (count == 0)
		{
			Debug.LogWarning("count is 0 ?? so callback invoked rightnow~!");
			callback();
		}
		int callTimes = 0;
		return delegate
		{
			callTimes++;
			if (callTimes == count)
			{
				callback();
			}
		};
	}

	public static Action<T> After<T>(int count, Action<T> callback)
	{
		if (count == 0)
		{
			Debug.LogWarning("count is 0 ?? so callback invoked rightnow~!");
			callback(default(T));
		}
		int callTimes = 0;
		return delegate(T obj)
		{
			callTimes++;
			if (callTimes == count)
			{
				callback(obj);
			}
		};
	}

	public static Action Once(Action callback)
	{
		return After(1, callback);
	}

	public static Action<T> Once<T>(Action<T> callback)
	{
		return After(1, callback);
	}

	public static T Max<T>(Dictionary<int, T> infos, Func<T, int> getter)
	{
		int num = int.MinValue;
		T result = default(T);
		foreach (KeyValuePair<int, T> info in infos)
		{
			int num2 = getter(info.Value);
			if (num2 > num)
			{
				num = num2;
				result = info.Value;
			}
		}
		return result;
	}

	public static T Max<T>(IEnumerable<T> infos, Func<T, int> getter)
	{
		int num = int.MinValue;
		T result = default(T);
		foreach (T info in infos)
		{
			int num2 = getter(info);
			if (num2 > num)
			{
				num = num2;
				result = info;
			}
		}
		return result;
	}

	public static bool Contains<T>(IEnumerable<T> infos, int value, Func<T, int> getter)
	{
		foreach (T info in infos)
		{
			int num = getter(info);
			if (num == value)
			{
				return true;
			}
		}
		return false;
	}

	public static T Min<T>(Dictionary<int, T> infos, Func<T, int> getter)
	{
		int num = int.MaxValue;
		T result = default(T);
		foreach (KeyValuePair<int, T> info in infos)
		{
			int num2 = getter(info.Value);
			if (num2 < num)
			{
				num = num2;
				result = info.Value;
			}
		}
		return result;
	}

	public static T Min<T>(IEnumerable<T> infos, Func<T, int> getter)
	{
		int num = int.MaxValue;
		T result = default(T);
		foreach (T info in infos)
		{
			int num2 = getter(info);
			if (num2 < num)
			{
				num = num2;
				result = info;
			}
		}
		return result;
	}

	public static List<V> Pluck<T, V>(IEnumerable<T> datas, Func<T, V> func)
	{
		List<V> list = new List<V>();
		foreach (T data in datas)
		{
			list.Add(func(data));
		}
		return list;
	}

	public static Action Throttle(Action action, int wait)
	{
		float lastCallTime = 0f;
		return delegate
		{
			if (lastCallTime <= 0f || Time.realtimeSinceStartup - lastCallTime >= (float)wait)
			{
				lastCallTime = Time.realtimeSinceStartup;
				action();
			}
		};
	}

	public static Dictionary<string, V> Map<T, V>(IEnumerable<T> list, Func<T, string> funcKey, Func<T, V> funcValue)
	{
		Dictionary<string, V> dictionary = new Dictionary<string, V>();
		foreach (T item in list)
		{
			dictionary[funcKey(item)] = funcValue(item);
		}
		return dictionary;
	}

	public static T Find<T>(IEnumerable<T> list, Func<T, bool> func)
	{
		foreach (T item in list)
		{
			if (func(item))
			{
				return item;
			}
		}
		return default(T);
	}

	public static long Reduce<T>(IEnumerable<T> list, Func<long, T, long> func)
	{
		long num = 0L;
		foreach (T item in list)
		{
			num = func(num, item);
		}
		return num;
	}

	public static List<T> Filter<T>(IEnumerable<T> list, Func<T, bool> func)
	{
		List<T> list2 = new List<T>();
		foreach (T item in list)
		{
			if (func(item))
			{
				list2.Add(item);
			}
		}
		return list2;
	}
}
