namespace CarLogic
{
	internal interface IResetChecker
	{
		float ResetDelay { get; }

		object ResetUserData { get; }

		bool StartToWait();

		bool NeedReset(object data);

		bool Cancelable(object data);

		void OnReset();
	}
}
