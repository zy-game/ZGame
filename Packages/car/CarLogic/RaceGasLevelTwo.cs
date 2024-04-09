namespace CarLogic
{
	public class RaceGasLevelTwo : CommonGasItem
	{
		public override RaceItemId ItemId => RaceItemId.GasLevelTwo;

		public RaceGasLevelTwo(int level = 1, float duration = 4f, bool useShake = true)
		{
			base.level = level;
			base.duration = duration;
			_useShake = false;
			SuperGasLevel = 2;
		}
	}
}
