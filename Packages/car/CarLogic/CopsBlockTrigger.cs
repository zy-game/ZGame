using System;
using UnityEngine;

namespace CarLogic
{
	public class CopsBlockTrigger : CopsBaseTrigger
	{
		private bool dragModified;

		private float dragPlus = 0.8f;

		private float _slowTime = 3f;

		private float _slowSpeed;

		public override RaceItemId ItemId => RaceItemId.COPS_BLOCK;

		public override string AffectEffect => RaceConfig.CopsItemBlockAttach;

		public override AudioClip AffectSound => RaceAudioManager.ActiveInstance.Sound_hurt_block;

		public CopsBlockTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
			_slowTime = parameter.AffectedTime;
			_slowSpeed = float.Parse(parameter.Params[0]) / 3.6f / 3f;
		}

		public override bool CopsApplyEffect(ItemParams ps)
		{
			base.CopsApplyEffect(ps);
			Release();
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks.ResetChecker, new ModifyAction<bool>(checker));
				Vector3 velocity = carState.rigidbody.velocity;
				float num = Mathf.Max(0f, velocity.magnitude - _slowSpeed);
				if ((double)num < 0.001)
				{
					carState.rigidbody.velocity = Vector3.zero;
				}
				else
				{
					carState.rigidbody.velocity = velocity.normalized * num;
				}
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
