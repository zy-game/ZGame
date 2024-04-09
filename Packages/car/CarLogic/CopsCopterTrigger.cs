namespace CarLogic
{
	public class CopsCopterTrigger : CopsOilTrigger
	{
		public override RaceItemId ItemId => RaceItemId.COPS_COPTER;

		public CopsCopterTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
		}
	}
}
