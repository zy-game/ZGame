namespace CarLogic
{
	public class RaceCopsBlockItem : RaceCopsItemBase
	{
		protected override string objName => "TriggerCopsBlock";

		protected override string objPath => RaceConfig.CopsItemBlock;

		public override RaceItemId ItemId => RaceItemId.COPS_BLOCK;
	}
}
