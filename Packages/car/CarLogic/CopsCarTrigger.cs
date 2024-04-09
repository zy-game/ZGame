using System;
using UnityEngine;

namespace CarLogic
{
	public class CopsCarTrigger : CopsBaseTrigger
	{
		private bool dragModified;

		private float dragPlus = 0.8f;

		private float _slowTime = 3f;

		private float _slowSpeed = 1f;

		public override RaceItemId ItemId => RaceItemId.COPS_CAR;

		public override string AffectEffect => RaceConfig.CopsItemCarAttach;

		public override AudioClip AffectSound => RaceAudioManager.ActiveInstance.Sound_hurt_block;

		public CopsCarTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
			_slowTime = parameter.AffectedTime;
			_slowSpeed = float.Parse(parameter.Params[0]);
		}

		public override bool CopsApplyEffect(ItemParams ps)
		{
			base.CopsApplyEffect(ps);
			Release();
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks.ResetChecker, new ModifyAction<bool>(checker));
				carState.rigidbody.velocity *= _slowSpeed;
				carState.rigidbody.drag += dragPlus;
				dragModified = true;
			}
			carState.view.CallDelay(Stop, parameter.AffectedTime);
			return true;
		}

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
			}
		}

		private void checker(ref bool enable)
		{
			enable = false;
		}

		public override void Stop()
		{
			base.Stop();
			if (carState != null && carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks.ResetChecker, new ModifyAction<bool>(checker));
				if (dragModified)
				{
					carState.rigidbody.drag -= dragPlus;
					dragModified = false;
				}
			}
		}
	}
}
