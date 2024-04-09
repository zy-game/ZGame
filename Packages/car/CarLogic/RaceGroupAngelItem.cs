namespace CarLogic
{
	public class RaceGroupAngelItem : RaceItemBase
	{
		private RaceItemParameters param;

		public override RaceItemId ItemId => RaceItemId.GROUP_ANGLE;

		public RaceGroupAngelItem(RaceItemParameters param)
		{
			this.param = param;
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			if (ps.targets != null)
			{
				for (int i = 0; i < ps.targets.Length; i++)
				{
					CarState carState = ps.targets[i];
					ItemParams ps2 = new ItemParams(new CarState[1] { carState }, ps.user, ps.instanceId);
					new RaceAngelItem(param, team: true).Toggle(ps2);
				}
			}
			Break();
		}

		public override void Break()
		{
			base.Break();
		}
	}
}
