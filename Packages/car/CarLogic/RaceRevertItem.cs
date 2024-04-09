using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class RaceRevertItem : RaceItemBase
	{
		private float duration = 4f;

		private float startTime;

		private const float fadeTime = 0.5f;

		private ItemFollower follower;

		private GameObject itemObject;

		private Animator[] antors;

		private int stage = -1;

		private float throwDuration;

		private float throwTime;

		private Vector3 floatOffset = Vector3.zero;

		public override RaceItemId ItemId => RaceItemId.CONTROL_REVERT;

		public RaceRevertItem(RaceItemParameters param)
		{
			duration = param.AffectedTime;
		}

		protected override void init()
		{
			base.init();
		}

		private void PlayDevil()
		{
			CarState carState = targets[0];
			Transform transform = itemObject.transform;
			follower = new ItemFollower(carState, transform, 0f);
			follower.Start();
			antors = itemObject.GetComponentsInChildren<Animator>();
			setAnimatorParams(-1);
			if (RaceAudioManager.ActiveInstance != null)
			{
				AudioSource audioSource = itemObject.AddComponent<AudioSource>();
				audioSource.clip = RaceAudioManager.ActiveInstance.Sound_mogui;
				audioSource.loop = true;
				audioSource.Play();
			}
			stage = 0;
			carState.view.PlayOnDevil();
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			targets = ps.targets;
			if (targets == null || targets.Length < 1 || targets[0] == null || targets[0].view == null)
			{
				Break();
				return;
			}
			bool r = true;
			if (targets[0].CallBacks.AffectChecker != null)
			{
				targets[0].CallBacks.AffectChecker(ItemId, ref r);
			}
			if (targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
			{
				targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.CHECK, this);
			}
			if (!r)
			{
				Break();
				return;
			}
			CarState state = targets[0];
			if (state != null && !state.ReachedEnd)
			{
				List<RaceItemBase> list = new List<RaceItemBase>(4);
				if (state.view.ItController.ApplyingExcept(this, list))
				{
					list[0].Enhance(duration);
					Break();
					return;
				}
				CarView view = state.view;
				view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(onUpdate));
				startTime = Time.time + 100f;
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemEvil, delegate(UnityEngine.Object o)
				{
					startTime = Time.time;
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					if (!(gameObject == null))
					{
						itemObject = gameObject;
						itemObject.transform.position = base.Params.user.transform.TransformPoint(floatOffset);
						itemObject.transform.localRotation = Quaternion.identity;
						CarView view2 = state.view;
						view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(Throwing));
					}
				});
			}
			else
			{
				Break();
			}
		}

		private void Throwing()
		{
			itemObject.transform.position = base.Params.user.transform.TransformPoint(floatOffset);
			itemObject.transform.localRotation = Quaternion.identity;
			throwTime += Time.deltaTime;
			if (throwTime >= throwDuration)
			{
				CarState carState = targets[0];
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(Throwing));
				CarView view2 = carState.view;
				view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(onUpdate));
				PlayDevil();
			}
		}

		private float onSteer(CarState state)
		{
			return 0f - state.Steer;
		}

		private void onUpdate()
		{
			if (targets == null || targets.Length == 0)
			{
				Break();
				return;
			}
			if (stage == 0 && Time.time - startTime > 0.5f)
			{
				stage = 1;
				CarState carState = targets[0];
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnSteer = (Func<CarState, float>)Delegate.Combine(callBacks.OnSteer, new Func<CarState, float>(onSteer));
				carState.CurDriftState.StartSteer *= -1f;
				setAnimatorParams(0);
				if (carState.CallBacks.OnAffectedByItem != null)
				{
					carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
				}
			}
			if (stage == 1 && Time.time - startTime > duration + 0.5f)
			{
				setAnimatorParams(1);
				stage = 2;
			}
			if (stage == 2 && Time.time - startTime > duration + 1f)
			{
				Break();
			}
		}

		public override void Enhance(float addition)
		{
			base.Enhance(addition);
			startTime = Time.time + addition - duration;
		}

		public override void Break()
		{
			base.Break();
			if (follower != null)
			{
				follower.finish();
			}
			if (targets != null)
			{
				for (int i = 0; i < targets.Length; i++)
				{
					CarState carState = targets[i];
					CarCallBack callBacks = carState.CallBacks;
					callBacks.OnSteer = (Func<CarState, float>)Delegate.Remove(callBacks.OnSteer, new Func<CarState, float>(onSteer));
					if ((bool)carState.view)
					{
						CarView view = carState.view;
						view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
					}
				}
			}
			if ((bool)itemObject)
			{
				UnityEngine.Object.Destroy(itemObject);
			}
		}

		private void setAnimatorParams(int p)
		{
			if (antors == null)
			{
				return;
			}
			for (int i = 0; i < antors.Length; i++)
			{
				if (antors[i] != null)
				{
					antors[i].SetInteger("state", p);
				}
			}
		}
	}
}
