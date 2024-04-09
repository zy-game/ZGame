using System;

namespace CarLogic
{
	public class ActionManager : Singleton<ActionManager>, IInit
	{
		public Action<AbstractView> SetupFastShadow;

		public void Init()
		{
		}
	}
}
