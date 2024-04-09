using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class RaceAngelItem : RaceItemBase
	{
		private const float defTime = 1f;

		private float defStart;

		private float duration = 3f;

		private float startTime;

		private Transform angelMesh;

		private float offset;

		private ItemFollower follower;

		private bool team;

		private string togglePath = string.Empty;

		private string endPath = string.Empty;

		public override RaceItemId ItemId => RaceItemId.ANGEL;

		public RaceAngelItem(RaceItemParameters param, bool team = false)
		{
			this.team = team;
			togglePath = "Effects/Sence/" + param.Params[0];
			endPath = "Effects/Sence/" + param.Params[1];
			duration = param.LifeTime;
		}

		public override void Toggle(ItemParams ps)
		{
			targets = ps.targets;
			List<RaceItemBase> list = new List<RaceItemBase>(4);
			if (targets[0].view.ItController.ApplyingExcept(this, list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					RaceItemBase raceItemBase = list[i];
					raceItemBase.Break();
				}
			}
			base.Toggle(ps);
			if (targets == null || targets.Length < 1)
			{
				if (ps.user == null)
				{
					Break();
					return;
				}
				targets = new CarState[1] { ps.user };
				itemParams.targets = targets;
			}
			if (targets != null && targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
			{
				targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
			}
			string path = togglePath;
			Singleton<ResourceOffer>.Instance.Load(path, delegate(UnityEngine.Object o)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
				if (!(gameObject == null))
				{
					if (status != 0)
					{
						angelMesh = gameObject.transform;
						angelMesh.rotation = targets[0].transform.rotation;
						follower = new ItemFollower(targets[0], angelMesh, offset, null, null, look: true);
						follower.Start();
					}
					else
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			});
			if (RaceAudioManager.ActiveInstance != null)
			{
				targets[0].view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_tianshi);
			}
			startTime = Time.time;
			CarView view = targets[0].view;
			view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(update));
			CarCallBack callBacks = targets[0].CallBacks;
			callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
			CarCallBack callBacks2 = targets[0].CallBacks;
			callBacks2.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks2.AffectChecker, new ModifyAction<RaceItemId, bool>(attackChecker));
		}

		private void onAffectByItem(RaceItemId id, CarState car, ItemCallbackType type, object o)
		{
			if (type != ItemCallbackType.CHECK)
			{
				return;
			}
			switch (id)
			{
				case RaceItemId.ROCKET:
				case RaceItemId.WATER_FLY:
				case RaceItemId.WATER_BOMB:
				case RaceItemId.INK_BOTTLE:
				case RaceItemId.BANANA:
				case RaceItemId.MINE:
				case RaceItemId.CHEAT_BOX:
				case RaceItemId.CONTROL_REVERT:
				{
					bool attackable = true;
					attackChecker(id, ref attackable);
					if (!attackable)
					{
						onDefend(car);
					}
					break;
				}
				case RaceItemId.UFO:
				case RaceItemId.FOG:
					break;
			}
		}

		private void update()
		{
			if (Time.time - startTime > duration)
			{
				Break();
			}
			else if ((bool)angelMesh && !(Time.time - defStart < 1f) && follower != null)
			{
				follower.Pause = false;
			}
		}

		private void attackChecker(RaceItemId item, ref bool attackable)
		{
			if (!attackable)
			{
				return;
			}
			switch (item)
			{
				case RaceItemId.NONE:
				case RaceItemId.UFO:
				case RaceItemId.FOG:
				case RaceItemId.GAS:
				case RaceItemId.GROUP_GAS:
				case RaceItemId.ANGEL:
				case RaceItemId.UFO_CONFUSER:
				case RaceItemId.GROUP_ANGLE:
				case RaceItemId.COUPLE_GAS:
				case RaceItemId.CUPID_GAS:
					return;
			}
			if (team || item != RaceItemId.CONTROL_REVERT)
			{
				attackable = false;
			}
		}

		private void onDefend(CarState state)
		{
			if (state == null)
			{
				return;
			}
			if (state.CallBacks.OnAffectedByItem != null)
			{
				state.CallBacks.OnAffectedByItem(ItemId, state, ItemCallbackType.DEFEND, this);
			}
			string path = endPath;
			Singleton<ResourceOffer>.Instance.Load(path, delegate(UnityEngine.Object o)
			{
				if (status != 0)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					if (!(gameObject == null))
					{
						Transform transform = gameObject.transform;
						if (state == null || state.transform == null)
						{
							UnityEngine.Object.Destroy(transform.gameObject);
						}
						else
						{
							transform.parent = state.transform;
							transform.localRotation = Quaternion.identity;
							transform.localPosition = Vector3.zero;
							if (follower != null)
							{
								follower.Pause = true;
							}
							UnityEngine.Object.Destroy(gameObject, 1f);
							defStart = Time.time;
							if (RaceAudioManager.ActiveInstance != null)
							{
								state.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_tianshi);
							}
						}
					}
				}
			});
			Break();
		}

		public override void Break()
		{
			base.Break();
			if (follower != null)
			{
				follower.finish();
			}
			if (targets == null || targets.Length < 1)
			{
				if ((bool)angelMesh)
				{
					UnityEngine.Object.Destroy(angelMesh.gameObject);
				}
				return;
			}
			CarView view = targets[0].view;
			view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(update));
			CarCallBack callBacks = targets[0].CallBacks;
			callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
			CarCallBack callBacks2 = targets[0].CallBacks;
			callBacks2.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks2.AffectChecker, new ModifyAction<RaceItemId, bool>(attackChecker));
			if ((bool)angelMesh)
			{
				UnityEngine.Object.Destroy(angelMesh.gameObject);
			}
		}
	}
}
