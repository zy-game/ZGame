namespace CarLogic
{
	public class CopsElectricTrigger : CopsOilTrigger
	{
		public override RaceItemId ItemId => RaceItemId.COPS_ELECTRIC;

		public override string AffectEffect => RaceConfig.CopsItemElectricAttach;

		public CopsElectricTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
		}
	}
}
