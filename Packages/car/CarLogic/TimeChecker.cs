using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public static class TimeChecker
	{
		private class Checker
		{
			public long addTime;

			public long passTime;

			public long curTime;

			public long frameCount;

			public Checker(long time)
			{
				addTime = time;
				frameCount = 0L;
				passTime = 0L;
				curTime = 0L;
			}

			public void start(long time)
			{
				curTime = time;
			}

			public void end(long time)
			{
				passTime += time - curTime;
				frameCount++;
			}

			public float AvgTime()
			{
				return (float)passTime / (float)frameCount;
			}
		}

		private static Dictionary<string, Checker> m_dicStartTime = new Dictionary<string, Checker>();

		public static void StartCheck(string name)
		{
			if (!m_dicStartTime.ContainsKey(name))
			{
				m_dicStartTime.Add(name, new Checker(DateTime.Now.Ticks));
			}
			m_dicStartTime[name].start(DateTime.Now.Ticks);
		}

		public static void EndCheck(string name)
		{
			if (m_dicStartTime.ContainsKey(name))
			{
				m_dicStartTime[name].end(DateTime.Now.Ticks);
				if (DateTime.Now.Ticks - m_dicStartTime[name].addTime > 30000000)
				{
					Debug.LogWarning("TimeChecker check name : " + name + "\t Avg Time : " + (double)m_dicStartTime[name].AvgTime() * 0.1 * 0.001 * 0.001 + " s");
					m_dicStartTime.Remove(name);
				}
			}
			else
			{
				Debug.LogError("TimeChecker have no check, name : " + name);
			}
		}
	}
}
