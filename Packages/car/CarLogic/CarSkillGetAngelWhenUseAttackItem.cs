using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillGetAngelWhenUseAttackItem : CarSkillCopyItemToUseOrGet
	{
		public override CarSkillId ID => CarSkillId.ITEM_ATTACK_USE_ANGEL;

		public override void Init(int param1, int param2, int param3)
		{
			base.Init(param1, param2, param3);
			SetExParam(new List<RaceItemId>
			{
				RaceItemId.MINE,
				RaceItemId.FOG,
				RaceItemId.CHEAT_BOX,
				RaceItemId.BANANA
			}, new List<RaceItemId> { RaceItemId.ANGEL }, isGet: false);
		}

		public override void SetExParam(List<RaceItemId> useIds, List<RaceItemId> getIds, bool isGet)
		{
			base.SetExParam(useIds, getIds, isGet);
		}
	}
}
