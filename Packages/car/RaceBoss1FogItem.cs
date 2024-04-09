using CarLogic;
using UnityEngine;

public class RaceBoss1FogItem : RaceFogItem
{
	public override RaceItemId ItemId => RaceItemId.FOG;

	public RaceBoss1FogItem(RaceItemParameters param)
		: base(param)
	{
	}

	public override void Toggle(ItemParams ps)
	{
		base.Toggle(ps);
		if (ps == null || ps.user == null || ps.user.view == null)
		{
			return;
		}
		ps.user.view.CallDelay(delegate
		{
			RaceItemBase raceItemBase = RaceItemFactory.BuildItemById(RaceItemId.BOSS1_MINE);
			if (raceItemBase != null)
			{
				ItemParams itemParams = new ItemParams(null, null, 0);
				Vector3 vector = (itemParams.position = RaceItemFactory.GetLayPosition(RaceItemId.BOSS1_MINE, null, ps.user));
				itemParams.user = ps.user;
				raceItemBase.Toggle(itemParams);
			}
		}, 0.5f);
	}
}
