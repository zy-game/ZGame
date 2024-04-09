namespace CarLogic
{
	internal class ResetWaiter
	{
		public IResetChecker Checker;

		public float ResetTime;

		public object UserData;

		public ResetWaiter(IResetChecker checker, float resetTime, object userdata = null)
		{
			Checker = checker;
			ResetTime = resetTime;
			UserData = userdata;
		}
	}
}
