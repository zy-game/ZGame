namespace CarLogic
{
	public class RaceCopsOilItem : RaceCopsItemBase
	{
		protected override string objName => "TriggerCopsOil";

		protected override string objPath => RaceConfig.CopsItemOil;

		public override RaceItemId ItemId => RaceItemId.COPS_OIL;
	}
}
