using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace UdpAdapterClient
{
	public class udp_packet
	{
		public _udp_head_t m_head = new _udp_head_t();

		public List<byte[]> m_data_list;

		public static byte checksum_key = 53;

		public udp_packet()
			: this(0, 0)
		{
		}

		public udp_packet(byte __flag, ushort __seq)
		{
			m_data_list = new List<byte[]>();
			reset(__flag, __seq);
		}

		public udp_packet(byte[] __buf, int dataLength)
			: this()
		{
			unserialize(__buf, dataLength);
		}

		public void reset(byte __flags, ushort __seq)
		{
			m_head.m_flags = __flags;
			m_head.m_seq = __seq;
		}

		public byte[] serialize()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter _writer = new BinaryWriter(memoryStream);
			m_head.m_data_count = (byte)m_data_list.Count();
			m_head.m_time_stamp = udp_time.GameNow();
			m_head.m_net_delay = udp_time.AvgServerDelay();
			m_head.to_bytes(_writer);
			writeUserData(_writer);
			return memoryStream.ToArray();
		}

		public int unserialize(byte[] __buf, int dataLenth)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(__buf));
			try
			{
				m_head.from_bytes(binaryReader);
				for (int i = 0; i < m_head.m_data_count; i++)
				{
					int count = binaryReader.ReadUInt16();
					byte[] _buf = binaryReader.ReadBytes(count);
					push_data(_buf);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			finally
			{
				binaryReader.Close();
			}
			return 0;
		}

		public int get_data_len()
		{
			int num = 0;
			foreach (byte[] item in m_data_list)
			{
				num += item.Length;
			}
			return num;
		}

		public void writeUserData(BinaryWriter __writer)
		{
			foreach (byte[] item in m_data_list)
			{
				__writer.Write((ushort)item.Length);
				__writer.Write(item);
			}
		}

		public int push_data(byte[] __buf)
		{
			udp_define.assert(__buf != null && __buf.Length <= 65000);
			int num = __buf.Length + 2;
			m_data_list.Add(__buf);
			m_head.m_data_size += (uint)num;
			return num;
		}

		public void update_send_info()
		{
			m_head.m_time_stamp = udp_time.GameNow();
		}

		public static byte checksum(byte[] __buf, int length)
		{
			byte b = checksum_key;
			int num = length - 1;
			for (int i = 0; i < num; i++)
			{
				b ^= __buf[i + 1];
			}
			return b;
		}

		public static void setChecksumKey(byte sumkey)
		{
			checksum_key = sumkey;
		}

		public void print_packet(IPEndPoint __sock_addr, bool bRev = false)
		{
			string text = null;
			text = ((!bRev) ? "[S]" : "[R]");
			string text2 = null;
			if (m_head.m_flags == 0)
			{
				text2 = "REALIABLE|";
			}
			if (m_head.m_flags == 1)
			{
				text2 = "RESPONSE|";
			}
			if (m_head.m_flags == 2)
			{
				text2 = "REQUEST|";
			}
			if (m_head.m_flags == 3)
			{
				text2 = "UNREALIABLE|";
			}
			if (m_head.m_flags == 4)
			{
				text2 = "SYNCTIME|";
			}
			if (m_head.m_flags == 5)
			{
				text2 = "CHECKSUM_ERR|";
			}
			if (m_head.m_flags == 6)
			{
				text2 = "SYNCRELIABLE|";
			}
			if (m_head.m_flags == 7)
			{
				text2 = "SYNCRESPONSE|";
			}
			if (m_head.m_flags == 8)
			{
				text2 = "HEARTBEAT|";
			}
			if (m_head.m_flags == 9)
			{
				text2 = "CLOSE|";
			}
			udp_define.printf("PACKET_DATA:{0}| [flag] : {1} [seq] : {2} ; [m_time_stamp] : {3} ; [m_net_delay] : {4} ", text, text2, m_head.m_seq, m_head.m_time_stamp, m_head.m_net_delay);
		}
	}
}
