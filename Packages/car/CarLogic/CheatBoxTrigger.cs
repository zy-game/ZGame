namespace CarLogic
{
	public class CheatBoxTrigger : MineTrigger
	{
		public override RaceItemId ItemId => RaceItemId.CHEAT_BOX;

		public CheatBoxTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
		}
	}
}
