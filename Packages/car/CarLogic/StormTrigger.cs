using System;

namespace CarLogic
{
	public class StormTrigger : PassiveTrigger
	{
		private bool old;

		private ItemFloter floater;

		public override RaceItemId ItemId => RaceItemId.STORM;

		public StormTrigger(ushort id, long playerId = 0L)
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
			RaceItemParameters itemParameters = RaceItemFactory.GetItemParameters(RaceItemId.STORM);
			floater = new ItemFloter(carState, RaceItemBase.floatCurve, RaceConfig.ItemStormAttach, finish, RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true), float.Parse(itemParameters.Params[2]));
			floater.Start();
			if (carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
			if (carState.CallBacks.OnFreezeMoveEffect != null)
			{
				carState.CallBacks.OnFreezeMoveEffect(ItemId, RaceItemFactory.GetDelayDuration(ItemId));
			}
			if (RaceAudioManager.ActiveInstance != null)
			{
				carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_hurt_storm);
			}
		}

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
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
			}
		}
	}
}
