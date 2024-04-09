using UdpAdapterClient;
using UnityEngine;

namespace CarLogic
{
	public class SyncTimeForecast
	{
		private bool IsDemo;

		public long time
		{
			get
			{
				if (IsDemo)
				{
					return udp_time.NowGMT() + 20;
				}
				if ((bool)SyncMoveController.OpenNewForeCast)
				{
					return udp_time.GameNow();
				}
				return 0L;
			}
		}

		public float deltaTime => Time.fixedDeltaTime;

		public void init()
		{
			if (!IsDemo)
			{
				IsDemo = Application.loadedLevelName.ToLower().Contains("demo");
			}
		}
	}
}
