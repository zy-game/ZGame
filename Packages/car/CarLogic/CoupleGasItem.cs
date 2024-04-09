namespace CarLogic
{
	public class CoupleGasItem : CommonGasItem
	{
		public override RaceItemId ItemId => RaceItemId.COUPLE_GAS;

		public CoupleGasItem(int level = 1, float duration = 4f, bool useShake = false)
			: base(level, duration, useShake)
		{
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			if (carState != null)
			{
				carState.N2State.GasType = N2StateGasType.COUPLE;
			}
		}

		protected override void update()
		{
			if (carState != null && !(carState.view == null))
			{
				base.update();
				carState.N2State.GasType = N2StateGasType.COUPLE;
			}
		}
	}
}
