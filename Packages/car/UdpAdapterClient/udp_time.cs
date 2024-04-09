using System;
using UnityEngine;

namespace UdpAdapterClient
{
	public class udp_time
	{
		private static long netMgrStartUtc;

		private static long netMgrStartTime = 0L;

		public static ushort avg_server_delay = 0;

		public static ushort avg_client_delay = 0;

		public static ushort sync_net_delay = 0;

		public static long sync_net_offset = 0L;

		public static long gameTime = 0L;

		private static DateTime DEFAULT_UTC_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long timeOffset { get; set; }

		public static long timeSvrOffset { get; set; }

		public static long syn_time_1 { get; set; }

		public static long syn_time_2 { get; set; }

		public static long syn_time_3 { get; set; }

		public static long syn_time_4 { get; set; }

		public static void InitStartTime()
		{
			netMgrStartUtc = NowGMT();
			netMgrStartTime = (long)(Time.realtimeSinceStartup * 1000f);
		}

		public static void update()
		{
			updateTimeOffect();
		}

		public static long GameNow()
		{
			return NowGMT() + timeOffset - sync_net_offset;
		}

		public static long NowGMT()
		{
			return udp_define.datetime_to_ms(DateTime.UtcNow) - udp_define.datetime_to_ms(DEFAULT_UTC_TIME);
		}

		public static long LogicTime()
		{
			long num = (long)(Time.realtimeSinceStartup * 1000f);
			return num - netMgrStartTime + netMgrStartUtc;
		}

		public static void updateTimeOffect()
		{
			long num = NowGMT();
			long num2 = LogicTime();
			timeOffset = num2 - num;
		}

		public static void resetSyncTime()
		{
			syn_time_1 = 0L;
			syn_time_2 = 0L;
			syn_time_3 = 0L;
			syn_time_4 = 0L;
		}

		public static void CalculateSyncTime()
		{
			sync_net_delay = (ushort)(syn_time_2 - syn_time_1 + (syn_time_4 - syn_time_3));
			sync_net_offset = (long)((double)(syn_time_2 - syn_time_1 + syn_time_3 - syn_time_4) * 0.5);
			udp_define.printf("syn1:{0},syn2:{1},syn3:{2},syn4:{3},delay:{4},offset:{5},time:{6},rtime:{7}", syn_time_1, syn_time_2, syn_time_3, syn_time_4, sync_net_delay, sync_net_offset, NowGMT(), GameNow());
			resetSyncTime();
		}

		public static ushort CalculateServerDelay(long nDelayMs)
		{
			ushort num = (ushort)((nDelayMs > 0) ? nDelayMs : 0);
			if (avg_server_delay < 0)
			{
				avg_server_delay = num;
			}
			else
			{
				avg_server_delay = (ushort)(0.7 * (double)(int)avg_server_delay + 0.3 * (double)(int)num);
			}
			return avg_server_delay;
		}

		public static ushort AvgNetDelay()
		{
			return (ushort)((double)(avg_server_delay + avg_client_delay) * 0.5);
		}

		public static ushort AvgServerDelay()
		{
			return avg_server_delay;
		}

		public static void SetClientDelay(ushort nDelayMs)
		{
			avg_client_delay = nDelayMs;
		}

		public static ushort AvgClientDelay()
		{
			return avg_client_delay;
		}
	}
}
