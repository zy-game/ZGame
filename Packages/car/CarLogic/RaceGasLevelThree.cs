namespace CarLogic
{
	public class RaceGasLevelThree : CommonGasItem
	{
		public override RaceItemId ItemId => RaceItemId.GasLevelThree;

		public RaceGasLevelThree(int level = 1, float duration = 4f, bool useShake = true)
		{
			base.level = level;
			base.duration = duration;
			_useShake = false;
			SuperGasLevel = 3;
		}
	}
}
