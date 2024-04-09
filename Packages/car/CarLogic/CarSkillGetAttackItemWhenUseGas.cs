using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillGetAttackItemWhenUseGas : CarSkillCopyItemToUseOrGet
	{
		public override CarSkillId ID => CarSkillId.ITEM_USE_ANGEL_CPOY_ATTACK_ITEM2;

		public override void Init(int param1, int param2, int param3)
		{
			base.Init(param1, param2, param3);
			SetExParam(new List<RaceItemId> { RaceItemId.GAS }, new List<RaceItemId>
			{
				RaceItemId.UFO,
				RaceItemId.WATER_FLY,
				RaceItemId.ROCKET,
				RaceItemId.WATER_BOMB
			}, isGet: false);
		}

		public override void SetExParam(List<RaceItemId> useIds, List<RaceItemId> getIds, bool isGet)
		{
			base.SetExParam(useIds, getIds, isGet);
		}
	}
}
