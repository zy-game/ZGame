using System;
using UnityEngine;

namespace CarLogic
{
	public class SyncTime
	{
		public float offset;

		private static float begin;

		private static float end;

		private static float easeTime;

		private static Func<float, float, float, float> easeFunc;

		private float cacheNow;

		private float cacheDelta;

		private float cacheLast;

		public float time
		{
			get
			{
				float num = Time.time + offset;
				if (easeFunc != null && num >= begin && num <= end)
				{
					if (cacheNow > 0f)
					{
						return cacheNow;
					}
					cacheNow = easeFunc(begin, end, (num - begin) / easeTime);
					if (cacheNow >= end)
					{
						return num;
					}
					return cacheNow;
				}
				return num;
			}
		}

		public float deltaTime
		{
			get
			{
				float num = Time.time;
				if (easeFunc != null && num >= begin && num <= end)
				{
					if (cacheDelta > 0f)
					{
						return cacheDelta;
					}
					if (time >= end)
					{
						return Time.deltaTime;
					}
					if (cacheLast <= 0f)
					{
						float num2 = num - Time.deltaTime;
						cacheLast = easeFunc(begin, end, (num2 - begin) / easeTime);
					}
					cacheDelta = time - cacheLast;
					return cacheDelta;
				}
				return Time.deltaTime;
			}
		}

		public void ClearCache()
		{
			cacheNow = 0f;
			cacheDelta = 0f;
			cacheLast = 0f;
		}

		public void StartEase(Func<float, float, float, float> easeAction, float duration)
		{
			begin = Time.time;
			end = begin + duration;
			easeTime = duration;
			easeFunc = easeAction;
		}

		public void ClearEase()
		{
			begin = 0f;
			end = 0f;
			easeTime = 0f;
			easeFunc = null;
		}
	}
}
