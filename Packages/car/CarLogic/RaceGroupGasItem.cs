namespace CarLogic
{
	public class RaceGroupGasItem : RaceItemBase
	{
		public override RaceItemId ItemId => RaceItemId.GROUP_GAS;

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			if (ps.targets != null)
			{
				for (int i = 0; i < ps.targets.Length; i++)
				{
					CarState carState = ps.targets[i];
					ItemParams ps2 = new ItemParams(new CarState[1] { carState }, ps.user, ps.instanceId);
					CommonGasItem commonGasItem = new CommonGasItem(1, RaceConfig.GroupGasTime, useShake: false);
					commonGasItem.IsTeam = true;
					commonGasItem.Toggle(ps2);
				}
			}
			Break();
		}
	}
}
