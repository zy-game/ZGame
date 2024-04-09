using System;
using UnityEngine;

namespace CarLogic
{
	public class ItemTweener
	{
		protected CarState target;

		protected float maxDuration = 3f;

		protected float startTime;

		protected Action onFinish = delegate
		{
		};

		protected bool finished;

		public bool Finished => finished;

		public float StartTime => startTime;

		public ItemTweener(CarState target, float duration = 3f, Action callback = null)
		{
			this.target = target;
			maxDuration = duration;
			if (callback != null)
			{
				onFinish = (Action)Delegate.Combine(onFinish, callback);
			}
		}

		public virtual void Start()
		{
			startTime = Time.time;
			finished = false;
		}

		public virtual void finish()
		{
			if (!finished)
			{
				finished = true;
				onFinish();
			}
		}
	}
}
