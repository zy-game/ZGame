using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillCopyUseItem : CarSkillCopyItemToUseOrGet
	{
		public override CarSkillId ID => CarSkillId.ITEM_CPOY_USE;

		public override void Init(int param1, int param2, int param3)
		{
			base.Init(param1, param2, param3);
			SetExParam(new List<RaceItemId>(), new List<RaceItemId>(), isGet: true);
		}

		public override void SetExParam(List<RaceItemId> useIds, List<RaceItemId> getIds, bool isGet)
		{
			base.SetExParam(useIds, getIds, isGet);
		}
	}
}
