namespace CarLogic
{
	public class RaceCopsCarItem : RaceCopsItemBase
	{
		protected override string objName => "TriggerCopsCar";

		protected override string objPath => RaceConfig.CopsItemCar;

		public override RaceItemId ItemId => RaceItemId.COPS_CAR;
	}
}
