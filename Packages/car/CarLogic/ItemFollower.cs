using System;
using UnityEngine;

namespace CarLogic
{
	public class ItemFollower : ItemTweener
	{
		private Transform item;

		private float offset;

		private Camera camera;

		private bool pause;

		private bool synRotation;

		public bool Pause { get; set; }

		public float MaxDuration
		{
			get
			{
				return maxDuration;
			}
			set
			{
				maxDuration = value;
			}
		}

		public ItemFollower(CarState state, Transform itemObject, float offset, Action onOver = null, Camera cam = null, bool look = false)
			: base(state, 999f, onOver)
		{
			item = itemObject;
			this.offset = offset;
			camera = cam;
			synRotation = look;
		}

		public override void Start()
		{
			base.Start();
			if (target != null && target.view != null)
			{
				CarView view = target.view;
				view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(update));
				CarCallBack callBacks = target.CallBacks;
				callBacks.PauseOnThrow = (Action<bool>)Delegate.Combine(callBacks.PauseOnThrow, new Action<bool>(onThrow));
				if ((bool)item)
				{
					item.parent = target.transform;
					Vector3 localPosition = new Vector3(0f, offset, 0f);
					item.localPosition = localPosition;
					item.localRotation = Quaternion.identity;
				}
			}
		}

		public override void finish()
		{
			base.finish();
			if (target != null && target.view != null)
			{
				CarView view = target.view;
				view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(update));
				CarCallBack callBacks = target.CallBacks;
				callBacks.PauseOnThrow = (Action<bool>)Delegate.Remove(callBacks.PauseOnThrow, new Action<bool>(onThrow));
			}
		}

		private void onThrow(bool paused)
		{
			if ((bool)item && target != null && target.transform != null)
			{
				if (paused)
				{
					item.transform.parent = null;
					return;
				}
				item.parent = target.transform;
				Vector3 localPosition = new Vector3(0f, offset, 0f);
				item.localPosition = localPosition;
				item.localRotation = Quaternion.identity;
			}
		}

		private void update()
		{
			if (target == null || target.view == null || item == null)
			{
				finish();
			}
			else if (!pause)
			{
				bool flag = synRotation;
			}
		}
	}
}
