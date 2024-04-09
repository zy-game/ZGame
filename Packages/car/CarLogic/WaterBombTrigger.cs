using System;
using UnityEngine;

namespace CarLogic
{
	public class WaterBombTrigger : PassiveTrigger
	{
		private bool old;

		private ItemFloter floater;

		public override RaceItemId ItemId => RaceItemId.WATER_BOMB;

		public WaterBombTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
		}

		public override void ApplyEffect(ItemParams ps)
		{
			base.ApplyEffect(ps);
			if (carState == null || carState.view == null)
			{
				Stop();
				return;
			}
			bool r = true;
			if (carState.CallBacks.AffectChecker != null)
			{
				carState.CallBacks.AffectChecker(ItemId, ref r);
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.CHECK, this);
			}
			if (!r)
			{
				Stop();
				return;
			}
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
			}
			floater = new ItemFloter(carState, RaceItemBase.floatCurve, RaceConfig.ItemWaterBubble, finish, RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true));
			floater.Start();
			if (carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
			if (carState.CallBacks.OnFreezeMoveEffect != null)
			{
				carState.CallBacks.OnFreezeMoveEffect(ItemId, RaceItemFactory.GetDelayDuration(ItemId));
			}
			carState.view.CallDelay(delegate
			{
				if (status != 0)
				{
					CarView view = carState.view;
					view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(update));
				}
			}, 1f);
			if (RaceAudioManager.ActiveInstance != null)
			{
				carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_shuipaojizhong);
			}
		}

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
			}
		}

		private void update()
		{
			if (carState == null || (floater != null && Time.time - floater.StartTime < RaceConfig.MinFloatTime) || carState.CallBacks.ItemPauseChecker == null)
			{
				return;
			}
			bool r = false;
			carState.CallBacks.ItemPauseChecker(ItemId, ref r);
			if (r)
			{
				manualPause = true;
				if (floater != null && !floater.Finished)
				{
					floater.finish();
				}
				else
				{
					Stop();
				}
			}
		}

		public override void Release()
		{
			if ((bool)base.ItemObject)
			{
				GameObject gameObject = base.ItemObject.transform.root.gameObject;
				if ((bool)gameObject)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}

		private void finish()
		{
			if (carState != null && old)
			{
				carState.view.Controller.Active = old;
			}
			Stop();
		}

		public override void Stop()
		{
			base.Stop();
			if (floater != null)
			{
				floater.finish();
			}
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
				if (carState.view != null)
				{
					CarView view = carState.view;
					view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
				}
			}
		}
	}
}
