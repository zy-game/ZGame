using UnityEngine;

namespace CarLogic
{
	public class CopsNailTrigger : CopsBlockTrigger
	{
		public override RaceItemId ItemId => RaceItemId.COPS_NAIL;

		public override string AffectEffect => RaceConfig.CopsItemNailAttach;

		public override AudioClip AffectSound => RaceAudioManager.ActiveInstance.Sound_hurt_block;

		public CopsNailTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
		}
	}
}
