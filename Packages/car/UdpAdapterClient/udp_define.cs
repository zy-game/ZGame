using System;
using UnityEngine;

namespace UdpAdapterClient
{
	public class udp_define
	{
		public const int MAX_BUF_SIZE = 65535;

		public const int UDP_DATA_MAX = 65000;

		public const int UDP_BATCH_SEND_DATA = 240;

		public const int UDP_RETRY_ERROR_TIMES = 3;

		public const long MS_PER_SEC = 1000L;

		public const long NS_PER_MS = 1000000L;

		public const long NS_PER_SEC = 1000000000L;

		public const long UDP_SYN_RTO_NS = 3000000000L;

		public const long UDP_RTO_MIN_NS = 300000000L;

		public const long UDP_RTO_MAX_NS = 6000000000L;

		public const int UDP_SEND_TYPE_RELIABLE = 0;

		public const int UDP_SEND_TYPE_UNRELIABLE = 1;

		public const int UDP_FLAG_REALIABLE = 0;

		public const int UDP_FLAG_RESPONSE = 1;

		public const int UDP_FLAG_REQUEST = 2;

		public const int UDP_FLAG_UNREALIABLE = 3;

		public const int UDP_FLAG_SYNCTIME = 4;

		public const int UDP_FLAG_CHECKSUM_ERR = 5;

		public const int UDP_FLAG_SYNCRELIABLE = 6;

		public const int UDP_FLAG_SYNCRESPONSE = 7;

		public const int UDP_FLAG_HEARTBEAT = 8;

		public const int UDP_FLAG_CLOSE = 9;

		public const int UDP_HEARTBEAT_TIME = 2000;

		public const int UDP_CONN_OVER_TIME = 20000;

		public const int UDP_OPT_TIMESTAMP = 1;

		public const int UDP_OPT_TIMESTAMP_REPLY = 2;

		public const int SYNC_TIME_STEP_1 = 1;

		public const int SYNC_TIME_STEP_2 = 2;

		public const int SYNC_TIME_STEP_END = 3;

		public const int UDP_ADAPTER_SESSION_OPEN = 1;

		public const int UDP_ADAPTER_SESSION_CLOSE = 2;

		public static bool has_one_flag(int __val, int __flags)
		{
			return (__val & __flags) != 0;
		}

		public static bool has_flag(int __val, int __flags)
		{
			return __val == __flags;
		}

		public static bool before(ushort __seq1, ushort __seq2)
		{
			return (short)(__seq1 - __seq2) < 0;
		}

		public static bool after(ushort __seq2, ushort __seq1)
		{
			return before(__seq1, __seq2);
		}

		public static ushort next_seq(ushort seq)
		{
			return (ushort)((seq == ushort.MaxValue) ? 1u : ((uint)(seq + 1)));
		}

		public static long datatime_to_ns(DateTime __dt)
		{
			return __dt.Ticks * 100;
		}

		public static long timespan_to_ns(TimeSpan __ts)
		{
			return __ts.Ticks * 100;
		}

		public static long datetime_to_ms(DateTime __dt)
		{
			return (long)((double)__dt.Ticks * 0.0001);
		}

		public static void assert(bool __condition)
		{
		}

		public static void printf(string __format, params object[] __arg_list)
		{
			Debug.Log("printf(string __format, params object[] __arg_list)");
		}

		public static bool isMultiThreading()
		{
			return true;
		}
	}
}
