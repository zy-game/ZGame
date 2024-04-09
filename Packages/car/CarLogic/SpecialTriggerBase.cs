using System;

namespace CarLogic
{
	public abstract class SpecialTriggerBase
	{
		protected Action<SpecialTriggerBase, SpecialCallback> callback;

		protected CarState target;

		public abstract SpecialType Type { get; }

		public virtual float Duration
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public virtual void Toggle(CarState state)
		{
			target = state;
			if (callback != null)
			{
				callback(this, SpecialCallback.Toggle);
			}
			if (state.CallBacks.OnSpecialCallback != null)
			{
				state.CallBacks.OnSpecialCallback(target, this, SpecialCallback.Toggle);
			}
			state.ApplyingSpecialType = Type;
			target.ApplyingSpecial = this;
		}

		public virtual void Stop()
		{
			if (callback != null)
			{
				callback(this, SpecialCallback.Stop);
			}
			if (target != null)
			{
				target.ApplyingSpecialType = SpecialType.None;
			}
			if (target.CallBacks.OnSpecialCallback != null)
			{
				target.CallBacks.OnSpecialCallback(target, this, SpecialCallback.Stop);
			}
			target.ApplyingSpecial = null;
		}

		public virtual void Pause()
		{
			if (callback != null)
			{
				callback(this, SpecialCallback.Pause);
			}
		}

		public virtual void Break()
		{
			if (callback != null)
			{
				callback(this, SpecialCallback.Break);
			}
			Stop();
		}

		public void AddCallback(Action<SpecialTriggerBase, SpecialCallback> cb)
		{
			if (cb != null)
			{
				callback = (Action<SpecialTriggerBase, SpecialCallback>)Delegate.Combine(callback, cb);
			}
		}

		public void RemoveCallback(Action<SpecialTriggerBase, SpecialCallback> cb)
		{
			if (cb != null)
			{
				callback = (Action<SpecialTriggerBase, SpecialCallback>)Delegate.Remove(callback, cb);
			}
		}
	}
}
