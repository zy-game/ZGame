namespace CarLogic
{
	public class RaceCopsNailItem : RaceCopsItemBase
	{
		protected override string objName => "TriggerCopsNail";

		protected override string objPath => RaceConfig.CopsItemNail;

		public override RaceItemId ItemId => RaceItemId.COPS_NAIL;
	}
}
