using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillUseGasToTeam : CarSkillUseItemToTeamOrOppTeam
	{
		public override CarSkillId ID => CarSkillId.ITEM_USE_GAS_TO_TEAM;

		public override void Init(int param1, int param2, int param3)
		{
			base.Init(param1, param2, param3);
			SetExParam(new List<RaceItemId> { RaceItemId.GAS }, isTeam: true);
		}

		public override void SetExParam(List<RaceItemId> ItemIds, bool isTeam)
		{
			base.SetExParam(ItemIds, isTeam);
		}
	}
}