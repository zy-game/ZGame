using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillGetGasWhenUseAttackItem : CarSkillCopyItemToUseOrGet
	{
		public override CarSkillId ID => CarSkillId.ITEM_ATTACK_USE_GAS;

		public override void Init(int param1, int param2, int param3)
		{
			base.Init(param1, param2, param3);
			SetExParam(new List<RaceItemId>
			{
				RaceItemId.UFO,
				RaceItemId.WATER_FLY,
				RaceItemId.ROCKET,
				RaceItemId.WATER_BOMB
			}, new List<RaceItemId> { RaceItemId.GAS }, isGet: false);
		}

		public override void SetExParam(List<RaceItemId> useIds, List<RaceItemId> getIds, bool isGet)
		{
			base.SetExParam(useIds, getIds, isGet);
		}
	}
}
